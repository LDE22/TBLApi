using Microsoft.EntityFrameworkCore;
using TBLApi.Models;

namespace TBLApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<ServiceModel> Services { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<ActionLog> ActionLogs { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Chat> Chats { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<ModeratorStatistic> ModeratorStatistics { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Booking>()
        .HasOne(b => b.Service) // Связь один к одному или один ко многим
        .WithMany(s => s.Bookings)
        .HasForeignKey(b => b.ServiceId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
        .HasOne(r => r.Client)
        .WithMany()
        .HasForeignKey(r => r.ClientId)
        .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Specialist)
                .WithMany()
                .HasForeignKey(r => r.SpecialistId)
                .OnDelete(DeleteBehavior.Restrict);

            // Настройки для Chat
            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Sender)
                .WithMany()
                .HasForeignKey(c => c.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Chat>()
                .HasOne(c => c.Receiver)
                .WithMany()
                .HasForeignKey(c => c.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Favorite>()
        .HasOne(f => f.Client)
        .WithMany(c => c.Favorites)
        .HasForeignKey(f => f.ClientId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Service)
                .WithMany(s => s.Favorites)
                .HasForeignKey(f => f.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
