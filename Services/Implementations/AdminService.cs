using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.Implementations
{
    public class AdminService : IAdminService
    {
        public async Task<(bool Success, Object? objekt, string Massage)> adminServicesesAsync()
        {
            return (true, null, "Testet har lyckats");
        }
    }
}
