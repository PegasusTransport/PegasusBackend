using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface IMapService
    {
        Task<ServiceResponse<RouteInfoDto>> GetRouteInfoAsync(List<CoordinateDto> coordinates);
        Task<ServiceResponse<LocationInfoDto>> GetLocationDetailsAsync(CoordinateDto coordinateDto);
        Task<ServiceResponse<AutoCompleteResponseDto>> AutoCompleteAddreses(AutocompleteRequestDto request);
        Task<ServiceResponse<CoordinateDto>> GetCoordinatesByPlaceIdAsync(string placeIdRequestplaceId);
    }
}
