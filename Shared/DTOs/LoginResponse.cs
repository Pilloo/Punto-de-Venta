namespace DTOs
{
    /// <summary>
    /// Represents the result of the Login command execution.
    /// </summary>
    public class LoginResponse
    {
        /// <value>The JWT token generated.</value>
        public string AccessToken { get; set; } = string.Empty;

        /// <value>The refresh token used to generate a new JWT token.</value>
        public string? RefreshToken { get; set; } = string.Empty;
    }
}
