using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace Client.Services
{
    public class AuthStateProvider(LocalStorageService localStorage) : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {

        }
    }
}
