using System;
using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts.Suasor.Entities;

namespace SayApp.FichajesQR.Data.DbContexts;
//namespace SayAppTms.Dedica.Persistence.DbContexts; //Copiados desde aquí

public partial class SuasorDbContext : DbContext
{
    public SuasorDbContext() { }

    public SuasorDbContext(DbContextOptions<SuasorDbContext> options)
        : base(options) { }

    public virtual DbSet<SaymaColaborador> SaymaColaborador { get; set; }
    public virtual DbSet<SaymaDelegacionColaborador> SaymaDelegacionColaborador { get; set; }
    public virtual DbSet<SaymaDepartamentoColaboradorGen> SaymaDepartamentoColaboradorGen { get; set; }
    public virtual DbSet<User> User { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Modern_Spanish_100_CI_AS");

        modelBuilder.Entity<SaymaColaborador>(entity =>
        {
            entity.HasKey(e => e.CodColaborador).HasName("SAYMA$Colaborador$0");
            entity.ToTable("SAYMA$Colaborador");
            entity.HasIndex(e => new { e.UsuarioAsociado, e.CodColaborador }, "$1").IsUnique();
            entity.HasIndex(e => new { e.NombreColaborador, e.CodColaborador }, "$2").IsUnique();
            entity.HasIndex(e => new { e.Id, e.CodColaborador }, "$3").IsUnique();
            entity.HasIndex(e => new { e.ModifiedOn, e.CodColaborador }, "$4").IsUnique();
            entity.Property(e => e.CodColaborador).HasMaxLength(10).HasColumnName("Cod_ colaborador");
            entity.Property(e => e.NombreColaborador).HasMaxLength(50).HasColumnName("Nombre colaborador");
            entity.Property(e => e.Departamento).HasMaxLength(10);
            entity.Property(e => e.Baja);
            entity.Property(e => e.NIF).HasMaxLength(12).HasColumnName("N_I_F_");
            entity.Property(e => e.SiglasDomicilio).HasMaxLength(2).HasColumnName("Siglas domicilio");
            entity.Property(e => e.Domicilio).HasMaxLength(40);
            entity.Property(e => e.Casa).HasMaxLength(4);
            entity.Property(e => e.Escalera).HasMaxLength(4);
            entity.Property(e => e.Piso).HasMaxLength(4);
            entity.Property(e => e.Puerta).HasMaxLength(4);
            entity.Property(e => e.CodPoblacion).HasMaxLength(10).HasColumnName("Cod_ poblacion");
            entity.Property(e => e.Poblacion).HasMaxLength(50);
            entity.Property(e => e.CodPostal).HasMaxLength(10).HasColumnName("Cod_ postal");
            entity.Property(e => e.ZonaPostal).HasMaxLength(50).HasColumnName("Zona postal");
            entity.Property(e => e.CodProvincia).HasMaxLength(10).HasColumnName("Cod_ provincia");
            entity.Property(e => e.Provincia).HasMaxLength(50);
            entity.Property(e => e.CodPais).HasMaxLength(3).HasColumnName("Cod_ pais");
            entity.Property(e => e.TelefonoPral).HasMaxLength(20).HasColumnName("Telefono pral_");
            entity.Property(e => e.Fax).HasMaxLength(12);
            entity.Property(e => e.Email).HasMaxLength(40);
            entity.Property(e => e.CodTipoTrabajo).HasMaxLength(10).HasColumnName("Cod_ tipo trabajo");
            entity.Property(e => e.BloqueadoImputaciones);
            entity.Property(e => e.Imagen).HasColumnType("image");
            entity.Property(e => e.UsuarioAsociado).HasMaxLength(50).HasColumnName("Usuario asociado");
            entity.Property(e => e.FormulaFechaCalcAgenda).HasMaxLength(32).IsUnicode(false).HasColumnName("Formula fecha calc_ agenda");
            entity.Property(e => e.FormulaFechaCalcExpImp).HasMaxLength(32).IsUnicode(false).HasColumnName("Formula fecha calc_ exp_ imp_");
            entity.Property(e => e.Id);
            entity.Property(e => e.CreatedOn).HasColumnType("datetime").HasColumnName("Created On");
            entity.Property(e => e.CreatedBy).HasMaxLength(50).HasColumnName("Created By");
            entity.Property(e => e.ModifiedOn).HasColumnType("datetime").HasColumnName("Modified On");
            entity.Property(e => e.ModifiedBy).HasMaxLength(50).HasColumnName("Modified By");
            entity.Property(e => e.Timestamp).IsRowVersion().IsConcurrencyToken().HasColumnName("timestamp");
        });

        modelBuilder.Entity<SaymaDelegacionColaborador>(entity =>
        {
            entity.HasKey(e => new { e.CodColaborador, e.CodDelegacion, e.FechaDeAlta }).HasName("SAYMA$Delegacion-Colaborador$0");
            entity.ToTable("SAYMA$Delegacion-Colaborador");
            entity.HasIndex(e => new { e.CodColaborador, e.FechaDeAlta, e.CodDelegacion }, "$1").IsUnique();
            entity.Property(e => e.CodColaborador).HasMaxLength(10).HasColumnName("Cod_ Colaborador");
            entity.Property(e => e.CodDelegacion).HasMaxLength(10).HasColumnName("Cod_ Delegacion");
            entity.Property(e => e.FechaDeAlta).HasColumnType("datetime").HasColumnName("Fecha de alta");
            entity.Property(e => e.CodMotivoBaja).HasMaxLength(3).HasColumnName("Cod_ motivo baja");
            entity.Property(e => e.FechaDeBaja).HasColumnType("datetime").HasColumnName("Fecha de baja");
            entity.Property(e => e.Timestamp).IsRowVersion().IsConcurrencyToken().HasColumnName("timestamp");
        });

        modelBuilder.Entity<SaymaDepartamentoColaboradorGen>(entity =>
        {
            entity.HasKey(e => new { e.CodColaborador, e.CodDepartamento, e.FechaDeAlta }).HasName("SAYMA$Departamento-Colaborador GEN$0");
            entity.ToTable("SAYMA$Departamento-Colaborador GEN");
            entity.HasIndex(e => new { e.CodColaborador, e.FechaDeAlta, e.CodDepartamento }, "$1").IsUnique();
            entity.Property(e => e.CodColaborador).HasMaxLength(10).HasColumnName("Cod_ Colaborador");
            entity.Property(e => e.CodDepartamento).HasMaxLength(10).HasColumnName("Cod_ Departamento");
            entity.Property(e => e.FechaDeAlta).HasColumnType("datetime").HasColumnName("Fecha de alta");
            entity.Property(e => e.CodMotivoBaja).HasMaxLength(3).HasColumnName("Cod_ motivo baja");
            entity.Property(e => e.FechaDeBaja).HasColumnType("datetime").HasColumnName("Fecha de baja");
            entity.Property(e => e.Timestamp).IsRowVersion().IsConcurrencyToken().HasColumnName("timestamp");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("User", "dbo"); // Mapea a [dbo].[User]
            entity.HasKey(e => e.UserName); // Ajusta si la clave es otra
            entity.Property(e => e.UserName).HasColumnName("User Name").HasMaxLength(50);
            entity.Property(e => e.WindowsSecurityId).HasColumnName("Windows Security ID").HasMaxLength(200);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

    // Evita que se puedan hacer cambios en esta base de datos de solo lectura
    public override int SaveChanges()
        => throw new InvalidOperationException("La base de datos Suasor es de solo lectura y no admite cambios estructurales.");

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => throw new InvalidOperationException("La base de datos Suasor es de solo lectura y no admite cambios estructurales.");
}
