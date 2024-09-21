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
    public class OrderItemsController : ControllerBase
    {
        private readonly DBHelper _dbHelper;

        public OrderItemsController(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // GET: api/orderitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderItem>>> GetOrderItems()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var orderItems = await dbConnection.QueryAsync<OrderItem>("SELECT * FROM OrderItems");
                return Ok(orderItems);
            }
        }

        // GET: api/orderitems/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderItem>> GetOrderItem(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var orderItem = await dbConnection.QuerySingleOrDefaultAsync<OrderItem>("SELECT * FROM OrderItems WHERE OrderItemId = @Id", new { Id = id });
                if (orderItem == null)
                {
                    return NotFound();
                }
                return Ok(orderItem);
            }
        }

        // POST: api/orderitems
        [HttpPost]
        public async Task<ActionResult<OrderItem>> PostOrderItem(OrderItem orderItem)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "INSERT INTO OrderItems (OrderId, MenuItemId, Quantity, Price) VALUES (@OrderId, @MenuItemId, @Quantity, @Price); SELECT CAST(SCOPE_IDENTITY() as int)";
                var orderItemId = await dbConnection.QuerySingleAsync<int>(sqlQuery, orderItem);
                orderItem.OrderItemId = orderItemId; // تعيين ID عنصر الطلب
                return CreatedAtAction(nameof(GetOrderItem), new { id = orderItem.OrderItemId }, orderItem);
            }
        }

        // PUT: api/orderitems/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrderItem(int id, OrderItem orderItem)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "UPDATE OrderItems SET OrderId = @OrderId, MenuItemId = @MenuItemId, Quantity = @Quantity, Price = @Price WHERE OrderItemId = @Id";
                var affectedRows = await dbConnection.ExecuteAsync(sqlQuery, new { orderItem.OrderId, orderItem.MenuItemId, orderItem.Quantity, orderItem.Price, Id = id });
                if (affectedRows == 0)
                {
                    return NotFound();
                }
                return NoContent();
            }
        }

        // DELETE: api/orderitems/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrderItem(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "DELETE FROM OrderItems WHERE OrderItemId = @Id";
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
