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

        [HttpGet("OrderDetails/{id}")]
        public async Task<ActionResult<OrderDetails>> GetOrderDetails(int id)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = @"
                SELECT o.OrderId, o.TotalAmount, o.Status, o.OrderDate,
                       r.RestaurantId, r.Name AS RestaurantName, r.Address AS RestaurantAddress,
                       u.UserId, u.Username AS UserName, u.Email AS UserEmail
                FROM Orders o
                JOIN Restaurants r ON o.RestaurantId = r.RestaurantId
                JOIN Users u ON o.UserId = u.UserId
                WHERE o.OrderId = @Id";

                var orderDetails = await dbConnection.QueryAsync<OrderDetails, RestaurantInfo, UserInfo, OrderDetails>(
            sqlQuery,
            (order, restaurant, user) =>
            {
                order.User = user;
                order.Restaurant = restaurant;
                return order;
            },
            new { Id = id },
            splitOn: "RestaurantId,UserId" // تقسيم على UserId ثم RestaurantId
        );


                if (orderDetails == null)
                {
                    return NotFound();
                }
                return Ok(orderDetails);
            }
        }

        [HttpGet("OrderDetails/All")]
        public async Task<ActionResult<OrderDetails>> GetOrderDetailsAll()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = @"
                SELECT o.OrderId, o.TotalAmount, o.Status, o.OrderDate,
                       r.RestaurantId, r.Name AS RestaurantName, r.Address AS RestaurantAddress,
                       u.UserId, u.Username AS UserName, u.Email AS UserEmail
                FROM Orders o
                JOIN Restaurants r ON o.RestaurantId = r.RestaurantId
                JOIN Users u ON o.UserId = u.UserId";

                var orderDetails = await dbConnection.QueryAsync<OrderDetails, RestaurantInfo, UserInfo, OrderDetails>(
            sqlQuery,
            (order, restaurant, user) =>
            {
                order.User = user;
                order.Restaurant = restaurant;
                return order;
            },
            splitOn: "RestaurantId,UserId" // تقسيم على UserId ثم RestaurantId
        );


                if (orderDetails == null)
                {
                    return NotFound();
                }
                return Ok(orderDetails);
            }
        }

        [HttpGet("with-items")]
        public async Task<ActionResult<IEnumerable<OrderWithItems>>> GetOrdersWithItems()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = @"SELECT o.OrderId, o.TotalAmount, o.Status, o.OrderDate,
                   oi.OrderItemId, oi.MenuItemId, m.Name AS MenuItemName, oi.Price , oi.Quantity
            FROM Orders o
            LEFT JOIN OrderItems oi ON o.OrderId = oi.OrderId
            LEFT JOIN MenuItems m ON oi.MenuItemId = m.MenuItemId
            ";

                var orderDictionary = new Dictionary<int, OrderWithItems>();

                var result = await dbConnection.QueryAsync<OrderWithItems, OrderItemInfo, OrderWithItems>(
                    sqlQuery,
                    (order, orderItem) =>
                    {
                        if (!orderDictionary.TryGetValue(order.OrderId, out var orderWithItems))
                        {
                            orderWithItems = new OrderWithItems
                            {
                                OrderId = order.OrderId,
                                TotalAmount = order.TotalAmount,
                                Status = order.Status,
                                OrderDate = order.OrderDate,
                                OrderItems = new List<OrderItemInfo>()
                            };
                            orderDictionary.Add(order.OrderId, orderWithItems);
                        }

                        if (orderItem != null)
                        {
                            orderWithItems.OrderItems.Add(new OrderItemInfo
                            {
                                OrderItemId = orderItem.OrderItemId,
                                MenuItemId = orderItem.MenuItemId,
                                MenuItemName = orderItem.MenuItemName,
                                Price = orderItem.Price,
                                Quantity = orderItem.Quantity // إضافة قيمة الكمية
                            });
                        }

                        return orderWithItems;
                    },
                    splitOn: "OrderItemId" // تقسيم على OrderItemId
                );

                return Ok(orderDictionary.Values);
            }
        }
    }
}
