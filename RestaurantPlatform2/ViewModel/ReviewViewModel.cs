using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.ViewModels
{
    public class ReviewViewModel
    {
        [Required]
        public int DishId { get; set; }

        [Display(Name = "Оценка (1 до 5)")]
        [Required(ErrorMessage = "Оценката е задължителна.")]
        [Range(1, 5, ErrorMessage = "Оценката трябва да е между 1 и 5.")]
        public int Rating { get; set; }

        [Display(Name = "Коментар")]
        [Required(ErrorMessage = "Коментарът е задължителен.")]
        [StringLength(500, ErrorMessage = "Коментарът не може да надвишава 500 символа.")]
        public string Comment { get; set; }

        public string? DishName { get; set; } // по избор, само за показване в View
    }
}
