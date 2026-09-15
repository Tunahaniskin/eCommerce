using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Catalog;
using ECommerce.API.Modules.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. RabbitMQ / MassTransit Yapılandırması
builder.Services.AddMassTransit(x =>
{
    // İleride Consumer sınıflarımızı buraya kaydedeceğiz
    
    // Consumer'ı MassTransit'e kaydediyoruz
    x.AddConsumer<ECommerce.API.Modules.Catalog.Features.UpdateStock.OrderCreatedConsumer>();
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], 
                 Convert.ToUInt16(builder.Configuration["RabbitMQ:Port"]), "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// 3. Modüllerin Sisteme Kaydedilmesi
builder.Services.AddCatalogModule();
builder.Services.AddOrderModule();

var app = builder.Build();

app.MapGet("/", () => "E-Commerce Modular Monolith API is running!");

app.MapCatalogEndpoints();
app.MapOrderEndpoints(); 

app.Run();