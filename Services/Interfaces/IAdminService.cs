namespace PegasusBackend.Services.Interfaces
{
    public interface IAdminService
    {
        Task<(bool Success, Object? objekt, string Massage)> adminServicesesAsync(); // Services ska returnera en tuple!
    }
}
