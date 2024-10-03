using FoodOrderApi.Models;

namespace FoodOrderApi.Repository
{
    public interface IUserRepository
    {
        Task<User2> GetUserAsync(string username);
    }
}
