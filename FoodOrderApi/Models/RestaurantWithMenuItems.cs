namespace FoodOrderApi.Models
{
    //public class RestaurantWithMenuItems
    //{
    //    public required Restaurant Restaurant { get; set; }
    //    public List<MenuItem> MenuItems { get; set; }
    //}

    public class RestaurantWithMenuItems
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<MenuItem> MenuItems { get; set; }
    }

    public class RestaurantWithMenuItems2
    {
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public List<MenuItemInfo> MenuItems { get; set; } = new List<MenuItemInfo>();
    }

    public class MenuItemInfo
    {
        public int MenuItemId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }
}
