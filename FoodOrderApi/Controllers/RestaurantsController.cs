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

        //[HttpGet("{id}/menuitems")]
        //public async Task<ActionResult<RestaurantWithMenuItems>> GetRestaurantWithMenuItems(int id)
        //{
        //    using (IDbConnection dbConnection = _dbHelper.Connection)
        //    {
        //        dbConnection.Open();
        //        var sqlQuery = @"
        //    SELECT r.*, m.*
        //    FROM Restaurants r
        //    LEFT JOIN MenuItems m ON r.RestaurantId = m.RestaurantId
        //    WHERE r.RestaurantId = @Id";

        //        var restaurantDictionary = new Dictionary<int, RestaurantWithMenuItems>();

        //        var result = await dbConnection.QueryAsync<Restaurant, MenuItem, RestaurantWithMenuItems>(
        //            sqlQuery,
        //            (restaurant, menuItem) =>
        //            {
        //                if (!restaurantDictionary.TryGetValue(restaurant.RestaurantId, out var restaurantWithMenuItems))
        //                {
        //                    restaurantWithMenuItems = new RestaurantWithMenuItems
        //                    {
        //                        Restaurant = restaurant,
        //                        MenuItems = new List<MenuItem>()
        //                    };
        //                    restaurantDictionary.Add(restaurant.RestaurantId, restaurantWithMenuItems); 
        //                }

        //                if (menuItem != null)
        //                {
        //                    restaurantWithMenuItems.MenuItems.Add(menuItem);
        //                }

        //                return restaurantWithMenuItems;
        //            },
        //            new { Id = id },
        //            splitOn: "MenuItemId" // يجب أن يكون هنا اسم العمود الذي يبدأ منه الكائن الثاني
        //        );

        //        if (!restaurantDictionary.Any())
        //        {
        //            return NotFound();
        //        }

        //        return Ok(restaurantDictionary.Values.First());
        //    }
        //}

        [HttpGet("{id}/menuitems")]
        public async Task<ActionResult<RestaurantWithMenuItems>> GetRestaurantWithMenuItems(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = @"
            SELECT r.RestaurantId, r.Name, r.Address, r.Phone, r.CreatedAt, 
                   m.MenuItemId, m.RestaurantId, m.Name AS MenuItemName, m.Description, m.Price, m.CreatedAt AS MenuItemCreatedAt
            FROM Restaurants r
            LEFT JOIN MenuItems m ON r.RestaurantId = m.RestaurantId
            WHERE r.RestaurantId = @Id";

                var restaurantDictionary = new Dictionary<int, RestaurantWithMenuItems>();

                var result = await dbConnection.QueryAsync<Restaurant, MenuItem, RestaurantWithMenuItems>(
                    sqlQuery,
                    (restaurant, menuItem) =>
                    {
                        if (!restaurantDictionary.TryGetValue(restaurant.RestaurantId, out var restaurantWithMenuItems))
                        {
                            restaurantWithMenuItems = new RestaurantWithMenuItems
                            {
                                RestaurantId = restaurant.RestaurantId,
                                Name = restaurant.Name,
                                Address = restaurant.Address,
                                Phone = restaurant.Phone,
                                CreatedAt = restaurant.CreatedAt,
                                MenuItems = new List<MenuItem>()
                            };
                            restaurantDictionary.Add(restaurant.RestaurantId, restaurantWithMenuItems);
                        }

                        if (menuItem != null)
                        {
                            restaurantWithMenuItems.MenuItems.Add(menuItem);
                        }

                        return restaurantWithMenuItems;
                    },
                    new { Id = id },
                    splitOn: "MenuItemId" // تأكد من أن هذا هو اسم العمود الذي يبدأ منه كائن MenuItem
                );

                if (!restaurantDictionary.Any())
                {
                    return NotFound();
                }

                return Ok(restaurantDictionary.Values.First());
            }


        }

        [HttpGet("all/menuitems")]
        public async Task<ActionResult<IEnumerable<RestaurantWithMenuItems>>> GetAllRestaurantsWithMenuItems()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = @"
            SELECT r.RestaurantId, r.Name, r.Address, r.Phone, r.CreatedAt, 
                   m.MenuItemId, m.RestaurantId , m.Name , m.Description, m.Price, m.CreatedAt
            FROM Restaurants r
            LEFT JOIN MenuItems m ON r.RestaurantId = m.RestaurantId";

                var restaurantDictionary = new Dictionary<int, RestaurantWithMenuItems>();

                var result = await dbConnection.QueryAsync<Restaurant, MenuItem, RestaurantWithMenuItems>(
                    sqlQuery,
                    (restaurant, menuItem) =>
                    {
                        if (!restaurantDictionary.TryGetValue(restaurant.RestaurantId, out var restaurantWithMenuItems))
                        {
                            restaurantWithMenuItems = new RestaurantWithMenuItems
                            {
                                RestaurantId = restaurant.RestaurantId,
                                Name = restaurant.Name,
                                Address = restaurant.Address,
                                Phone = restaurant.Phone,
                                CreatedAt = restaurant.CreatedAt,
                                MenuItems = new List<MenuItem>()
                            };
                            restaurantDictionary.Add(restaurant.RestaurantId, restaurantWithMenuItems);
                        }

                        if (menuItem != null)
                        {
                            restaurantWithMenuItems.MenuItems.Add(menuItem);
                        }

                        return restaurantWithMenuItems;
                    },
                    splitOn: "MenuItemId"
                );

                return Ok(restaurantDictionary.Values);//Ok(result);
                //return Ok(result);

            }
        }

        [HttpPost("Create")]
        public async Task<ActionResult> CreateRestaurant([FromBody] RestaurantCreateDto restaurantDto)
        {
            if (restaurantDto == null || restaurantDto.MenuItems == null || !restaurantDto.MenuItems.Any())
            {
                return BadRequest("Invalid restaurant data.");
            }
            var restaurantId = 0;
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();

                // إدخال بيانات المطعم
                var insertRestaurantQuery = @"
            INSERT INTO Restaurants (Name, Address, Phone)
            VALUES (@Name, @Address, @Phone);
            SELECT SCOPE_IDENTITY();"; // استخدم الحقل المناسب إذا كنت تستخدم SQL Server

                restaurantId = await dbConnection.ExecuteScalarAsync<int>(insertRestaurantQuery, restaurantDto);

                // إدخال الأصناف المرتبطة بالمطعم
                var insertMenuItemsQuery = @"
            INSERT INTO MenuItems (Name, Price, Description, RestaurantId)
            VALUES (@Name, @Price, @Description, @RestaurantId)"; // إضافة وصف الصنف

                foreach (var menuItem in restaurantDto.MenuItems)
                {
                    await dbConnection.ExecuteAsync(insertMenuItemsQuery, new
                    {
                        Name = menuItem.Name,
                        Price = menuItem.Price,
                        Description = menuItem.Description, // إضافة الوصف
                        RestaurantId = restaurantId
                    });
                }
            }

            return CreatedAtAction(nameof(CreateRestaurant), new { id = restaurantId }, restaurantDto);
        }

        [HttpPost("bulk")]
        public async Task<ActionResult> CreateRestaurants([FromBody] RestaurantsCreateDto restaurantsDto)
        {
            if (restaurantsDto == null || restaurantsDto.Restaurants == null || !restaurantsDto.Restaurants.Any())
            {
                return BadRequest("Invalid restaurant data.");
            }

            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();

                foreach (var restaurantDto in restaurantsDto.Restaurants)
                {
                    // إدخال بيانات المطعم
                    var insertRestaurantQuery = @"
                INSERT INTO Restaurants (Name, Address, Phone)
                VALUES (@Name, @Address, @Phone);
                SELECT SCOPE_IDENTITY();"; // استخدم SCOPE_IDENTITY() إذا كنت تستخدم SQL Server

                    var restaurantId = await dbConnection.ExecuteScalarAsync<int>(insertRestaurantQuery, restaurantDto);

                    // إدخال الأصناف المرتبطة بالمطعم
                    var insertMenuItemsQuery = @"
                INSERT INTO MenuItems (Name, Price, Description, RestaurantId)
                VALUES (@Name, @Price, @Description, @RestaurantId)";

                    foreach (var menuItem in restaurantDto.MenuItems)
                    {
                        await dbConnection.ExecuteAsync(insertMenuItemsQuery, new
                        {
                            Name = menuItem.Name,
                            Price = menuItem.Price,
                            Description = menuItem.Description,
                            RestaurantId = restaurantId
                        });
                    }
                }
            }

            return CreatedAtAction(nameof(CreateRestaurants), null, restaurantsDto);
        }

        [HttpGet("all-with-menu-items")]
        public async Task<ActionResult<IEnumerable<RestaurantWithMenuItems>>> GetAllRestaurantsWithMenuItemsPro()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<RestaurantWithMenuItems2, MenuItemInfo, RestaurantWithMenuItems2>(
                    "GetAllRestaurantsWithMenuItems",
                    (restaurant, menuItem) =>
                    {
                        if (menuItem != null)
                        {
                            restaurant.MenuItems.Add(menuItem);
                        }
                        return restaurant;
                    },
                    splitOn: "MenuItemId", // تقسيم على MenuItemId
                    commandType: CommandType.StoredProcedure
                );
                var groupedResult = result.GroupBy(r => r.RestaurantId)
                                 .Select(g =>
                                 {
                                     var restaurant = g.First();
                                     restaurant.MenuItems = g.SelectMany(x => x.MenuItems).ToList();
                                     return restaurant;
                                 })
                                 .ToList();

                return Ok(groupedResult);
                //return Ok(result.GroupBy(r => r.RestaurantId).Select(g => g.First()).ToList());
            }
        }

        [HttpGet("all-with-menu-items-F")]
        public async Task<ActionResult<IEnumerable<RestaurantWithMenuItems>>> GetAllRestaurantsWithMenuItemsF()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();

                var result = await dbConnection.QueryAsync<RestaurantWithMenuItems2, MenuItemInfo, RestaurantWithMenuItems2>(
                    "SELECT * FROM GetAllRestaurantsWithMenuItemsF()",
                    (restaurant, menuItem) =>
                    {
                        if (menuItem != null)
                        {
                            restaurant.MenuItems.Add(menuItem);
                        }
                        return restaurant;
                    },
                    splitOn: "MenuItemId"
                );

                var groupedResult = result.GroupBy(r => r.RestaurantId)
                                          .Select(g =>
                                          {
                                              var restaurant = g.First();
                                              restaurant.MenuItems = g.SelectMany(x => x.MenuItems).ToList();
                                              return restaurant;
                                          })
                                          .ToList();

                return Ok(groupedResult);
            }
        }
    }


}

