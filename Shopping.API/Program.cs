using Microsoft.Data.SqlClient;
using System.Data;
using Shopping.API.Data;
using Shopping.API.Repositories;
using Shopping.API.Repositories.Interfaces;
using Shopping.API.Services;
using Shopping.API.Services.Interfaces;
using Shopping.API.Data.Interfaces;

var builder = WebApplication.CreateBuilder(args);

ConfigureService(builder);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void ConfigureService(WebApplicationBuilder builder)
{
    builder.Services.AddTransient<IDbConnection>(provider =>
        new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddSingleton<IDbContext, DbContext>();
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
    });

    builder.Services.AddScoped<ICacheService, CacheService>();

    builder.Services.AddScoped<ICartRepository, CartRepository>();
    builder.Services.AddScoped<IOrderRepository, OrderRepository>();

    builder.Services.AddScoped<ICartService, CartService>();
    builder.Services.AddScoped<IPaymentService, PaymentService>();

    builder.Services.AddHttpClient<IPaymentService, PaymentService>(client =>
    {
        client.BaseAddress = new Uri("https://rendimentopay.free.beeceptor.com");
        client.Timeout = TimeSpan.FromSeconds(30);
    });
}