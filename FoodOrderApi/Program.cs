using FoodOrderApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Data;
using System.Security.Claims;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddSingleton(new DBHelper(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IDbConnection>(sp => new DBHelper(builder.Configuration.GetConnectionString("DefaultConnection")).Connection);


// Configure Basic Authentication
//builder.Services.AddAuthentication("BasicAuthentication")
//    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);


// Configure JWT Bearer Authentication
var key = builder.Configuration["Jwt:Key"]; // ÊÃßÏ ãä ÅÖÇÝÉ åÐÇ Ýí appsettings.json
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Food Order API", Version = "v1" });

    // ÅÖÇÝÉ ãÚáæãÇÊ ÇáãÕÇÏÞÉ
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "íÑÌì ÅÏÎÇá ÑãÒ Bearer ÈÇÓÊÎÏÇã åÐÇ ÇáÔßá: 'Bearer {token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // ÇÓã ÇáÊÚÑíÝ ÇáÐí ÇÓÊÎÏãÊå Ýí AddSecurityDefinition
                }
            },
            new string[] {}
        }
    });
});

//builder.Services.AddSwaggerGen(




//    c =>
//{
//    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Food Order API", Version = "v1" });

//    // ÅÖÇÝÉ ãÚáæãÇÊ ÇáãÕÇÏÞÉ
//    c.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
//    {
//        In = ParameterLocation.Header,
//        Description = "íÑÌì ÅÏÎÇá ÈíÇäÇÊ ÇáÇÚÊãÇÏ ÈÇÓÊÎÏÇã ÊäÓíÞ Basic Ýí åÐÇ ÇáÔßá: 'Basic {base64encoded-credentials}'",
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
//                    Id = "Basic" // ÇÓã ÇáÊÚÑíÝ ÇáÐí ÇÓÊÎÏãÊå Ýí AddSecurityDefinition
//                }
//            },
//            new string[] {}
//        }
//    });
//}


//);




var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Food Order API V1");
    });
}

app.UseHttpsRedirection();

// Use Authentication and Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
