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
    public class MenuItemsController : ControllerBase
    {
        private readonly DBHelper _dbHelper;

        public MenuItemsController(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // GET: api/menuitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var menuItems = await dbConnection.QueryAsync<MenuItem>("SELECT * FROM MenuItems");
                return Ok(menuItems);
            }
        }

        // GET: api/menuitems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var menuItem = await dbConnection.QuerySingleOrDefaultAsync<MenuItem>("SELECT * FROM MenuItems WHERE MenuItemId = @Id", new { Id = id });
                if (menuItem == null)
                {
                    return NotFound();
                }
                return Ok(menuItem);
            }
        }

        // POST: api/menuitems
        [HttpPost]
        public async Task<ActionResult<MenuItem>> PostMenuItem(MenuItem menuItem)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "INSERT INTO MenuItems (RestaurantId, Name, Description, Price) VALUES (@RestaurantId, @Name, @Description, @Price); SELECT CAST(SCOPE_IDENTITY() as int)";
                var menuItemId = await dbConnection.QuerySingleAsync<int>(sqlQuery, menuItem);
                menuItem.MenuItemId = menuItemId; // تعيين ID عنصر القائمة
                return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.MenuItemId }, menuItem);
            }
        }

        // PUT: api/menuitems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutMenuItem(int id, MenuItem menuItem)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "UPDATE MenuItems SET RestaurantId = @RestaurantId, Name = @Name, Description = @Description, Price = @Price WHERE MenuItemId = @Id";
                var affectedRows = await dbConnection.ExecuteAsync(sqlQuery, new { menuItem.RestaurantId, menuItem.Name, menuItem.Description, menuItem.Price, Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }

        // DELETE: api/menuitems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenuItem(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "DELETE FROM MenuItems WHERE MenuItemId = @Id";
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
