namespace DTOs.Auth
{
    public class UserDto
    {
        public Guid UserId { get; set; } = Guid.Empty;
        public string UserName { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = default;
    }
}
