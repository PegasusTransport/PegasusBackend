namespace PegasusBackend.Settings
{
    public class PasswordResetSettings
    {
        public string FrontendUrl { get; set; } = string.Empty;
        public int TokenLifetimeHours { get; set; } 
        public int MaxRequestsPerHour { get; set; } 
    }
}