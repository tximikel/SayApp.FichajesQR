// SayApp.FichajesQR.Gestion\ViewModels\EmpleadoConQRViewModel.cs
namespace SayApp.FichajesQR.Gestion.ViewModels
{
    public class EmpleadoConQRViewModel
    {
        public SayApp.FichajesQR.Data.DbContexts.App.Entities.Empleado Empleado { get; set; } = null!;
        public SayApp.FichajesQR.Data.DbContexts.App.Entities.EmpleadoQR? QRActivo { get; set; }
        public bool EsQRValido => QRActivo != null && QRActivo.Activo;
    }
}