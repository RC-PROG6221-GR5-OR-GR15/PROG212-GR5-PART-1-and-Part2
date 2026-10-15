using Microsoft.EntityFrameworkCore;
using Race_Day.Models;
namespace Race_Day.Data
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
    }
}
