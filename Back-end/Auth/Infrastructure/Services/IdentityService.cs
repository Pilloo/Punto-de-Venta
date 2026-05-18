
using AuthModule.Core.Interfaces;
using AuthModule.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace AuthModule.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<IdentityService> _logger;
        private readonly ICryptoService _cryptoService;
        private readonly ECDsa _pubKey;
        private readonly ECDsa _privKey;
        private readonly SigningCredentials _signinCredentials;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public IdentityService(IConfiguration configuration, ICryptoService cryptoService, ILogger<IdentityService> logger, IRefreshTokenRepository refreshTokenRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _cryptoService = cryptoService;
            _refreshTokenRepository = refreshTokenRepository;

            _pubKey = _cryptoService.LoadEcdsaKey(_configuration["Jwt:PublicKeyPath"]!);
            _privKey = _cryptoService.LoadEcdsaKey(_configuration["Jwt:PrivateKeyPath"]!);
            _signinCredentials = new SigningCredentials(new ECDsaSecurityKey(_privKey), SecurityAlgorithms.EcdsaSha256);
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
        public async Task<string> GenerateRefreshTokenAsync(User user, CancellationToken ct)
        {
            try
            {
                Span<byte> randomBytes = stackalloc byte[64];
                RandomNumberGenerator.Fill(randomBytes);
                string base64Token = Convert.ToBase64String(randomBytes);

                RefreshToken tokenEntity = new RefreshToken
                {
                    UserId = user.Id,
                    Token = base64Token,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsRevoked = false,
                    CreatedAt = DateTime.UtcNow,
                };

                await _refreshTokenRepository.SaveRefreshTokenAsync(tokenEntity, ct);

                return base64Token;
            }
            catch (OperationCanceledException op)
            {
                _logger.LogInformation("Operation canceled!");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
