using PegasusBackend.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace PegasusBackend.Tests.Services
{
    public class ServiceResponse
    {
        [Fact]
        public void ServiceResponse_Success_ReturnsCorrectData()
        {
            // Arrange
            var data = "test";

            // Act
            var response = ServiceResponse<string>.SuccessResponse(
                HttpStatusCode.OK,
                data
            );

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(data, response.Data);
        }

        [Fact]
        public void ServiceResponse_Fail_ReturnsNoData()
        {
            // Act
            var response = ServiceResponse<string>.FailResponse(
                HttpStatusCode.BadRequest,
                "Error"
            );

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Null(response.Data);
        }
    }
}
