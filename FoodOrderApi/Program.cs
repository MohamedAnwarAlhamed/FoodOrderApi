using FoodOrderApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using FoodOrderApi.Repository;
using Microsoft.AspNetCore.DataProtection;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton(new DBHelper(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbConnection>(sp => new DBHelper(builder.Configuration.GetConnectionString("DefaultConnection")).Connection);


 //Configure Basic Authentication
//builder.Services.AddAuthentication("BasicAuthentication")
//    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);


 //Configure JWT Bearer Authentication


//// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Food Order API", Version = "v1" });

//    // ≈÷«›… „⁄·Ê„«  «·„’«œﬁ…
//    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Ì—ÃÏ ≈œŒ«· —„“ Bearer »«” Œœ«„ Â–« «·‘ﬂ·: 'Bearer {token}'",
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "bearer"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Bearer" // «”„ «· ⁄—Ì› «·–Ì «” Œœ„ Â ›Ì AddSecurityDefinition
//                }
//            },
//            new string[] {}
//        }
//    });
//});

//builder.Services.AddSwaggerGen(




//    c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Food Order API", Version = "v1" });

//    // ≈÷«›… „⁄·Ê„«  «·„’«œﬁ…
//    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "Ì—ÃÏ ≈œŒ«· »Ì«‰«  «·«⁄ „«œ »«” Œœ«„  ‰”Ìﬁ Basic ›Ì Â–« «·‘ﬂ·: 'Basic {base64encoded-credentials}'",
//        Name = "Authorization",
//        Type = SecuritySchemeType.Http,
//        Scheme = "basic"
//    });

//    c.AddSecurityRequirement(new OpenApiSecurityRequirement
//    {
//        {
//            new OpenApiSecurityScheme
//            {
//                Reference = new OpenApiReference
//                {
//                    Type = ReferenceType.SecurityScheme,
//                    Id = "Basic" // «”„ «· ⁄—Ì› «·–Ì «” Œœ„ Â ›Ì AddSecurityDefinition
//                }
//            },
//            new string[] {}
//        }
//    });
//}


//);


// ≈÷«›… Œœ„«  IdentityServer
builder.Services.AddIdentityServer()
    .AddDeveloperSigningCredential()
    .AddInMemoryClients(new List<Client>
    {
        new Client
        {
            ClientId = "client_id",
            ClientSecrets =
            {
                new Duende.IdentityServer.Models.Secret("client_secret".Sha256())
            },
            AllowedGrantTypes = GrantTypes.ResourceOwnerPassword,
            AllowedScopes = { "FoodOrderApi" }
        }
    })
    .AddInMemoryApiResources(new List<ApiResource>
    {
        new ApiResource("FoodOrderApi", "Food Order API")
    })
    .AddInMemoryApiScopes(new List<ApiScope>
    {
        new ApiScope("FoodOrderApi", "Access to Food Order API")
    });

// ≈⁄œ«œ Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Food Order API", Version = "v1" });

    // ≈⁄œ«œ OAuth 2.0
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Password = new OpenApiOAuthFlow
            {
                TokenUrl = new Uri("https://localhost:44338/api/Auth/token", UriKind.Absolute),
                Scopes = new Dictionary<string, string>
                {
                    { "FoodOrderApi", "Access to the Food Order API" } // √÷› «·‰ÿ«ﬁ«  Â‰« ≈–« ·“„ «·√„—
                }
            }
        }
    });

    // ≈⁄œ«œ „ ÿ·»«  «·√„«‰
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { "FoodOrderAPI" }
        }
    });
});

var key = builder.Configuration["Jwt:Key"]; //  √ﬂœ „‰ ≈÷«›… Â–« ›Ì appsettings.json
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});
// »‰«¡ «· ÿ»Ìﬁ
var app = builder.Build();

app.UseIdentityServer();
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Order API V1");
});
app.UseHttpsRedirection();

app.UseRouting();
app.UseIdentityServer(); //  √ﬂœ „‰ √‰ IdentityServer Ì√ Ì √Ê·«
app.UseAuthentication(); //  √ﬂœ „‰ √‰ UseAuthentication ﬁ»· UseAuthorization
app.UseAuthorization();


app.MapControllers();

app.Run();
