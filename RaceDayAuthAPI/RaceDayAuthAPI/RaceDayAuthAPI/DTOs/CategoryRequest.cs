
using System.ComponentModel.DataAnnotations;

namespace RaceDayAuthAPI.DTOs
{
    public class CategoryRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string CategoryType { get; set; } = string.Empty;

        [Range(0, 120)]
        public int? MinimumAge { get; set; }

        [Range(0, 120)]
        public int? MaximumAge { get; set; }

        [Range(0.01, 10000)]
        public decimal Distance { get; set; }
    }
}
