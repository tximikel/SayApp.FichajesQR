namespace SayApp.FichajesQR.Data.DbContexts.App.Entities
{
    public class EmpleadoQR
    {
        public int EmpleadoQRId { get; set; }
        public int EmpleadoId { get; set; }
        public string CodigoQR { get; set; } = null!;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? FechaDesactivacion { get; set; }
        public string? CreadoPor { get; set; }
        public string? DesactivadoPor { get; set; }

        public Empleado Empleado { get; set; } = null!;
    }
}