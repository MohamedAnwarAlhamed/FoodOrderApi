namespace FoodOrderApi.Models
{
    public class RestaurantWithOrders
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public List<OrderInfo> Orders { get; set; }
    }

    public class OrderInfo
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
    }
}
