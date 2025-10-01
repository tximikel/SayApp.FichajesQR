namespace SayApp.FichajesQR.Data.DbContexts.App.Entities
{
    public class Empleado
    {
        public int EmpleadoId { get; set; }
        public Guid GuidAD { get; set; }
        public string Nombre { get; set; } = null!;
        public string? Apellidos { get; set; }
        public string Estado { get; set; } = null!;
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaBaja { get; set; }

        public ICollection<EmpleadoQR> QRs { get; set; } = new List<EmpleadoQR>();
        public ICollection<Fichaje> Fichajes { get; set; } = new List<Fichaje>();
    }
}