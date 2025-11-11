using Microsoft.EntityFrameworkCore;
using TKV.Model.DbModels;

namespace TKV.Model.DbbContext;

public class MyDbContext(DbContextOptions<MyDbContext> options) : DbContext(options)
{
    public DbSet<Request> Request { get; set; }
    public DbSet<Coverage> Coverage { get; set; }
    public DbSet<RequestType> RequestType { get; set; }
}