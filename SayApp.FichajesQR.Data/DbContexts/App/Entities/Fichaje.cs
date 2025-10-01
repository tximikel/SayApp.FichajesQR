namespace SayApp.FichajesQR.Data.DbContexts.App.Entities
{
    public class Fichaje
    {
        public long FichajeId { get; set; }
        public int? EmpleadoId { get; set; } // FK a Empleado.Id
        public string CodigoQR { get; set; } = null!;
        public int Estado { get; set; }
        public string? ErrorDescripcion { get; set; }
        public DateTime TimestampLectura { get; set; }
        public DateTime? TimestampProcesado { get; set; }
        public DateTime? TimestampEnviado { get; set; }
        public string? Oficina { get; set; }

        // Auditoría
        public DateTime FechaCreacion { get; set; }
        public string CreadoPor { get; set; } = null!;
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public Empleado? Empleado { get; set; }
    }
}