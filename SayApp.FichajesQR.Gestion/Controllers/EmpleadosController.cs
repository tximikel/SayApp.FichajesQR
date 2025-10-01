using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts;
using SayApp.FichajesQR.Data.DbContexts.App.Entities;
using System.Data;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;


namespace SayApp.FichajesQR.Gestion.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly AppDBContext _context;

        public EmpleadosController(AppDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _context.Empleados.ToListAsync();
            return View(empleados);
        }

        public async Task<IActionResult> CrearQR(int id)
        {
            var generadoPor = User.Identity?.Name ?? Environment.UserName;
            var payload = $"EMP-{id}-{DateTime.UtcNow:yyyyMMddHHmmss}";

            // Desactivar otros QR activos del empleado
            var otrosQR = await _context.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .ToListAsync();
            foreach (var qr in otrosQR)
            {
                qr.Activo = false;
                qr.FechaDesactivacion = DateTime.UtcNow;
                qr.DesactivadoPor = generadoPor;
            }

            // Insertar nuevo QR
            var empleadoQR = new EmpleadoQR
            {
                EmpleadoId = id,
                CodigoQR = payload,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = generadoPor
            };
            _context.EmpleadosQR.Add(empleadoQR);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = $"QR generado y almacenado en la base de datos para el empleado {id}.";
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> GenerarImagenQR(int id)
        {
            var qr = await _context.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .OrderByDescending(q => q.FechaCreacion)
                .FirstOrDefaultAsync();

            if (qr == null)
            {
                TempData["Mensaje"] = "No existe un QR activo para este empleado.";
                return RedirectToAction("Index");
            }

            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "QR_Empleados");
            Directory.CreateDirectory(folder);
            var filePath = Path.Combine(folder, $"Empleado_{id}.png");

            GenerarQR_Imagen(qr.CodigoQR, filePath);

            TempData["Mensaje"] = $"Imagen QR generada y guardada en {filePath}";
            return RedirectToAction("Index");
        }

        // Método auxiliar
        private static void GenerarQR_Imagen(string payload, string fileName)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);

            var qrCode = new BitmapByteQRCode(qrCodeData);
            var qrBytes = qrCode.GetGraphic(20);

            using var ms = new MemoryStream(qrBytes);
            using var qrImage = new Bitmap(ms);
            qrImage.Save(fileName, ImageFormat.Png);
        }
    }
}