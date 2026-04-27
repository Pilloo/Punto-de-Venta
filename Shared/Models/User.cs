using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Models
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
