using ShopSphere.Domain.Common;
using ShopSphere.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopSphere.Domain.Entities
{
    public class Order : BaseEntity
    {
        public string OrderNumber { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public OrderStatus Status { get; set; } = OrderStatus.Confirmed;
        public decimal TotalAmount { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
