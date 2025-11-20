using Castle.Core.Logging;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
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
                new() { Latitude = 59.649762m, Longitude = 17.923781m },
                new() { Latitude = 59.293938m, Longitude = 18.083374m },
            };
            var jsonResponse = @"
                {
                  ""status"": ""OK"",
                  ""routes"": [
                    {
                      ""legs"": [
                        {
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
                ItExpr.IsAny<HttpRequestMessage>(), // Match any request
                ItExpr.IsAny<CancellationToken>()) // Match any cancellation token
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse),
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);


            var mockConfiguration = new Mock<IConfiguration>();
            var mockLogger = new Mock<ILogger<MapService>>();
            var mockIHttpClientFactory=  new Mock<IHttpClientFactory>();

            mockIHttpClientFactory
                .Setup(x => x.CreateClient(It.IsAny<string>()))
                .Returns(httpClient);

            var mapService = new MapService(mockConfiguration.Object, mockIHttpClientFactory.Object, mockLogger.Object);

            // Act


            // Assert
        }
    }
}
