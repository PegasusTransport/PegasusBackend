namespace PegasusBackend.Helpers.MailjetHelpers
{
    public enum MailjetTemplateType
    {
        Welcome, //when u have created account
        ForgotPassword, //when u forget ur password
        TwoFA, //2FA
        PendingConfirmation, //When booking wihtout acc
        BookingConfirmation,//when booking is confirmed
        Receipt, //reciept
        DriverNewBooking // notification to all drivers when a booking is confirmed
        // need to implement customerWelcome, and Driver Welcome for registrering!
    }
}
