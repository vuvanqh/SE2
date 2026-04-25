using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Infrastructure.Identity;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        if (!await roleManager.RoleExistsAsync(UserRoleOptions.Manager.ToString()))
            await roleManager.CreateAsync(new ApplicationRole { Name = UserRoleOptions.Manager.ToString() });



        if (!await roleManager.RoleExistsAsync(UserRoleOptions.Admin.ToString()))
            await roleManager.CreateAsync(new ApplicationRole { Name = UserRoleOptions.Admin.ToString() });

        var admin = await userManager.FindByEmailAsync("admin@pw.edu.pl");

        if (admin == null)
        {
            admin = new ApplicationUser
            {
                FirstName = "Admin",
                LastName = "Hehe",
                UserName = "heheAdmin",
                Email = "admin@pw.edu.pl",
                EmailConfirmed = true
            };
            Console.WriteLine("\nCreatingADmin\n");
            await userManager.CreateAsync(admin, "Password123!");
            await userManager.AddToRoleAsync(admin, UserRoleOptions.Admin.ToString());
        }

    }
}