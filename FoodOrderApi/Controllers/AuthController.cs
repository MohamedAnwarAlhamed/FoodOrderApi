using Duende.IdentityServer.Stores;
using FoodOrderApi.Models;
using FoodOrderApi.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FoodOrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        //[HttpPost("token")]
        //public IActionResult GenerateToken([FromBody] UserLogin login)
        //{
        //    // تحقق من بيانات الاعتماد (هنا يمكنك استخدام قاعدة بيانات أو أي منطق آخر)
        //    if (login.Username == "test" && login.Password == "password") // مثال بسيط
        //    {
        //        var claims = new[]
        //        {
        //            new Claim(ClaimTypes.Name, login.Username)
        //        };

        //        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Z5sE4l3pTg9pQj8u5l7qX8r3Tg9pQj8u5l7qX8r3Tg9pQj8uncjbjcsbdcmzxnciubdsiucb489375897345"));
        //        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        //        var token = new JwtSecurityToken(
        //            issuer: "YourIssuer",
        //            audience: "YourAudience",
        //            claims: claims,
        //            expires: DateTime.Now.AddSeconds(3),
        //            signingCredentials: creds);

        //        return Ok(new
        //        {
        //            token = new JwtSecurityTokenHandler().WriteToken(token)
        //        });
        //    }

        //    return Unauthorized();
        //}


        //private readonly IUserRepository _userRepository;

        //public AuthController(IUserRepository userRepository)
        //{
        //    _userRepository = userRepository;
        //}

        [HttpPost("token")]
        public IActionResult GenerateToken([FromForm] UserLogin login)
        {
            var clientId = HttpContext.Request.Form["client_id"];
            var clientSecret = HttpContext.Request.Form["client_secret"];

            if (clientId != "client_id" || clientSecret != "client_secret")
            {
                return Unauthorized(new { Message = "Invalid client credentials" });
            }

            // تحقق من بيانات الاعتماد (يمكنك استخدام قاعدة بيانات هنا)
            if (login.Username == "user1" && login.Password == "password1")
            {
                var token = GenerateJwtToken(login.Username);
                return Ok(new { token });
            }

            return Unauthorized();
        }
        
        
        private string GenerateJwtToken(string username)
        {
            // إعداد معلومات المستخدم
            var claims = new[]
            {
        new Claim(JwtRegisteredClaimNames.Sub, username),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

            // إعداد المفتاح السري
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("Z5sE4l3pTg9pQj8u5l7qX8r3Tg9pQj8u5l7qX8r3Tg9pQj8uncjbjcsbdcmzxnciubdsiucb489375897345")); // استخدم مفتاحًا قويًا
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // إنشاء الرمز
            var token = new JwtSecurityToken(
                issuer: "YourIssuer",
                audience: "YourAudience",
                claims: claims,
                expires: DateTime.Now.AddSeconds(9),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
