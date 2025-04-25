using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantPlatform.Models
{
    public enum OrderStatus
    {
        Изчакваща,
        ОбработваСе,
        Доставена,
        Отказана
    }

    public class Order
    {
        [ForeignKey("UserId")]
        public IdentityUser User { get; set; }

        public int Id { get; set; }

        public string UserId { get; set; }  // външен ключ към потребителя

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public OrderStatus Status { get; set; } = OrderStatus.Изчакваща;
        public ICollection<OrderItem> Items { get; set; }
        public bool IsNew { get; set; } = true;

    }
}
