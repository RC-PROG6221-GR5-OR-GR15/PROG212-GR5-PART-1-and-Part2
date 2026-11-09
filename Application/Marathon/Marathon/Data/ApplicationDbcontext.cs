using Microsoft.EntityFrameworkCore;
using Marathon.Models;
namespace Marathon.Data
{
    public class ApplicationDbcontext: DbContext
    {
        public ApplicationDbcontext(DbContextOptions options): base (options)
        {
            
        }
        public DbSet<User> users { get; set; }
        public DbSet<Role> roles { get; set; }
        public DbSet<UserRole> userRoles { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Enrolment> enrolments { get; set; }
        public DbSet<Event> events { get; set; }
        

        public DbSet<Participant_Event> participant_Events { get; set; }
        public DbSet<Result> results { get; set; }
        public DbSet<UserAccount> accounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Money columns: match DECIMAL(10,2) in the database script
            modelBuilder.Entity<Category>().Property(c => c.EntryFee).HasPrecision(10, 2);
            modelBuilder.Entity<Enrolment>().Property(e => e.amount_paid).HasPrecision(10, 2);
            modelBuilder.Entity<Result>().Property(r => r.prize_money).HasPrecision(10, 2);
            modelBuilder.Entity<Event>().Property(e => e.EventFee).HasPrecision(10, 2);
            modelBuilder.Entity<Event>().Property(e => e.DistanceKm).HasPrecision(10, 2);
        }
    }
}
