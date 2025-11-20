using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PegasusBackend.Data;
using PegasusBackend.Models;
using PegasusBackend.Repositorys.Implementations;
using PegasusBackend.Repositorys.Interfaces;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PegasusBackend.Tests.Services
{
    public class CarTests
    {
        [Fact]
        public async Task FindCarByRegNumberAsync_ReturnCar_WhenCarExist()
        {
            // arrange
            var testLicensePlate = "123456";

            var car = new Cars
            {
                Capacity = 0,
                LicensePlate = testLicensePlate,
                Make = "Volvo",
                Model = "K",
                CarId = 1,
                DriverIdFk = null
            };

            var options = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var mockLogger = new Mock<ILogger<CarRepo>>();

       
            using var context = new AppDBContext(options);
            context.Cars.Add(car);
            await context.SaveChangesAsync();
            var mockRepo = new CarRepo(context, mockLogger.Object);

            // act
            var result = await mockRepo.FindCarByRegNumberAsync(testLicensePlate);
            

            //Assert
            Assert.NotNull(result);
            Assert.Equal(testLicensePlate, result.LicensePlate);
        }
    }
}
