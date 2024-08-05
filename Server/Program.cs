using System.Reflection;
using System.Text;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Server.Data;
using Server.Domain.Entities;
using Server.Security;
using Server.Security.Interfaces;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISecurityService, SecurityService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*** FluentValidation files register ***/
builder.Services
    .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

/*** Database connection ***/
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite")));

/*** MediatR configuration ***/
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssembly(typeof(Program).Assembly));

/*** Carter configuration ***/
builder.Services.AddCarter();

/*** Configure Identity ***/
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>();

/*** Authentication configuration ***/
var configuration = builder.Configuration.GetSection("Authentication");
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidAudience = configuration.GetSection("Audience").Value,
            ValidIssuer = configuration.GetSection("Issuer").Value,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(configuration.GetSection("AccessTokenSecurityKey").Value!))
        };
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapCarter();

app.UseHttpsRedirection();

app.Run();
