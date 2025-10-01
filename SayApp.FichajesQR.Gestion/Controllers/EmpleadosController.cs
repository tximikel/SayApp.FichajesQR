using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SayApp.FichajesQR.Data.DbContexts;
using SayApp.FichajesQR.Data.DbContexts.App.Entities;

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

        // Aquí irán las acciones para crear QR y generar imagen QR
    }
}