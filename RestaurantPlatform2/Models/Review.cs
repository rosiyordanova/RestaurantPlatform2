using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models
{
    public class Review
    {
        public int Id { get; set; }

        [Required]
        public int DishId { get; set; }
        public Dish Dish { get; set; }  // навигация към Dish

        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(500)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Добавено поле за одобрение на рецензията
        public bool IsApproved { get; set; } = false; // по подразбиране е false
    }
}
