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

/// <summary>
/// Extension methods for configuring baseline services in the DI container.
/// </summary>
public static class BaselineConfigExtention
{
    /// <summary>
    /// Configures core baseline services including routing, CORS, controllers, DB context, identity, and authentication.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="config">The application configuration.</param>
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
                    var origins = config.GetSection("AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
                    policy
                        .WithOrigins(origins)
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
        if (Environment.GetEnvironmentVariable("USE_IN_MEMORY_DATABASE") == "true")
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                var dbName = Environment.GetEnvironmentVariable("IN_MEMORY_DATABASE_NAME") ?? "StudentPlanner";
                options.UseInMemoryDatabase(dbName);
            });
        }
        else
        {
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
        }

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
            var secretKey = config["Jwt:SecretKey"] ?? "default_secret_key_for_testing_purposes_only_1234567890";
            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(secretKey)),
                //ClockSkew = TimeSpan.Zero
            };
        });

        services.AddSwaggerGen(options =>
        {
            var xmlFile = Path.Combine(AppContext.BaseDirectory, "StudentPlanner.UI.xml");
            if (File.Exists(xmlFile))
            {
                options.IncludeXmlComments(xmlFile);
            }
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
