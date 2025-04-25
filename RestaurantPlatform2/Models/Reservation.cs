    using Microsoft.AspNetCore.Identity;
    using System.ComponentModel.DataAnnotations;

    namespace RestaurantPlatform.Models
    {
        public enum ReservationStatus
        {
            Изчакваща,
            Одобрена,
            Отказана
        }

        public class Reservation
        {
            public int Id { get; set; }

            [Required]
            public string UserId { get; set; }
            public IdentityUser User { get; set; }

            [Required]
            [DataType(DataType.Date)]
            public DateTime Date { get; set; }

            [Required]
            [DataType(DataType.Time)]
            public TimeSpan Time { get; set; }

            [Range(1, 20)]
            public int NumberOfGuests { get; set; }

            [StringLength(500)]
            public string Note { get; set; }

            public ReservationStatus Status { get; set; } = ReservationStatus.Изчакваща;

            public DateTime CreatedAt { get; set; } = DateTime.Now;
            public bool IsNew { get; set; } = true;

        }
    }
