using AnalyticService.Models;
using Microsoft.EntityFrameworkCore;

namespace AnalyticService.Context;

public sealed class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}