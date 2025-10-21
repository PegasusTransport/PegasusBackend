using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PegasusBackend.Configurations;
using PegasusBackend.DTOs.MailjetDTOs;
using PegasusBackend.Helpers.MailjetHelpers;
using PegasusBackend.Helpers.StatusMapper;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestMailjetController : ControllerBase
    {
        private readonly IMailjetEmailService _mailjet;

        public TestMailjetController(IMailjetEmailService mailjet)
        {
            _mailjet = mailjet;
        }

        //  Note: that the fluent validation dosent kick in when the dto isnt from the [FromBody]!
        [HttpGet("testWelcomeMail")]
        public async Task<ActionResult<bool>> TestWelcomeRespons(string yourEmail,string verificationLink, string yourName) =>
            Generate.ActionResult(await _mailjet.SendEmailAsync(
                yourEmail,
                MailjetTemplateType.Welcome,
                new AccountWelcomeRequestDto
                {
                    Firstname = yourName,
                    VerificationLink = verificationLink,
                    ButtonName = MailjetButtonType.Verify
                },
                MailjetSubjects.Welcome));

        [HttpGet("testBookningConfirmationMail")]
        public async Task<ActionResult<bool>> TestBookingConfirmationRespons(
            string yourEmail,
            string firstName,
            string pickupAdress,
            string? stops,
            string destination,
            string pickupTime,
            decimal totalPrice) =>
            Generate.ActionResult(await _mailjet.SendEmailAsync(
                yourEmail,
                MailjetTemplateType.BookingConfirmation,
                new BookingConfirmationRequestDto
                {
                    Firstname = firstName,
                    PickupAddress = pickupAdress,
                    Stops = string.IsNullOrWhiteSpace(stops) ? "Inga stopp angivna!" : stops,
                    Destination = destination,
                    PickupTime = pickupTime,
                    TotalPrice = totalPrice

                },
                MailjetSubjects.BookingConfirmation));

        [HttpGet("testForgotPasswordMail")]
        public async Task<ActionResult<bool>> TestForgotPasswordMail(
            string yourEmail,
            string firstName,
            string resetLink) =>
            Generate.ActionResult(await _mailjet.SendEmailAsync(
                yourEmail,
                MailjetTemplateType.ForgotPassword,
                new ForgotPasswordRequestDto
                {
                    Firstname = firstName,
                    ResetLink = resetLink
                },
                MailjetSubjects.ForgotPassword));

        [HttpGet("testPendingConfirmationMail")]
        public async Task<ActionResult<bool>> TestPendingConfirmationMail(
        string yourEmail,
        string firstName,
        string pickupAddress,
        string? stops,
        string destination,
        string pickupTime,
        decimal totalPrice,
        string confirmationLink)
        => Generate.ActionResult(await _mailjet.SendEmailAsync(
            yourEmail,
            MailjetTemplateType.PendingConfirmation,
            new PendingConfirmationRequestDto
            {
                Firstname = firstName,
                PickupAddress = pickupAddress,
                Stops = string.IsNullOrWhiteSpace(stops) ? "Inga stopp angivna!" : stops,
                Destination = destination,
                PickupTime = pickupTime,
                TotalPrice = totalPrice,
                ConfirmationLink = confirmationLink
            },
            MailjetSubjects.PendingConfirmation));

        [HttpGet("testTwoFAMail")]
        public async Task<ActionResult<bool>> TestTwoFAMail(string yourEmail, string firstName, string code)
            => Generate.ActionResult(await _mailjet.SendEmailAsync(
                yourEmail,
                MailjetTemplateType.TwoFA,
                new TwoFARequestDto
                {
                    Firstname = firstName,
                    VerificationCode = code
                },
                MailjetSubjects.TwoFA));
    }
}

