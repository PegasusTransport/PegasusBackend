using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Helpers;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;
using System;
using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace PegasusBackend.Services.Implementations
{
    public class MapService(IConfiguration config) : IMapService
    {
        private readonly HttpClient _httpClient = new();
        private readonly string _key = config["GoogleMaps:ApiKey"]!;


        public async Task<ServiceResponse<RouteInfoDto>> GetRouteInfoAsync(List<CoordinateDto> coordinates)
        {
            try
            {
                if (coordinates == null || coordinates.Count < 2)
                    return ServiceResponse<RouteInfoDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "At least 2 coordinates are requared."
                    );

                var url = MapHelper.BuildDirectionsUrl(coordinates, _key);

                HttpResponseMessage result;
                try
                {
                    result = await _httpClient.GetAsync(url);
                }
                catch (HttpRequestException ex)
                {
                    return ServiceResponse<RouteInfoDto>.FailResponse(
                        HttpStatusCode.ServiceUnavailable,
                        $"Network error while calling Google Maps API: {ex.Message}"
                    );
                }

                if (!result.IsSuccessStatusCode)
                {
                    return ServiceResponse<RouteInfoDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Couldnt get ruttDestination from google api. Check Key and network."
                    );
                }

                string json;
                try
                {
                    json = await result.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    return ServiceResponse<RouteInfoDto>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        $"Failed to read response from API: {ex.Message}"
                    );
                }

                JsonDocument document;
                try
                {
                    document = JsonDocument.Parse(json);
                }
                catch (JsonException ex)
                {
                    return ServiceResponse<RouteInfoDto>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        $"Invalid JSON format from Google API: {ex.Message}"
                    );
                }

                using (document)
                {
                    if (document.RootElement.TryGetProperty("status", out var apiStatus) && apiStatus.GetString() != "OK")
                    {
                        var messange = document.RootElement.TryGetProperty("error_message", out var errorMessage)
                            ? errorMessage.GetString()
                            : "Directions API error.";

                        return ServiceResponse<RouteInfoDto>.FailResponse(
                            HttpStatusCode.BadRequest,
                            $"{apiStatus.GetString()}: {messange}"
                        );
                    }

                    if (!document.RootElement.TryGetProperty("routes", out var routes) || routes.GetArrayLength() == 0)
                    {
                        return ServiceResponse<RouteInfoDto>.FailResponse(
                            HttpStatusCode.NotFound,
                            "There was no rutt/way found. Please try again later. "
                        );
                    }

                    
                    RouteInfoDto routeInfo;
                    try
                    {
                        routeInfo = MapHelper.ParseRouteInfo(json);
                    }
                    catch (Exception ex)
                    {
                        return ServiceResponse<RouteInfoDto>.FailResponse(
                            HttpStatusCode.InternalServerError,
                            $"Failed to parse route info: {ex.Message}"
                        );
                    }

                    return ServiceResponse<RouteInfoDto>.SuccessResponse(
                        HttpStatusCode.OK,
                        routeInfo,
                        "Route data retrieved."
                    );
                }
            }
            catch (Exception ex)
            {
                return ServiceResponse<RouteInfoDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    $"Unexpected error: {ex.Message}"
                );
            }
        }

        // municipality is null. in the respons. maybe use postalcode i zoneHelper?
        public async Task<ServiceResponse<LocationInfoDto>> GetLocationDetailsAsync(CoordinateDto coordinateDto)
        {
            try
            {
                if (coordinateDto == null)
                    return ServiceResponse<LocationInfoDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Coordinate data is missing."
                    );

                var url = MapHelper.BuildGeocodeUrl(coordinateDto, _key);

                HttpResponseMessage result;
                try
                {
                    result = await _httpClient.GetAsync(url);
                }
                catch (HttpRequestException ex)
                {
                    return ServiceResponse<LocationInfoDto>.FailResponse(
                        HttpStatusCode.ServiceUnavailable,
                        $"Network error while calling Google Geocode API: {ex.Message}"
                    );
                }

                if (!result.IsSuccessStatusCode)
                {
                    return ServiceResponse<LocationInfoDto>.FailResponse(
                        HttpStatusCode.BadRequest,
                        "Couldnt get location details from google api. Check Key and network."
                    );
                }

                string json;
                try
                {
                    json = await result.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    return ServiceResponse<LocationInfoDto>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        $"Failed to read response content: {ex.Message}"
                    );
                }

                JsonDocument document;
                try
                {
                    document = JsonDocument.Parse(json);
                }
                catch (JsonException ex)
                {
                    return ServiceResponse<LocationInfoDto>.FailResponse(
                        HttpStatusCode.InternalServerError,
                        $"Invalid JSON response: {ex.Message}"
                    );
                }

                using (document)
                {
                    if (document.RootElement.TryGetProperty("status", out var apiStatus) && apiStatus.GetString() != "OK")
                    {
                        var messange = document.RootElement.TryGetProperty("error_message", out var errorMessage)
                            ? errorMessage.GetString()
                            : "Geocoding API error.";

                        return ServiceResponse<LocationInfoDto>.FailResponse(
                            HttpStatusCode.BadRequest,
                            $"{apiStatus.GetString()}: {messange}"
                        );
                    }

                    if (!document.RootElement.TryGetProperty("results", out var results) || results.GetArrayLength() == 0)
                    {
                        return ServiceResponse<LocationInfoDto>.FailResponse(
                            HttpStatusCode.NotFound,
                            "No address found for the provided coordinates."
                        );
                    }

                    // Getting the first result in google api witch is the most relevant
                    var firstResult = results[0];

                    string? formattedAddress = null;

                    try
                    {
                        formattedAddress = firstResult.GetProperty("formatted_address").GetString();
                    }
                    catch { }

                    string? city = null;
                    string? municipality = null;
                    string? postalCode = null;

                    if (firstResult.TryGetProperty("address_components", out var components))
                    {
                        foreach (var component in components.EnumerateArray())
                        {
                            try
                            {
                                if (!component.TryGetProperty("types", out var types)) continue;

                                var typeList = types.EnumerateArray().Select(t => t.GetString()).ToList();

                                if (typeList.Contains("locality")) // City
                                    city = component.GetProperty("long_name").GetString();

                                if (typeList.Contains("administrative_area_level_2")) // Municipality
                                    municipality = component.GetProperty("long_name").GetString();

                                if (typeList.Contains("postal_code")) // Postal Code
                                    postalCode = component.GetProperty("long_name").GetString();
                            }
                            catch
                            {
                                continue;
                            }
                        }
                    }

                    var locationInfo = new LocationInfoDto
                    {
                        FormattedAddress = formattedAddress,
                        City = city,
                        Municipality = municipality,
                        PostalCode = postalCode,
                        Latitude = coordinateDto.Latitude,
                        Longitude = coordinateDto.Longitude
                    };

                    return ServiceResponse<LocationInfoDto>.SuccessResponse(
                        HttpStatusCode.OK,
                        locationInfo,
                        "Location details successfully retrieved."
                    );
                }
            }
            catch (Exception ex)
            {
                return ServiceResponse<LocationInfoDto>.FailResponse(
                    HttpStatusCode.InternalServerError,
                    $"Unexpected error: {ex.Message}"
                );
            }
        }
    }
}
