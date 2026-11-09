using System.ComponentModel.DataAnnotations;

namespace RaceDayAuthAPI.DTOs
{
    public class EventRequest
    {
        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        [Range(0.01, 10000)]
        public decimal Distance { get; set; }

        [Required]
        public string EventType { get; set; } = string.Empty;

        public string? BannerImageLink { get; set; }
    }
}