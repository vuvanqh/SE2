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

        if (!await roleManager.RoleExistsAsync("Manager"))
            await roleManager.CreateAsync(new ApplicationRole { Name=UserRoleOptions.Manager.ToString()});

        var user = await userManager.FindByEmailAsync("manager@pw.edu.pl");

        if (user == null)
        {
            user = new ApplicationUser
            {
                FirstName = "Manager",
                LastName = "Hehe",
                UserName = "hehe",
                Email = "manager@pw.edu.pl",
                FacultyId = Guid.Parse("ff8c5ad6-e743-4756-aaf9-7f56d686e57f"),
                EmailConfirmed = true
            };

            await userManager.CreateAsync(user, "Password123!");
            await userManager.AddToRoleAsync(user, "Manager");
        }

    }
}