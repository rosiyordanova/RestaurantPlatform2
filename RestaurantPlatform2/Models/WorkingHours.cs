using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models
{
    public class WorkingHours
    {
        public int Id { get; set; }

        [Required]
        public DayOfWeek Day { get; set; }

        [Required]
        public TimeSpan OpenTime { get; set; }

        [Required]
        public TimeSpan CloseTime { get; set; }
    }
}
