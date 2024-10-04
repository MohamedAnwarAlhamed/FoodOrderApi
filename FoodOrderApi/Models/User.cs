namespace FoodOrderApi.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class User2
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
       
    }

    public class User3
    {
        public string Username { get; set; }
        public string Token { get; set; }
    }
}
