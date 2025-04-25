using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantPlatform.Models
{
    public class FavoriteDish
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual Microsoft.AspNetCore.Identity.IdentityUser User { get; set; }

        [Required]
        public int DishId { get; set; }

        [ForeignKey("DishId")]
        public virtual Dish Dish { get; set; }
    }
}
