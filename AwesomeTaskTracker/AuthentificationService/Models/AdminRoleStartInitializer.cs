using Microsoft.AspNetCore.Identity;
using TasksTrackerService.WebConstants;

namespace AuthentificationService.Models;

public abstract class AdminRoleStartInitializer
{
    public static async void AdminStartInitialize(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        await AddAdminRoleToDb(roleManager);
        await AddAdminUser(userManager);
        await AddAdminRoleToUser(userManager, roleManager);
    }

    private static async Task AddAdminRoleToUser(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        var admin = userManager.Users.FirstOrDefault(u => u.Email == RoleNames.AdminEmail);
        var adminRole = roleManager.Roles.FirstOrDefault(r => r.Name == RoleNames.AdminRole);
        await userManager.AddToRoleAsync(admin!, adminRole!.NormalizedName!);
    }

    private static async Task AddAdminUser(UserManager<User> userManager)
    {
        var usersCount = userManager.Users.Count();
        if (usersCount > 0)
        {
            return;
        }

        var admin = new User
        {
            Id = 1,
            Email = RoleNames.AdminEmail,
            UserName = RoleNames.AdminEmail
        };

        await userManager.CreateAsync(admin, RoleNames.AdminEmail);
    }

    private static async Task AddAdminRoleToDb(RoleManager<Role> roleManager)
    {
        var adminRole = roleManager.Roles.ToList().FirstOrDefault(r => r.Name == RoleNames.AdminRole);

        if (adminRole is not null)
        {
            return;
        }

        var newId = 1;
        var users = roleManager.Roles.ToList();
        if (users.Count > 0)
        {
            newId = roleManager.Roles.ToList().Select(r => r.Id).Max() + 1;
        }

        var newAdmin = new Role
        {
            Id = newId,
            Name = RoleNames.AdminRole,
            ConcurrencyStamp = RoleNames.AdminRole,
            NormalizedName = RoleNames.AdminRole
        };

        await roleManager.CreateAsync(newAdmin);
    }
}