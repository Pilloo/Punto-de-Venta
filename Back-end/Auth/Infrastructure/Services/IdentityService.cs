
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityService> _logger;
        private readonly ICryptoService _cryptoService;
        private readonly ECDsa _pubKey;
        private readonly ECDsa _privKey;
        private readonly TokenValidationParameters _validationParameters;
        private readonly SigningCredentials _signinCredentials;

        public IdentityService(IConfiguration configuration, ICryptoService cryptoService, ILogger<IdentityService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _cryptoService = cryptoService;

            _pubKey = _cryptoService.LoadEcdsaKey(_configuration["Jwt:PublicKeyPath"]!);
            _privKey = _cryptoService.LoadEcdsaKey(_configuration["Jwt:PrivateKeyPath"]!);
            _signinCredentials = new SigningCredentials(new ECDsaSecurityKey(_privKey), SecurityAlgorithms.EcdsaSha256);

            _validationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new ECDsaSecurityKey(_pubKey),
                ValidAlgorithms

            };
        }

        /// <summary>
        /// Generates a JSON Web Token (JWT) access token for the specified user.
        /// </summary>
        /// <remarks>The generated token includes the user's ID, given name, and last name as claims, and
        /// is valid for 30 minutes. The token is signed using the ECDSA algorithm with the configured private
        /// key.</remarks>
        /// <param name="user">The user for whom to generate the access token. Cannot be null.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the generated JWT access token
        /// as a string.</returns>
        public Task<string> GenerateAccessTokenAsync(User user)
        {
            Claim[] claims = new[]
            {
                    new Claim(JwtRegisteredClaimNames.GivenName, user.GivenName),
                    new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString())
            };

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = _signinCredentials
            };

            try
            {
                JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
                SecurityToken token = handler.CreateToken(tokenDescriptor);

                return Task.FromResult(handler.WriteToken(token));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Generates a cryptographically secure refresh token as a Base64-encoded string.
        /// </summary>
        /// <returns>A Base64-encoded string representing a securely generated refresh token.</returns>
        public async Task<string> GenerateRefreshTokenAsync()
        {
            Span<byte> randomBytes = stackalloc byte[64];

            RandomNumberGenerator.Fill(randomBytes);

            return await Task.FromResult(Convert.ToBase64String(randomBytes));
        }
    }
}
