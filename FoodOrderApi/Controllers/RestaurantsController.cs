using Microsoft.AspNetCore.Mvc;
using Dapper;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using FoodOrderApi.Models;
namespace FoodOrderApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly DBHelper _dbHelper;

        public RestaurantsController(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // GET: api/restaurants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var restaurants = await dbConnection.QueryAsync<Restaurant>("SELECT * FROM Restaurants");
                return Ok(restaurants);
            }
        }

        // GET: api/restaurants/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Restaurant>> GetRestaurant(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var restaurant = await dbConnection.QuerySingleOrDefaultAsync<Restaurant>("SELECT * FROM Restaurants WHERE RestaurantId = @Id", new { Id = id });
                if (restaurant == null)
                {
                    return NotFound();
                }
                return Ok(restaurant);
            }
        }

        // POST: api/restaurants
        [HttpPost]
        public async Task<ActionResult<Restaurant>> PostRestaurant(Restaurant restaurant)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "INSERT INTO Restaurants (Name, Address, Phone) VALUES (@Name, @Address, @Phone); SELECT CAST(SCOPE_IDENTITY() as int)";
                var restaurantId = await dbConnection.QuerySingleAsync<int>(sqlQuery, restaurant);
                restaurant.RestaurantId = restaurantId; // تعيين ID المطعم
                return CreatedAtAction(nameof(GetRestaurant), new { id = restaurant.RestaurantId }, restaurant);
            }
        }

        // PUT: api/restaurants/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRestaurant(int id, Restaurant restaurant)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "UPDATE Restaurants SET Name = @Name, Address = @Address, Phone = @Phone WHERE RestaurantId = @Id";
                var affectedRows = await dbConnection.ExecuteAsync(sqlQuery, new { restaurant.Name, restaurant.Address, restaurant.Phone, Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }

        // DELETE: api/restaurants/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRestaurant(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "DELETE FROM Restaurants WHERE RestaurantId = @Id";
                var affectedRows = await dbConnection.ExecuteAsync(sqlQuery, new { Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }
    }
}
