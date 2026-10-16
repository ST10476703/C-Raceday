
using System.ComponentModel.DataAnnotations;

namespace RaceDayAuthAPI.DTOs
{
    public class EnrolmentRequest
    {
        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
    }
}
