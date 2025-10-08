namespace PegasusBackend.Helpers.MailjetHelpers
{
    public enum MailjetTemplateType
    {
        Welcome, //when u have created account
        ResetPassword, //when u forget ur password
        TwoFA, //2FA
        CreatedAccount, //when we create acc for u
        PendingBooking, //When booking wihtout acc
        BookingConfirmation,//when booking is confirmed
        Receipt //reciept
    }
}
