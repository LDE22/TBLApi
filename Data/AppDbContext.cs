using Microsoft.EntityFrameworkCore;
using TBLApi.Controllers;
using TBLApi.Models;

namespace TBLApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ServiceModel> Services { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Связи для Message
            modelBuilder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany()
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ServiceModel>()
                .HasOne(s => s.Specialist)
                .WithMany()
                .HasForeignKey(s => s.SpecialistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
