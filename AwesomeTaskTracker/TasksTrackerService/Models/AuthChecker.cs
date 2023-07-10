using System.Runtime.Caching;
using TasksTrackerService.Context;
using TasksTrackerService.WebConstants;

namespace TasksTrackerService.Models;

public static class AuthChecker
{
    public static bool IsUserAuthenticated(ApplicationContext context, out string userRoleName, out string? userEmail)
    {
        bool isUserAuthenticated;
        userEmail = "";
        userRoleName = "";
        try
        {
            isUserAuthenticated = ((Dictionary<string, bool>)MemoryCache.Default.Get(EventsNames.UserAuthenticated)).FirstOrDefault()
                .Value;
            userEmail = ((Dictionary<string, bool>)MemoryCache.Default.Get(EventsNames.UserAuthenticated))
                .FirstOrDefault().Key;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }

        if (userEmail is not null)
        {
            var email = userEmail;
            var dbUser = context.Users.FirstOrDefault(u => u.Email == email);
            
            if (dbUser is not null)
            {
                userRoleName = dbUser.UserRoleName;
            }
        }

        return isUserAuthenticated;
    }
}