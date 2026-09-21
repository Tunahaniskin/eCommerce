using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Catalog;
using ECommerce.API.Modules.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Infrastructure.Endpoints;
using FluentValidation;
using ECommerce.API.Infrastructure.Database.Interceptors;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı (PostgreSQL)
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor); // EF Core komut zincirine dahil et
});

// 2. RabbitMQ / MassTransit Yapılandırması
builder.Services.AddMassTransit(x =>
{
    x.AddConsumers(typeof(Program).Assembly);
    
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"], 
                 Convert.ToUInt16(builder.Configuration["RabbitMQ:Port"]), "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
        
        cfg.ConfigureEndpoints(context);
    });
});

// 3. Modüller ve Swagger Servisleri
builder.Services.AddCatalogModule();
builder.Services.AddOrderModule();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddSingleton<AuditInterceptor>(); // Interceptor'ı DI konteynerine ekle

var app = builder.Build();

// 4. Swagger Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => "E-Commerce Modular Monolith API is running!");

app.MapEndpoints(typeof(Program).Assembly);

app.Run();