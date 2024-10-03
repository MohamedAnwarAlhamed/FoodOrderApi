using FoodOrderApi.Models;
namespace FoodOrderApi.Repository
{
    public class UserRepository : IUserRepository
    {
        public Task<User2> GetUserAsync(string username)
        {
            var user = InMemoryData.Users.FirstOrDefault(u => u.Username == username);
            return Task.FromResult(user);
        }
    }

    public static class InMemoryData
    {
        public static List<User2> Users = new List<User2>
    {
        new User2 { Username = "user1", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password1") },
        new User2 { Username = "user2", PasswordHash = BCrypt.Net.BCrypt.HashPassword("password2") }
    };
    }
}
