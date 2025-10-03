using Microsoft.AspNetCore.Mvc;
using SayApp.FichajesQR.Gestion.Models;
using System.Diagnostics;
using SayApp.FichajesQR.Gestion.ViewModels;
using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts;

namespace SayApp.FichajesQR.Gestion.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDBContext _context;

        public HomeController(ILogger<HomeController> logger, AppDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var empleados = await _context.Empleados
                .Include(e => e.QRs)
                .ToListAsync();

            var lista = empleados.Select(e => new EmpleadoConQRViewModel
            {
                Empleado = e,
                QRActivo = e.QRs.FirstOrDefault(q => q.Activo)
            }).ToList();

            return View(lista);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
