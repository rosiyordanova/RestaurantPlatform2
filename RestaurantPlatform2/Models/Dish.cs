using RestaurantPlatform.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models
{
    public class Dish
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        public string ImageUrl { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [NotMapped]
        public IFormFile ImageFile { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsNew { get; set; } = false;
        public bool IsAvailable { get; set; } = true;
        public Season Season { get; set; } = Season.Всесезонно;

        // Добави Reviews (ако е необходимо)
        public ICollection<Review> Reviews { get; set; }
    }
}
