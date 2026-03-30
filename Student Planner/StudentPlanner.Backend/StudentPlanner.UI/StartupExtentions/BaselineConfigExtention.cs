using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using StudentPlanner.Infrastructure;
using StudentPlanner.Infrastructure.IdentityEntities;
using System.Text.Json.Serialization;

namespace StudentPlanner.Backend;

public static class BaselineConfigExtention
{
    public static void ConfigureBaseline(this IServiceCollection services, IConfiguration config)
    {
        services.AddRouting(options =>
        {
            options.LowercaseUrls = true;
        });

        //cors
        services.AddCors(options =>
        {
            options.AddPolicy("AllowFrontend",
                policy =>
                {
                    policy
                        .WithOrigins(config.GetSection("AllowedOrigins").Get<string[]>()!)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
        });

        //controllers
        services.AddControllers(options =>
        {
            var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
            options.Filters.Add(new AuthorizeFilter(policy));
            options.Filters.Add(new ProducesAttribute("application/json"));
            options.Filters.Add(new ConsumesAttribute("application/json"));
        });

        //dbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("Default"), sqlOptions =>
            {
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorNumbersToAdd: null);
            });
            options.EnableSensitiveDataLogging();

        });

        //identity
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        //authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(config["Jwt:SecretKey"]!)),
                //ClockSkew = TimeSpan.Zero
            };
        });

        services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "StudentPlanner.UI.xml"));
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "StudentPlanner API",
                Version = "v1",
                Description = "API for managing university schedule and social events."
            });
            options.UseAllOfForInheritance();
            options.UseOneOfForPolymorphism();
        }); 

        //json
        services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        
        //authorization
        services.AddAuthorization();
    }
}
