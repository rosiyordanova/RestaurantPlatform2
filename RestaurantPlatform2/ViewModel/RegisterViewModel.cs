using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне {2} символа.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Паролите не съвпадат.")]
        [Display(Name = "Потвърди парола")]
        public string ConfirmPassword { get; set; }
    }
}
