namespace FoodOrderApi.Models
{
    public class OrderWithItems
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public List<OrderItemInfo> OrderItems { get; set; }
    }

    public class OrderItemInfo
    {
        public int OrderItemId { get; set; }
        public int MenuItemId { get; set; }
        public string MenuItemName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; } // إضافة كمية الطلب
    }

}
