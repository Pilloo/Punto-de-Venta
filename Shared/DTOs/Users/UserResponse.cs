using Models;

namespace DTOs.Users
{
    public class UserResponse
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string? UserName { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = default;

        public static UserResponse FromEntity(User user) => new()
        {
            GivenName = user.GivenName,
            LastName = user.LastName,
            UserName = user.UserName,
            UserId = user.Id,
            IsActive = user.IsActive,
        };
    }
}