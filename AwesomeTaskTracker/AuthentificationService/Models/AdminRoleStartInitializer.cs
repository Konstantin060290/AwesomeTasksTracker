using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

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
        var admin = userManager.Users.FirstOrDefault(u => u.Email == WebConstants.WebConstants.AdminEmail);
        var adminRole = roleManager.Roles.FirstOrDefault(r => r.Name == WebConstants.WebConstants.AdminRole);
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
            Email = WebConstants.WebConstants.AdminEmail,
            UserName = WebConstants.WebConstants.AdminEmail
        };

        await userManager.CreateAsync(admin, WebConstants.WebConstants.AdminEmail);
    }

    private static async Task AddAdminRoleToDb(RoleManager<Role> roleManager)
    {
        var adminRole = roleManager.Roles.ToList().FirstOrDefault(r => r.Name == WebConstants.WebConstants.AdminRole);

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
            Name = WebConstants.WebConstants.AdminRole,
            ConcurrencyStamp = WebConstants.WebConstants.AdminRole,
            NormalizedName = WebConstants.WebConstants.AdminRole
        };

        await roleManager.CreateAsync(newAdmin);
    }
}