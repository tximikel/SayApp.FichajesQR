namespace SayApp.FichajesQR.Data.DbContexts.App.Entities
{
    public class Empleado
    {
        public int Id { get; set; } // Clave primaria única (autoincremental)
        public int EmpleadoId { get; set; } // Id Suasor, informativo (puede repetirse)
        public Guid GuidAD { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Apellidos { get; set; }
        public string? DNI { get; set; } // <--- Añadido
        public string Estado { get; set; } = null!;
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public string CreadoPor { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public ICollection<EmpleadoQR> QRs { get; set; } = new List<EmpleadoQR>();
        public ICollection<Fichaje> Fichajes { get; set; } = new List<Fichaje>();
    }
}