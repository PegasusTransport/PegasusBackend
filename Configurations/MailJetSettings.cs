namespace PegasusBackend.Configurations
{
    public sealed class MailJetSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = "info@pegasustransport.se";
        public string SenderName { get; set; } = "Pegasus Transport";
        public MailjetTemplates Templates { get; set; } = new();
    }

    public sealed class MailjetTemplates
    {
        public long Welcome { get; set; }
        public long PendingConfirmation { get; set; }
        public long BookingConfirmation { get; set; }
        public long ForgotPassword { get; set; }
        public long TwoFA { get; set; }
        public long Receipt { get; set; }
    }
}
