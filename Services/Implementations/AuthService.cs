using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using PegasusBackend.DTOs.AuthDTOs;
using PegasusBackend.Models;
using PegasusBackend.Responses;
using PegasusBackend.Services.Interfaces;

namespace PegasusBackend.Services.Implementations
{
    public class AuthService(UserManager<User> userManager): IAuthService
    {
        public Task<TokenResponse?> LoginAsync(LoginReguest request)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse?> RefreshTokensAsync(RefreshTokenRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
