using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.DTOs.MapDTOs;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using PegasusBackend.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PegasusBackend.Tests.Services
{
    public class MapServiceTests
    {
        [Fact]
        public async Task GetRouteInfoAsync_ReturnsRouteData_WhenCoordinatesValid()
        {
            // Arrange
            var coordinates = new List<CoordinateDto>
            {
                new() { Latitude = 0, Longitude = 0 },
                new() { Latitude = 0, Longitude = 0 },
            };

            var jsonResponse = @"
            {
              ""status"": ""OK"",
              ""routes"": [
                {
                  ""legs"": [
                    {
                      ""start_address"": ""Stockholm, Sweden"",
                      ""end_address"": ""Västerås, Sweden"",
                      ""distance"": {
                        ""value"": 45000,
                        ""text"": ""45.0 km""
                      },
                      ""duration"": {
                        ""value"": 2700,
                        ""text"": ""45 mins""
                      }
                    }
                  ]
                }
              ]
            }";

        
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json"),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration
                .Setup(x => x["GoogleMaps:ApiKey"])
                .Returns("test-api-key");

            var mockLogger = new Mock<ILogger<MapService>>();

            var mockIHttpClientFactory = new Mock<IHttpClientFactory>();
            mockIHttpClientFactory
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mapService = new MapService(mockConfiguration.Object, mockIHttpClientFactory.Object, mockLogger.Object);

            // Act
            var result = await mapService.GetRouteInfoAsync(coordinates);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
        }
    }
}
