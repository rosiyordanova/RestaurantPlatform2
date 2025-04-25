using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public ICollection<Dish> Dishes { get; set; }
    }
}
