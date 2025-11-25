namespace PegasusBackend.Configurations
{
    public sealed class MailJetSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string SecretKey { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public MailjetTemplates Templates { get; set; } = new();
        public MailjetLinks Links { get; set; } = new();
    }

    public sealed class MailjetTemplates
    {
        public long Welcome { get; set; }
        public long PendingConfirmation { get; set; }
        public long BookingConfirmation { get; set; }
        public long ForgotPassword { get; set; }
        public long TwoFA { get; set; }
        public long Receipt { get; set; }
        public long DriverNewBooking { get; set; }
        public long AssignedDriver { get; set; }
    }

    public sealed class MailjetLinks
    {
        public string LocalConfirmationBase { get; set; } = string.Empty;
        public string ProductionConfirmationBase { get; set; } = string.Empty;
        public string DriverPortalUrl { get; set; } = string.Empty;
    }
}
