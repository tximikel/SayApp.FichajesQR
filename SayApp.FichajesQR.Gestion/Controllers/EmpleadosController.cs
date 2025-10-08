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
using SayApp.FichajesQR.Gestion.ViewModels;
using SayApp.FichajesQR.Data.OData;
using Microsoft.Extensions.Configuration;


namespace SayApp.FichajesQR.Gestion.Controllers
{
    public class EmpleadosController : Controller
    {
        private readonly AppDBContext _db;
        private readonly IConfiguration _configuration;

        public EmpleadosController(AppDBContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _db.Empleados
                .Include(e => e.QRs)
                .ToListAsync();

            var lista = empleados.Select(e => new EmpleadoConQRViewModel
            {
                Empleado = e,
                QRActivo = e.QRs.FirstOrDefault(q => q.Activo)
            }).ToList();

            return View(lista);
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
            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == id);
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

            // Anular otros QR activos del empleado
            var otrosQR = await _db.EmpleadosQR
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
            _db.EmpleadosQR.Add(empleadoQR);
            await _db.SaveChangesAsync();

            ProbarDescifradoYVerificacion(payloadFinal, guidADShort);

            TempData["Mensaje"] = $"QR generado y almacenado en la base de datos para el empleado {id}.";
            return RedirectToAction("GenerarImagenQR", new { id });
        }

        public async Task<IActionResult> GenerarImagenQR(int id)
        {
            var qr = await _db.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .OrderByDescending(q => q.FechaCreacion)
                .FirstOrDefaultAsync();

            if (qr == null)
            {
                TempData["Mensaje"] = "No existe un QR activo para este empleado.";
                return RedirectToAction("Index");
            }

            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == id);
            if (empleado == null)
            {
                TempData["Mensaje"] = "Empleado no encontrado.";
                return RedirectToAction("Index");
            }

