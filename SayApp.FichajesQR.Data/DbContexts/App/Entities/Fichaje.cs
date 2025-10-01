namespace SayApp.FichajesQR.Data.DbContexts.App.Entities
{
    public class Fichaje
    {
        public long FichajeId { get; set; }
        public string CodigoQR { get; set; } = null!;
        public int? EmpleadoId { get; set; }
        public int Estado { get; set; }
        public string? ErrorDescripcion { get; set; }
        public DateTime TimestampLectura { get; set; }
        public DateTime? TimestampProcesado { get; set; }
        public DateTime? TimestampEnviado { get; set; }
        public string? Oficina { get; set; }

        public Empleado? Empleado { get; set; }
    }
}