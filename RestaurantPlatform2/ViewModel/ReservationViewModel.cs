using System.ComponentModel.DataAnnotations;

namespace RestaurantPlatform.ViewModels
{
    public class ReservationViewModel
    {
        [Required(ErrorMessage = "Моля, изберете дата.")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Моля, изберете час.")]
        [DataType(DataType.Time)]
        public TimeSpan Time { get; set; }

        [Required(ErrorMessage = "Моля, въведете брой гости.")]
        [Range(1, 20, ErrorMessage = "Броят на гостите трябва да е между 1 и 20.")]
        [Display(Name = "Брой гости")]
        public int? NumberOfGuests { get; set; }


        [StringLength(500, ErrorMessage = "Коментарът не може да е повече от 500 символа.")]
        [Display(Name = "Коментар (по избор)")]
        public string? Note { get; set; }
    }
}
