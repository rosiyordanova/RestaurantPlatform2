using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models
{
    public class ContactMessage
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(1000)]
        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.Now;
    }
}
