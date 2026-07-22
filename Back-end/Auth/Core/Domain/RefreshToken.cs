using Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthModule.Core.Domain
{
    public class RefreshToken
    {
        [Key] public Guid Id { get; set; }

        public Guid UserId { get; set; } = Guid.Empty;
        public virtual User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; } = DateTime.MinValue;

        public bool IsRevoked { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}