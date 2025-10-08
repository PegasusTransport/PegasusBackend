namespace PegasusBackend.Configurations
{
    public sealed class MailJetSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ApiSecret { get; set; } = string.Empty;
        public string FromEmail { get; set; } = "info@pegasustransport.se";
        public string FromName { get; set; } = "Pegasus Transport";

        // Templates
        public long TemplateId_Welcome { get; set; }
        public long TemplateId_ResetPassword { get; set; }
        public long TemplateId_2FA { get; set; }
        public long TemplateId_CreatedAccount { get; set; }
        public long TemplateId_PendingBooking { get; set; }
        public long TemplateId_BookingConfirmation { get; set; }
        public long TemplateId_Receipt { get; set; }
    }
}
