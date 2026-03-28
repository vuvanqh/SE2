using Azure.Core;
using Microsoft.AspNetCore.Identity;
using StudentPlanner.Core.Application.Authentication;
using StudentPlanner.Core.Entities;
using StudentPlanner.Infrastructure;
using StudentPlanner.Infrastructure.IdentityEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudentPlanner.Infrastructure.Identity;

public class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;

    public IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<ApplicationRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }
    public async Task<User> SignInAsync(string email, string password)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email)) ?? throw new UnauthorizedAccessException("Invalid Credentials"); //i suppose we do not want to disclose whether an account associated with this email exists

        var result = await _signInManager.PasswordSignInAsync(user, password, isPersistent: false, lockoutOnFailure: true);
        if (!result.Succeeded)
            throw new UnauthorizedAccessException("Invalid Credentials");
        
        return user.ToUser(); 
    }

    public async Task RegisterUser(User user, string password, string? role = null)
    {
        ApplicationUser appUser = new ApplicationUser()
        {
            Id = user.Id,
            UserName = user.Email,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName
        };

        var result = await _userManager.CreateAsync(appUser, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new ApplicationException(errors);
        }

        if (role != null)
        {
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new ApplicationRole { Name = role });
            }
            await _userManager.AddToRoleAsync(appUser, role);
        }
    }

    public async Task<string> GeneratePasswordResetTokenAsync(string email)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email))?? throw new ApplicationException("Invalid Operation");
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task ResetPasswordAsync(string email, string token, string newPasswd)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email)) ?? throw new ApplicationException("Invalid Operation");

        var result = await _userManager.ResetPasswordAsync(user, token, newPasswd);
        if (!result.Succeeded)
        {
            var errors = string.Join("\n", result.Errors.Select(e => e.Description));
            throw new ApplicationException(errors);
        }
    }
    public async Task UpdateToken(string email, string tokenHash, DateTime expirationDate)
    {
        ApplicationUser user = (await _userManager.FindByEmailAsync(email)) ?? throw new ApplicationException("Invalid Operation");
        user.RefreshTokenHash = tokenHash;
        user.RefreshTokenExpirationDate = expirationDate;
        await _userManager.UpdateAsync(user);
    }
}
