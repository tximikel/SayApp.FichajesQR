namespace SayApp.FichajesQR.Data.DbContexts.App.Entities
{
    public class EmpleadoQR
    {
        public int EmpleadoQRId { get; set; }
        public int EmpleadoId { get; set; } // FK a Empleado.Id
        public string CodigoQR { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaDesactivacion { get; set; }
        public string? CreadoPor { get; set; }
        public string? DesactivadoPor { get; set; }

        // Auditoría
        public DateTime? FechaModificacion { get; set; }
        public string? ModificadoPor { get; set; }
        public byte[] RowVersion { get; set; } = null!;

        public Empleado Empleado { get; set; } = null!;
    }
}