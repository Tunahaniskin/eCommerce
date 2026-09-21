using ECommerce.API.Infrastructure.Database;
using ECommerce.API.Modules.Catalog;
using ECommerce.API.Modules.Order;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Infrastructure.Endpoints;
using FluentValidation;
using ECommerce.API.Infrastructure.Database.Interceptors;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ECommerce.API.Modules.Auth.Options;
using ECommerce.API.Modules.Auth.Services;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı ve Interceptor
builder.Services.AddSingleton<AuditInterceptor>();
builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    var auditInterceptor = sp.GetRequiredService<AuditInterceptor>();
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .AddInterceptors(auditInterceptor);
});

// 2. RabbitMQ / MassTransit
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

// 3. Modüller, Validation ve Swagger
builder.Services.AddCatalogModule();
builder.Services.AddOrderModule();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Token değerini kutuya yapıştırın (Başına 'Bearer ' yazmanıza gerek yok)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// 4. JWT ve Authentication Ayarları (BUILD'DEN ÖNCE OLMALI)
// Claim tip haritasını temizle - MUTLAKA AddAuthentication'dan önce olmalı!
System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.AddSingleton<JwtProvider>();

var jwtOptions = builder.Configuration.GetSection("JwtOptions").Get<JwtOptions>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions!.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
            
            // Map temizlendiğinde ClaimTypes.Role artık kısa "role" string'i olarak JWT'de kalır
            RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddAuthorization();


// ---------------- BUILD İŞLEMİ ----------------
var app = builder.Build();

// 5. Middleware Pipeline (BUILD'DEN SONRA)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => "E-Commerce Modular Monolith API is running!");

// Reflection ile Endpoint'leri Kaydet
app.MapEndpoints(typeof(Program).Assembly);

app.Run();