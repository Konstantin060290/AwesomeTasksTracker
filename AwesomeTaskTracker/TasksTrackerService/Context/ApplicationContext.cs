using Microsoft.EntityFrameworkCore;
using TasksTrackerService.Models;

namespace TasksTrackerService.Context;

public sealed class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
 
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}