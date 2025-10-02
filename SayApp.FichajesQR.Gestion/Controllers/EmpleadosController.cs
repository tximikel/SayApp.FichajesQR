using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts;
using SayApp.FichajesQR.Data.DbContexts.App.Entities;
using System.Data;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Text;


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

        // Para AES-256, la clave debe tener 32 bytes (caracteres UTF-8) y el IV exactamente 16 bytes.
        private static readonly string SecretKey = "FicharEmpleados/LangileakFitxatu"; // 32 caracteres
        private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("SayAppFichajesQR"); // 16 caracteres

        private static string GenerarChecksum(string data)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(SecretKey));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash, 0, 4).Replace("-", "");
        }

        private static string EncryptString(string plainText)
        {
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(SecretKey);
            aes.IV = AesIV;
            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var sw = new StreamWriter(cs))
                sw.Write(plainText);
            return Convert.ToBase64String(ms.ToArray());
        }

        private static string DecryptString(string cipherText)
        {
            var buffer = Convert.FromBase64String(cipherText);
            using var aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(SecretKey);
            aes.IV = AesIV;
            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(buffer);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var sr = new StreamReader(cs);
            return sr.ReadToEnd();
        }

        public async Task<IActionResult> CrearQR(int id)
        {
            var generadoPor = User.Identity?.Name ?? Environment.UserName;
            var empleado = await _context.Empleados.FirstOrDefaultAsync(e => e.Id == id);
            if (empleado == null)
            {
                TempData["Mensaje"] = "Empleado no encontrado.";
                return RedirectToAction("Index");
            }
            var GuidAD = empleado.GuidAD.ToString("N"); // GUID sin guiones

            // Usar solo los primeros 8 caracteres del GUID
            var guidADShort = GuidAD.Substring(0, 8);

            // Cifrar la parte corta del GUID
            var guidADCifrado = EncryptString(guidADShort);
            var guidShort = Convert.ToBase64String(Convert.FromBase64String(guidADCifrado)).TrimEnd('=');

            var idStr = id.ToString("X");
            var fechaCorta = DateTime.UtcNow.ToString("yyMMddHHmm");
            var chk = GenerarChecksum($"{idStr}|{guidShort}|{fechaCorta}");
            var payloadFinal = $"{idStr}|{guidShort}|{fechaCorta}|{chk}";

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

            var empleadoQR = new EmpleadoQR
            {
                EmpleadoId = id,
                CodigoQR = payloadFinal,
                Activo = true,
                FechaCreacion = DateTime.UtcNow,
                CreadoPor = generadoPor
            };
            _context.EmpleadosQR.Add(empleadoQR);
            await _context.SaveChangesAsync();

            ProbarDescifradoYVerificacion(payloadFinal, guidADShort);

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

            // Guardar la imagen QR en una carpeta accesible por la web (por ejemplo, wwwroot/qr_temp)
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qr_temp");
            Directory.CreateDirectory(wwwrootPath);
            var fileName = $"Empleado_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(wwwrootPath, fileName);

            GenerarQR_Imagen(qr.CodigoQR, filePath);

            // Pasar la ruta relativa a la vista usando ViewData
            ViewData["QrImagePath"] = $"/qr_temp/{fileName}";

            // Volver a cargar la lista de empleados y mostrar la imagen
            var empleados = await _context.Empleados.ToListAsync();
            return View("Index", empleados);
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

        private void ProbarDescifradoYVerificacion(string payloadFinal, string guidADShortOriginal)
        {
            var partes = payloadFinal.Split('|');
            if (partes.Length != 4)
            {
                System.Diagnostics.Debug.WriteLine("Formato de payload incorrecto.");
                return;
            }

            var idStr = partes[0];
            var guidShort = partes[1];
            var fechaCorta = partes[2];
            var chk = partes[3];

            var payloadBase = $"{idStr}|{guidShort}|{fechaCorta}";
            var chkEsperado = GenerarChecksum(payloadBase);
            bool chkCoincide = chkEsperado == chk;

            string guidADCifradoBase64 = guidShort;
            int padding = 4 - (guidADCifradoBase64.Length % 4);
            if (padding < 4) guidADCifradoBase64 = guidADCifradoBase64 + new string('=', padding);

            string guidADCifrado;
            try
            {
                guidADCifrado = Convert.ToBase64String(Convert.FromBase64String(guidADCifradoBase64));
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("Error al decodificar el GUID cifrado.");
                return;
            }
            var guidADDescifrado = DecryptString(guidADCifrado);

            bool guidCoincide = guidADDescifrado == guidADShortOriginal;

            System.Diagnostics.Debug.WriteLine($"GUID descifrado: {guidADDescifrado}");
            System.Diagnostics.Debug.WriteLine($"GUID coincide: {guidCoincide}");
            System.Diagnostics.Debug.WriteLine($"Checksum coincide: {chkCoincide}");
        }
    }
}