using Microsoft.EntityFrameworkCore;
using TKV.Model.DbModels;

namespace TKV.Model.DbContext;

public class MyDbContext(DbContextOptions<MyDbContext> options) : Microsoft.EntityFrameworkCore.DbContext(options)
{
    public DbSet<Request> Request { get; set; }
    public DbSet<Coverage> Coverage { get; set; }
    public DbSet<RequestType> RequestType { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Coverage>().HasData(
            new Coverage { Id = 1, Title = "Surgery", ProfitCoefficient = 0.0052 },
            new Coverage { Id = 2, Title = "Dentistry", ProfitCoefficient = 0.0042 },
            new Coverage { Id = 3, Title = "Hospitalization", ProfitCoefficient = 0.0050 }
        );
    }
}