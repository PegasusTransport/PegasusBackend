using Microsoft.Data.SqlClient;
using PegasusBackend.DTOs.CarDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface ICarService
    {
        Task<CarDto?> GetCarData(string regNo);
        Task<Cars?> CreateOrFindCarWithDriver(string regNo, Guid driverId);
    }
}
