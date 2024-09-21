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
    public class UsersController : ControllerBase
    {
        private readonly DBHelper _dbHelper;

        public UsersController(DBHelper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var users = await dbConnection.QueryAsync<User>("SELECT * FROM Users");
                return Ok(users);
            }
        }

        // POST: api/users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            using (IDbConnection dbConnection = _dbHelper.Connection)
            {
                dbConnection.Open();
                var sqlQuery = "INSERT INTO Users (Username, PasswordHash, Email) VALUES (@Username, @PasswordHash, @Email); SELECT CAST(SCOPE_IDENTITY() as int)";
                var userId = await dbConnection.QuerySingleAsync<int>(sqlQuery, user);
                user.UserId = userId; // تعيين ID المستخدم
                return CreatedAtAction(nameof(GetUsers), new { id = user.UserId }, user);
            }
        }
    }
}
