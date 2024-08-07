using System.Reflection;
using System.Security.Claims;
using System.Text;
using Carter;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Server.Data;
using Server.Domain.Entities;
using Server.Helpers;
using Server.Services;
using Server.Services.Interfaces;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<ISecurityService, SecurityService>();
builder.Services.AddTransient<IMailService, MailService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*** FluentValidation configuration**/
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
builder.Services.AddIdentity<User, Role>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

/*** Reset password token configuration ***/
builder.Services.Configure<DataProtectionTokenProviderOptions>(option =>
    option.TokenLifespan = TimeSpan.FromHours(1));

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

/*** Authentication roles policies ***/
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AppConstants.UserRole, policy =>
        policy
            .RequireClaim(ClaimTypes.Email)
            .RequireClaim(ClaimTypes.NameIdentifier)
            .RequireClaim(ClaimTypes.Role)
            .RequireRole(AppConstants.UserRole))
    .AddPolicy(AppConstants.AdminRole, policy =>
        policy
            .RequireClaim(ClaimTypes.Email)
            .RequireClaim(ClaimTypes.NameIdentifier)
            .RequireClaim(ClaimTypes.Role)
            .RequireRole(AppConstants.AdminRole)
    );

/*** Swagger configuration ***/
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer Authorization with refresh token. Example: Bearer {accessToken}",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
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
