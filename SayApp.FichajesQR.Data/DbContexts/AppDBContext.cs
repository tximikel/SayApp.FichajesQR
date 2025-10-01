using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts.App.Entities;

namespace SayApp.FichajesQR.Data.DbContexts
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options) { }

        public DbSet<Empleado> Empleados => Set<Empleado>();
        public DbSet<EmpleadoQR> EmpleadosQR => Set<EmpleadoQR>();
        public DbSet<Fichaje> Fichajes => Set<Fichaje>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Empleado>(entity =>
            {
                entity.HasKey(e => e.EmpleadoId);
                entity.Property(e => e.GuidAD).IsRequired();
                entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Apellidos).HasMaxLength(150);
                entity.Property(e => e.Estado).HasMaxLength(20).IsRequired();
                entity.Property(e => e.FechaAlta).IsRequired();
                entity.Property(e => e.FechaBaja);
            });

            modelBuilder.Entity<EmpleadoQR>(entity =>
            {
                entity.HasKey(e => e.EmpleadoQRId);
                entity.Property(e => e.CodigoQR).HasMaxLength(256).IsRequired();
                entity.Property(e => e.Activo).IsRequired();
                entity.Property(e => e.FechaCreacion).IsRequired();
                entity.Property(e => e.FechaDesactivacion);
                entity.Property(e => e.CreadoPor).HasMaxLength(50);
                entity.Property(e => e.DesactivadoPor).HasMaxLength(50);

                entity.HasOne(e => e.Empleado)
                      .WithMany(e => e.QRs)
                      .HasForeignKey(e => e.EmpleadoId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Fichaje>(entity =>
            {
                entity.HasKey(f => f.FichajeId);
                entity.Property(f => f.CodigoQR).HasMaxLength(256).IsRequired();
                entity.Property(f => f.Estado).IsRequired();
                entity.Property(f => f.ErrorDescripcion).HasMaxLength(255);
                entity.Property(f => f.TimestampLectura).IsRequired();
                entity.Property(f => f.TimestampProcesado);
                entity.Property(f => f.TimestampEnviado);
                entity.Property(f => f.Oficina).HasMaxLength(50);

                entity.HasOne(f => f.Empleado)
                      .WithMany(e => e.Fichajes)
                      .HasForeignKey(f => f.EmpleadoId)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}