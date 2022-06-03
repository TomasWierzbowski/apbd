using System;

namespace zad5.Models
{
    public class Order
    {
        public int IdOrder { get; set; }
        public int IdProduct { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime FullfilledAt { get; set; } = DateTime.Now;
    }
}
