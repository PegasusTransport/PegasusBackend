namespace PegasusBackend.Configurations
{
    public class IdempotencySettings
    {
        public int ExpirationHours { get; set; } 
        public int CleanupIntervalHours { get; set; } 
    }
}