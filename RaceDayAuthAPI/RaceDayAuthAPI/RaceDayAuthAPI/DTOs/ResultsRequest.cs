
using System.ComponentModel.DataAnnotations;

namespace RaceDayAuthAPI.DTOs
{
    public class ResultRequest : IValidatableObject
    {
        [Required]
        public TimeSpan? FinishTime { get; set; }

        [Required]
        [Range(1, int.MaxValue)]
        public int? FinishPosition { get; set; }
        public IEnumerable<ValidationResult> Validate( ValidationContext validationContext)
        {
            if (FinishTime.HasValue &&
                (FinishTime.Value <= TimeSpan.Zero ||
                 FinishTime.Value >= TimeSpan.FromDays(1)))
            {
                yield return new ValidationResult(
                    "Finish time must be greater than zero and less than 24 hours.",
                    new[] { nameof(FinishTime) });
            }
        }
    }
}
