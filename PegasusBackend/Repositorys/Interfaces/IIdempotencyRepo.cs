using PegasusBackend.Models;

namespace PegasusBackend.Repositorys.Interfaces
{
    public interface IIdempotencyRepo
    {
        Task<IdempotencyRecord?> GetByKeyAsync(string key);
        Task<IdempotencyRecord> CreateAsync(IdempotencyRecord record);
        Task<int> DeleteExpiredAsync();
    }
}