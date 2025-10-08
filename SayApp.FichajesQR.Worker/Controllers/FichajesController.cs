// Worker/Controllers/FichajesController.cs
using Microsoft.AspNetCore.Mvc;
using SayApp.FichajesQR.Data.DbContexts;
using SayApp.FichajesQR.Data.DbContexts.App.Entities;

[ApiController]
[Route("api/fichajes")]
public class FichajesController : ControllerBase
{
    private readonly AppDBContext _context;

    public FichajesController(AppDBContext context)
    {
        _context = context;
    }

    [HttpPost("simular")]
    public async Task<IActionResult> SimularFichaje([FromBody] SimularFichajeDto dto)
    {
        Console.WriteLine($"[SimularFichaje] Recibido para empleadoId={dto.empleadoId}, codigoQR={dto.codigoQR}");
        // Validar QR, empleado, etc. seg�n tu l�gica real
        var empleado = await _context.Empleados.FindAsync(dto.empleadoId);
        if (empleado == null)
            return BadRequest("Empleado no encontrado.");

        // Aqu� podr�as validar el QR si lo necesitas

        var fichaje = new Fichaje
        {
            EmpleadoId = empleado.Id,
            CodigoQR = dto.codigoQR,
            Estado = 1, // Estado simulado, aj�stalo seg�n tu l�gica
            TimestampLectura = DateTime.UtcNow,
            Oficina = "Gesti�n-Simulador",
            FechaCreacion = DateTime.UtcNow,
            CreadoPor = "Simulador"
        };

        _context.Fichajes.Add(fichaje);
        await _context.SaveChangesAsync();

        return Ok();
    }
}

public class SimularFichajeDto
{
    public string? codigoQR { get; set; }
    public int empleadoId { get; set; }
}