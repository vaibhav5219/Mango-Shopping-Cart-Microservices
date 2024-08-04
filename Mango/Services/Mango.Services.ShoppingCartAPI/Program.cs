using Microsoft.EntityFrameworkCore;
using Mango.Services.ShoppingCartAPI.Data;
using AutoMapper;
using Mango.Services.ShoppingCartAPI;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Mango.Services.ShoppingCartAPI.Extensions;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Mango.Services.ShoppingCartAPI.Service;
using Mango.Services.ShoppingCartAPI.Utility;
using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.RabbitMQSender;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

IMapper mapper = MappingConfig.RegisterMaps().CreateMapper();
builder.Services.AddSingleton(mapper);
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICouponService, CouponService>();
//builder.Services.AddScoped<IConfiguration, Configuration>();
//builder.Services.AddScoped<IMessageBus, MessageBus>();
builder.Services.AddScoped<IRabbitMQCartMessageSender, RabbitMQCartMessageSender>();
builder.Services.AddScoped<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient("Product", u=>u.BaseAddress = 
new Uri(builder.Configuration["ServiceUrls:ProductAPI"])).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();
builder.Services.AddHttpClient("Coupon", u=>u.BaseAddress = 
new Uri(builder.Configuration["ServiceUrls:CouponAPI"])).AddHttpMessageHandler<BackendApiAuthenticationHttpClientHandler>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>                //    For Adding token field in swagger
{
    option.AddSecurityDefinition(name: "Bearer", securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = JwtBearerDefaults.AuthenticationScheme
                }
            },
            new string[]{}
        }
    });
});

// Add Authentication so that without valid token no one can access coupon api => For authentication these settings are necessary
//var secret = builder.Configuration.GetValue<string>("ApiSettings:Secret");
builder.AddAppAuthentication();

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//ApplyMigration();

app.Run();


// Apply pending migration to database
void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var count = _db.Database.GetAppliedMigrations().Count();
        if (count > 0)
        {
            _db.Database.Migrate();
        }
    }
}