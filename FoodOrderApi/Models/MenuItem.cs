﻿namespace FoodOrderApi.Models
{
    public class MenuItem
    {
        public int MenuItemId { get; set; }
        public int RestaurantId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
