using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Services.Implementations;
using QuestPDF.Fluent;
using System.Net;
using System.Text;


namespace PegasusBackend.Tests.Services
{
    public class PdfServiceTestable : PdfService
    {
        public PdfServiceTestable(
            IConfiguration configuration,
            IHttpClientFactory httpClientFactory,
            ILogger<PdfService> logger)
            : base(configuration, httpClientFactory, logger)
        {
        }

        protected override byte[] GeneratePdf(Document document)
        {
            return Encoding.UTF8.GetBytes("fake-pdf-content");
        }
    }

    public class PdfServiceTests
    {
        [Fact]
        public async Task GenerateReceiptPdfAsync_ReturnsPdfBytes_WhenInputIsValid()
        {
            var configurationData = new Dictionary<string, string?>
            {
                { "PdfSettings:LogoUrl", "https://test.com/logo.png" },
                { "PdfSettings:CompanyName", "Pegasus Transport AB" },
                { "PdfSettings:CompanyEmail", "info@test.com" },
                { "PdfSettings:CompanyPhone", "08-1234567" },
                { "PdfSettings:HeaderColor", "#032240" },
                { "PdfSettings:LabelColor", "#1ea896" },
                { "PdfSettings:PriceColor", "#e5723a" },
                { "PdfSettings:DefaultTextColor", "#000000" },
                { "PdfSettings:BackgroundColor", "#ffffff" }
            };

            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configurationData)
                .Build();

            var mockHandler = new Mock<HttpMessageHandler>();

            mockHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new ByteArrayContent(Encoding.UTF8.GetBytes("fake-logo"))
                });

            var httpClient = new HttpClient(mockHandler.Object);

            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var logger = new Mock<ILogger<PdfService>>();

            var pdfService = new PdfServiceTestable(configuration, httpClientFactory.Object, logger.Object);

            var dto = new ReceiptRequestDto
            {
                CustomerFirstname = "John",
                PickupAddress = "Stockholm",
                Stops = "Lidingö",
                Destination = "Arlanda",
                PickupTime = DateTime.Now,
                DistanceKm = 45.0M,
                DurationMinutes = "00:45",
                TotalPrice = 599.0M,
                DriverFirstname = "Adam",
                LicensePlate = "ABC123",
                DriverImageUrl = null
            };

            var result = await pdfService.GenerateReceiptPdfAsync(dto);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            Assert.NotNull(result.Data);
            Assert.True(result.Data.Length > 0);
        }
    }
}
