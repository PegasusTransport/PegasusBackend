using Moq;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Interfaces;
using PegasusBackend.Services.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PegasusBackend.Tests.Services
{
    public class TaxiSettingsTest
    {
        [Fact]
        public async Task TaxiMeterPrice_ShouldCalculateCorrectPrice()
        {
            // Arrange:
            var faketaxi = new TaxiSettings
            {
                Id = 1,
                StartPrice = 100,
                KmPrice = 100,
                MinutePrice = 100,
            };
            var mockAdminRepo = new Mock<IAdminRepo>();

            mockAdminRepo.Setup(a => a.GetTaxiPricesAsync()).ReturnsAsync(faketaxi);

            var priceService = new PriceService(mockAdminRepo.Object);

            // Act
            var result = await priceService.TaxiMeterPrice(10, 10);

            // Assert:
            Assert.Equal(2100, (10 * faketaxi.KmPrice) + (10 * faketaxi.MinutePrice) + faketaxi.StartPrice);
        }
    }
}
