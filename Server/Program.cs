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
using Quartz;
using Quartz.AspNetCore;
using Server.BackgroundJobs;
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
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        
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
var authConfiguration = builder.Configuration.GetSection("Authentication");
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
            ValidAudience = authConfiguration.GetSection("Audience").Value,
            ValidIssuer = authConfiguration.GetSection("Issuer").Value,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(authConfiguration.GetSection("AccessTokenSecurityKey").Value!))
        };
    });

/*** Authentication roles policies ***/
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(Roles.User, policy =>
        policy
            .RequireClaim(ClaimTypes.Email)
            .RequireClaim(ClaimTypes.NameIdentifier)
            .RequireClaim(ClaimTypes.Role)
            .RequireRole(Roles.User))
    .AddPolicy(Roles.Admin, policy =>
        policy
            .RequireClaim(ClaimTypes.Email)
            .RequireClaim(ClaimTypes.NameIdentifier)
            .RequireClaim(ClaimTypes.Role)
            .RequireRole(Roles.Admin)
    );

/*** Background tasks configuration ***/
var backJobsConfiguration = builder.Configuration.GetSection("BackgroundJobs");
builder.Services.AddQuartz(options =>
{
    var jobKey = new JobKey(nameof(RefreshTokensCleanerBackgroundJob));
    options.AddJob<RefreshTokensCleanerBackgroundJob>(opts => opts.WithIdentity(jobKey));

    options.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(
            int.Parse(backJobsConfiguration.GetSection("ExpiredRefreshTokenCleaningTimeHour24").Value!),
            int.Parse(backJobsConfiguration.GetSection("ExpiredRefreshTokenCleaningTimeMinute60").Value!))
        )
    );
});
builder.Services.AddQuartzHostedService(options =>
    options.WaitForJobsToComplete = true);

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
    
    /*** Refresh database for dev and tests ***/
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetService<AppDbContext>()!;
    context.Database.EnsureDeleted();
    context.Database.EnsureCreated();
}

app.MapCarter();

app.UseHttpsRedirection();

app.Run();

public abstract partial class Program { } // config for tests
