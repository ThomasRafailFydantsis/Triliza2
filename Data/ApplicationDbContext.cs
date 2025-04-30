using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Runtime.ConstrainedExecution;
using triliza.Models;

namespace triliza.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

      
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Match>  PreSession { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<Session>(entity =>
            //{
            //    entity.HasKey(s => s.Id);



            //    entity.HasMany(c => c.MatchIds)
            //          .WithOne(q => q.Session)
            //          .HasForeignKey(q => q.SessionId)
            //          .OnDelete(DeleteBehavior.Cascade);

            //});

            builder.Entity<Match>(entity =>
            {
                entity.HasKey(s => s.Id);

                entity.HasOne(m => m.User)
                      .WithMany(u=>u.UserMatches)
                      .HasForeignKey(m => m.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                //entity.HasOne(uc => uc.Session)
                //      .WithMany()
                //      .HasForeignKey(uc => uc.SessionId)
                //      .OnDelete(DeleteBehavior.Cascade);

               
            });
        }

    }
}
