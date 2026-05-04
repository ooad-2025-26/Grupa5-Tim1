using bibliotecha.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace bibliotecha.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
    {
        public DbSet<Autor> Autor { get; set; }
        public DbSet<Knjiga> Knjiga { get; set; }
        public DbSet<Korisnik> Korisnik { get; set; }
        public DbSet<Obavjestenje> Obavjestenje { get; set; }
        public DbSet<Posudba> Posudba { get; set; }
        public DbSet<Primjerak> Primjerak { get; set; }
        public DbSet<Rezervacija> Rezervacija { get; set; }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Autor>().ToTable("Autor");
            modelBuilder.Entity<Knjiga>().ToTable("Knjiga");
            modelBuilder.Entity<Korisnik>().ToTable("Korisnik");
            modelBuilder.Entity<Obavjestenje>().ToTable("Obavjestenje");
            modelBuilder.Entity<Posudba>().ToTable("Posudba");
            modelBuilder.Entity<Primjerak>().ToTable("Primjerak");
            modelBuilder.Entity<Rezervacija>().ToTable("Rezervacija");
        }

    }

}
