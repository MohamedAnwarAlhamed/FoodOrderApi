namespace FoodOrderApi.Models
{
    public class OrderDetails
    {
        public int OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }
        public RestaurantInfo Restaurant { get; set; }
        public UserInfo User { get; set; }
    }

    public class RestaurantInfo
    {
        public int RestaurantId { get; set; }
        public string RestaurantName { get; set; }
        public string RestaurantAddress { get; set; }
    }

    public class UserInfo
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
    }
}
