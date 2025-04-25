using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace RestaurantPlatform.Models
{
    public class OrderStatusLog
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime ChangedAt { get; set; }

        public string ChangedByUserId { get; set; }

        public IdentityUser ChangedByUser { get; set; }

        public Order Order { get; set; }
    }
}
