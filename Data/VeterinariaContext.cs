using Microsoft.EntityFrameworkCore;
using Veterinaria.Models;

namespace Veterinaria.Data
{
    public class VeterinariaContext : DbContext
    {
        public VeterinariaContext(
            DbContextOptions<VeterinariaContext> options)
            : base(options)
        {
        }

        // Tablas
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Raza> Razas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<HClinica> HistoriasClinicas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Veterinario> Veterinarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Un turno puede tener una sola consulta
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Turno)
                .WithOne()
                .HasForeignKey<Consulta>(c => c.IdTurno)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Consulta>()
                .HasIndex(c => c.IdTurno)
                .IsUnique();

            // Matrícula única
            modelBuilder.Entity<Veterinario>()
                .HasIndex(v => v.Matricula)
                .IsUnique();

            // Un veterinario puede realizar muchas consultas
            modelBuilder.Entity<Consulta>()
                .HasOne(c => c.Veterinario)
                .WithMany()
                .HasForeignKey(c => c.IdVeterinario)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}