using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models
{
    public class SiteSetting
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Глобално съобщение")]
        [StringLength(500)]
        public string? GlobalMessage { get; set; }

        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}

