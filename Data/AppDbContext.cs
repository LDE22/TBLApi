using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using TBLApi.Models;

namespace TBLApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
    }
}
