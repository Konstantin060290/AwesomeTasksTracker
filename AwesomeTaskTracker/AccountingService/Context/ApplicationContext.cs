using AccountingService.Models;
using Microsoft.EntityFrameworkCore;

namespace AccountingService.Context;

public sealed class ApplicationContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Bill> Bills { get; set; } = null!;
    public DbSet<Transaction> Transactions { get; set; } = null!;

    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
        Database.EnsureCreated();
    }
}