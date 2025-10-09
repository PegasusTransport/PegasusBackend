namespace PegasusBackend.Configurations
{
    public sealed class MailJetSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "info@pegasustransport.se";
        public string FromName { get; set; } = "Pegasus Transport";

        public MailjetTemplates mailjetTemplates = new();
    }

    public sealed class MailjetTemplates
    {
        public long Welcome { get; set; }
        public long PendingConfirmation { get; set; }
        public long BookingConfirmation { get; set; }
        public long ForgotPassword { get; set; }
        public long TwoFA { get; set; }
    }
}
