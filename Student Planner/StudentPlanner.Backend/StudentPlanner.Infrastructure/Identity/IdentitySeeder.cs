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

        var manager = await userManager.FindByEmailAsync("manager@pw.edu.pl");

        // if (manager == null)
        // {
        //     manager = new ApplicationUser
        //     {
        //         FirstName = "Manager",
        //         LastName = "Hehe",
        //         UserName = "hehe",
        //         Email = "manager@pw.edu.pl",
        //         FacultyId = Guid.Parse("ff8c5ad6-e743-4756-aaf9-7f56d686e57f"),
        //         EmailConfirmed = true
        //     };

        //     await userManager.CreateAsync(manager, "Password123!");
        //     await userManager.AddToRoleAsync(manager, UserRoleOptions.Manager.ToString());
        // }

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