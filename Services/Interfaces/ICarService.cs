using Microsoft.Data.SqlClient;
using PegasusBackend.DTOs.CarDTOs;
using PegasusBackend.Responses;

namespace PegasusBackend.Services.Interfaces
{
    public interface ICarService
    {
        Task<ServiceResponse<bool>> CreateCar(string regNo);
        Task<ServiceResponse<CarDto>> GetCarData(string regNo);
    }
}
