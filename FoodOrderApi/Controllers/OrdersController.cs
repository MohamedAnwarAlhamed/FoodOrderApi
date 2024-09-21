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
    public class OrdersController : ControllerBase
    {
        private readonly DBHelper _dbHelper;

        public OrdersController(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // GET: api/orders
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var orders = await dbConnection.QueryAsync<Order>("SELECT * FROM Orders");
                return Ok(orders);
            }
        }

        // GET: api/orders/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var order = await dbConnection.QuerySingleOrDefaultAsync<Order>("SELECT * FROM Orders WHERE OrderId = @Id", new { Id = id });
                if (order == null)
                {
                    return NotFound();
                }
                return Ok(order);
            }
        }

        // POST: api/orders
        [HttpPost]
        public async Task<ActionResult<Order>> PostOrder(Order order)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "INSERT INTO Orders (UserId, RestaurantId, TotalAmount, Status) VALUES (@UserId, @RestaurantId, @TotalAmount, @Status); SELECT CAST(SCOPE_IDENTITY() as int)";
                var orderId = await dbConnection.QuerySingleAsync<int>(sqlQuery, order);
                order.OrderId = orderId; // تعيين ID الطلب
                return CreatedAtAction(nameof(GetOrder), new { id = order.OrderId }, order);
            }
        }

        // PUT: api/orders/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Order order)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "UPDATE Orders SET UserId = @UserId, RestaurantId = @RestaurantId, TotalAmount = @TotalAmount, Status = @Status WHERE OrderId = @Id";
                var affectedRows = await dbConnection.ExecuteAsync(sqlQuery, new { order.UserId, order.RestaurantId, order.TotalAmount, order.Status, Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }

        // DELETE: api/orders/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "DELETE FROM Orders WHERE OrderId = @Id";
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
