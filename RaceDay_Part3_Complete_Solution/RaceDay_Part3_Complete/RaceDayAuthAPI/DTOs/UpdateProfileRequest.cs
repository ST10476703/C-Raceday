
using System.ComponentModel.DataAnnotations;

namespace RaceDayAuthAPI.DTOs
{
    public class UpdateProfileRequest
    {
        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Phone]
        public string? Phone { get; set; }

        [Url]
        public string? ProfileImageLink { get; set; }
    }
}
