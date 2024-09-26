namespace FoodOrderApi.Models
{
    public class RestaurantCreateDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public List<MenuItemDto> MenuItems { get; set; }
    }

    public class MenuItemDto
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
    }

    public class RestaurantsCreateDto
    {
        public List<RestaurantCreateDto> Restaurants { get; set; }
    }
}
