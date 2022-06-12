using TournamentApp.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace TournamentApp.Data
{
    public class TournamentAppDbContext  : IdentityDbContext
    {
        public DbSet<Tournament> Tournaments { get; set; }
        public DbSet<Participant> Participants { get; set; }
        public DbSet<ApplicationUser> AppUsers { get; set; }
        public DbSet<TournamentType> TournamentTypes { get; set;}
        public DbSet<Team> Teams { get; set; }
        public DbSet<Match> Matches { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public TournamentAppDbContext(DbContextOptions<TournamentAppDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Participant>()
                .HasKey(p => new { p.UserId, p.TournamentId });

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.Tournament)
                .WithMany(t => t.Participants)
                .HasForeignKey(p => p.TournamentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithMany(u => u.UserTournaments)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Match>().HasOne(m => m.TeamA)
             .WithMany().HasForeignKey(m => m.TeamAId)
             .HasConstraintName("FK_MATCHS_TEAMA");

            modelBuilder.Entity<Match>().HasOne(m => m.TeamB)
             .WithMany(t => t.Matches).HasForeignKey(m => m.TeamBId)
             .HasConstraintName("FK_MATCHS_TEAMB");
        }


    }
}
