using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Models.Auth
{
    /// <summary>
    /// Represents a system user.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        [PersonalData]
        [MaxLength(256)]
        public string GivenName { get; set; } = null!;

        [PersonalData]
        [MaxLength(256)]
        public string LastName { get; set; } = null!;

        [Required]
        public bool IsActive { get; set; } = true;
    }
}
