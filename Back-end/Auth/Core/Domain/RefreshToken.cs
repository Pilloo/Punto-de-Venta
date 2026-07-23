using System.ComponentModel.DataAnnotations;
using Models.Auth;

namespace AuthModule.Core.Domain
{
    public class RefreshToken
    {
        [Key] public Guid Id { get; init; }

        public Guid UserId { get; init; } = Guid.Empty;
        public virtual User User { get; init; } = null!;

        public string Token { get; init; } = string.Empty;
        public DateTime ExpiresAt { get; init; } = DateTime.MinValue;

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; init; }
    }
}