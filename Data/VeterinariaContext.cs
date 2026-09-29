using Microsoft.EntityFrameworkCore;
using Veterinaria.Models;

namespace Veterinaria.Data
{
    public class VeterinariaContext : DbContext
    {
        public VeterinariaContext(DbContextOptions<VeterinariaContext> options)
            : base(options)
        {
        }

        // TABLAS
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Mascota> Mascotas { get; set; }
        public DbSet<Especie> Especies { get; set; }
        public DbSet<Raza> Razas { get; set; }
        public DbSet<Turno> Turnos { get; set; }
        public DbSet<Consulta> Consultas { get; set; }
        public DbSet<Tratamiento> Tratamientos { get; set; }
        public DbSet<ConsultaTratamiento> ConsultaTratamientos { get; set; }
        public DbSet<HClinica> HistoriasClinicas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // CLAVE COMPUESTA CONSULTA - TRATAMIENTO


            modelBuilder.Entity<ConsultaTratamiento>()
                .HasKey(ct => new
                {
                    ct.IdConsulta,
                    ct.IdTratamiento
                });



            // CONSULTA -> CONSULTA TRATAMIENTO

            modelBuilder.Entity<ConsultaTratamiento>()
                .HasOne(ct => ct.Consulta)
                .WithMany(c => c.ConsultaTratamientos)
                .HasForeignKey(ct => ct.IdConsulta);



            // TRATAMIENTO -> CONSULTA TRATAMIENTO

            modelBuilder.Entity<ConsultaTratamiento>()
                .HasOne(ct => ct.Tratamiento)
                .WithMany(t => t.ConsultaTratamientos)
                .HasForeignKey(ct => ct.IdTratamiento);
        }
    }
}