            // Guardar la imagen QR en una carpeta accesible por la web (por ejemplo, wwwroot/qr_temp)
            var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "qr_temp");
            Directory.CreateDirectory(wwwrootPath);
            var fileName = $"Empleado_{id}_{DateTime.UtcNow:yyyyMMddHHmmss}.png";
            var filePath = Path.Combine(wwwrootPath, fileName);

            GenerarQR_Imagen(qr.CodigoQR, filePath);

            // Pasar la ruta relativa y los datos del empleado a la vista usando ViewData
            ViewData["QrImagePath"] = $"/qr_temp/{fileName}";
            ViewData["QrEmpId"] = empleado.Id;
            ViewData["QrEmpleadoId"] = empleado.EmpleadoId;
            ViewData["QrEmpleadoNombre"] = $"{empleado.Nombre} {empleado.Apellidos}";
            ViewData["QrFechaCreacion"] = qr.FechaCreacion.ToString("yyyy-MM-dd HH:mm:ss");

            // Volver a cargar la lista de empleados y mostrar la imagen
            var empleadosViewModel = await ObtenerEmpleadosConQRViewModelAsync();
            return View("Index", empleadosViewModel);
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

        [HttpGet]
        public async Task<JsonResult> ComprobarQRActivo(int id)
        {
            var qr = await _db.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .OrderByDescending(q => q.FechaCreacion)
                .FirstOrDefaultAsync();

            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == id);

            if (qr != null && empleado != null)
            {
                return Json(new
                {
                    existe = true,
                    empleadoId = empleado.EmpleadoId,
                    id = empleado.Id,
                    nombre = empleado.Nombre,
                    apellidos = empleado.Apellidos,
                    fecha = qr.FechaCreacion.ToString("yyyy-MM-dd HH:mm")
                });
            }
            return Json(new { existe = false });
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var empleado = await _db.Empleados.FirstOrDefaultAsync(e => e.Id == id);
            if (empleado == null)
            {
                return NotFound();
            }

            var qrActivo = await _db.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .OrderByDescending(q => q.FechaCreacion)
                .FirstOrDefaultAsync();

            var vistaModelo = new EmpleadoConQRViewModel
            {
                Empleado = empleado,
                QRActivo = qrActivo
            };

            return View(vistaModelo);
        }

        private async Task<List<EmpleadoConQRViewModel>> ObtenerEmpleadosConQRViewModelAsync()
        {
            var empleados = await _db.Empleados
                .Include(e => e.QRs)
                .ToListAsync();

            return empleados.Select(e => new EmpleadoConQRViewModel
            {
                Empleado = e,
                QRActivo = e.QRs.FirstOrDefault(q => q.Activo)
            }).ToList();
        }

        [HttpPost]
        public async Task<IActionResult> AnularQR(int id)
        {
            var qr = await _db.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .OrderByDescending(q => q.FechaCreacion)
                .FirstOrDefaultAsync();

            if (qr != null)
            {
                qr.Activo = false;
                qr.FechaDesactivacion = DateTime.UtcNow;
                qr.DesactivadoPor = User.Identity?.Name ?? Environment.UserName;
                await _db.SaveChangesAsync();
                TempData["Mensaje"] = "QR desactivado correctamente.";
            }
            else
            {
                TempData["Mensaje"] = "No hay QR activo para anular.";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> SimularFichaje(int id)
        {
            // Obtener el QR activo del empleado
            var qr = await _db.EmpleadosQR
                .Where(q => q.EmpleadoId == id && q.Activo)
                .OrderByDescending(q => q.FechaCreacion)
                .FirstOrDefaultAsync();

            if (qr == null)
            {
                TempData["Mensaje"] = "No existe un QR activo para este empleado.";
                return RedirectToAction("Index");
            }

            // Llamada API al Worker
            using var httpClient = new HttpClient();
            // Cambia la URL por la de tu Worker real
            var apiUrl = "https://localhost:5001/api/fichajes/simular";
            var payload = new
            {
                codigoQR = qr.CodigoQR,
                empleadoId = id
            };
            var response = await httpClient.PostAsJsonAsync(apiUrl, payload);

            if (response.IsSuccessStatusCode)
            {
                TempData["Mensaje"] = "Fichaje simulado correctamente.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["Mensaje"] = $"Error al simular fichaje: {error}";
            }

            return RedirectToAction("Index");
        }

        // Vista previa de sincronización
        [HttpGet]
        public async Task<IActionResult> SincronizarPreview()
        {
            var odataService = new ODataEmpleadosService(
                _configuration["OData:BaseUrl"],
                _configuration["OData:Username"],
                _configuration["OData:Password"]
            );
            var externos = await odataService.GetEmpleadosAsync();
            var locales = _db.Empleados.ToList();

            // Detectar nuevos y bajas
            var nuevos = externos.Where(e => !locales.Any(l => l.DNI == e.Cod_Empleado)).ToList();
            var bajas = locales.Where(l => !externos.Any(e => e.Cod_Empleado == l.DNI)).ToList();

            return Json(new { nuevos, bajas });
        }

        // Aplicar sincronización
        [HttpPost]
        public async Task<IActionResult> SincronizarAplicar()
        {
            var odataService = new ODataEmpleadosService(
                _configuration["OData:BaseUrl"],
                _configuration["OData:Username"],
                _configuration["OData:Password"]
            );
            var externos = await odataService.GetEmpleadosAsync();
            var locales = _db.Empleados.ToList();

            // Nuevos empleados
            var nuevos = externos.Where(e => !locales.Any(l => l.DNI == e.Cod_Empleado)).ToList();
            foreach (var emp in nuevos)
            {
                bool esAlta = emp.Fecha_baja == DateTime.MinValue;
                var nuevo = new Empleado
                {
                    DNI = emp.Cod_Empleado,
                    Nombre = emp.Nombre,
                    Apellidos = $"{emp.Primer_apellido} {emp.Segundo_apellido}".Trim(),
                    Estado = esAlta ? "Alta" : "Baja",
                    FechaAlta = esAlta ? emp.Fecha_alta : DateTime.MinValue,
                    FechaBaja = !esAlta ? emp.Fecha_baja : null,
                    FechaCreacion = DateTime.UtcNow,
                    CreadoPor = User.Identity?.Name ?? Environment.UserName
                    // Otros campos si lo necesitas
                };
                _db.Empleados.Add(nuevo);
                // Aquí puedes crear el QR si lo necesitas
            }

            // Bajas
            var bajas = locales.Where(l => !externos.Any(e => e.Cod_Empleado == l.DNI)).ToList();
            foreach (var baja in bajas)
            {
                baja.Estado = "Baja";
                baja.FechaBaja = DateTime.UtcNow;
                // Aquí puedes anular el QR si lo necesitas
            }

            await _db.SaveChangesAsync();

            return Json(new { mensaje = "Sincronización aplicada correctamente." });
        }
    }
}