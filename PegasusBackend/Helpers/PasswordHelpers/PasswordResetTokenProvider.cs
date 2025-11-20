using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PegasusBackend.Models;

namespace PegasusBackend.Helpers
{
    public class PasswordResetTokenProviderOptions : DataProtectionTokenProviderOptions
    {
    }

    public class PasswordResetTokenProvider : DataProtectorTokenProvider<User>
    {
        public PasswordResetTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<PasswordResetTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<User>> logger)
            : base(dataProtectionProvider, options, logger)
        {
        }
    }
}