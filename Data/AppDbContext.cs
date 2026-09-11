using GuardianPet.Models;
using Microsoft.EntityFrameworkCore;

namespace GuardianPet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Pet> Pets { get; set; }
        public DbSet<Veterinarian> Veterinarians { get; set; }
        public DbSet<Consultation> Consultations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Cpf)
                .IsUnique();

            modelBuilder.Entity<Veterinarian>()
                .HasIndex(v => v.Email)
                .IsUnique();

            modelBuilder.Entity<Veterinarian>()
                .HasIndex(v => v.Crmv)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.Pets)
                .WithMany(p => p.Users)
                .UsingEntity(j => j.ToTable("pet_user"));

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Pet)
                .WithMany(p => p.Consultations)
                .HasForeignKey(c => c.PetId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consultation>()
                .HasOne(c => c.Veterinarian)
                .WithMany(v => v.Consultations)
                .HasForeignKey(c => c.VeterinarianId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}