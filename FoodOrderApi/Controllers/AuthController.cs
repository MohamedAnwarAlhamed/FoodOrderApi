using FoodOrderApi.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodOrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("token")]
        public IActionResult GenerateToken([FromBody] UserLogin login)
        {
            // تحقق من بيانات الاعتماد (هنا يمكنك استخدام قاعدة بيانات أو أي منطق آخر)
            if (login.Username == "test" && login.Password == "password") // مثال بسيط
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, login.Username)
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Z5sE4l3pTg9pQj8u5l7qX8r3Tg9pQj8u5l7qX8r3Tg9pQj8uncjbjcsbdcmzxnciubdsiucb489375897345"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "YourIssuer",
                    audience: "YourAudience",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(3),
                    signingCredentials: creds);

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token)
                });
            }

            return Unauthorized();
        }
    }

}
