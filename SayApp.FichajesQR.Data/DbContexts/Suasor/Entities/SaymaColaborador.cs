using System;
using System.Collections.Generic;

namespace SayApp.FichajesQR.Data.DbContexts.Suasor.Entities;

public partial class SaymaColaborador
{
    public byte[] Timestamp { get; set; } = null!;

    public string CodColaborador { get; set; } = null!;

    public string NombreColaborador { get; set; } = null!;

    public string Departamento { get; set; } = null!;

    public byte Baja { get; set; }

    public string NIF { get; set; } = null!;

    public string SiglasDomicilio { get; set; } = null!;

    public string Domicilio { get; set; } = null!;

    public string Casa { get; set; } = null!;

    public string Escalera { get; set; } = null!;

    public string Piso { get; set; } = null!;

    public string Puerta { get; set; } = null!;

    public string CodPoblacion { get; set; } = null!;

    public string Poblacion { get; set; } = null!;

    public string CodPostal { get; set; } = null!;

    public string ZonaPostal { get; set; } = null!;

    public string CodProvincia { get; set; } = null!;

    public string Provincia { get; set; } = null!;

    public string CodPais { get; set; } = null!;

    public string TelefonoPral { get; set; } = null!;

    public string Fax { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string CodTipoTrabajo { get; set; } = null!;

    public byte BloqueadoImputaciones { get; set; }

    public byte[]? Imagen { get; set; }

    public string UsuarioAsociado { get; set; } = null!;

    public string FormulaFechaCalcAgenda { get; set; } = null!;

    public string FormulaFechaCalcExpImp { get; set; } = null!;

    public Guid Id { get; set; }

    public DateTime CreatedOn { get; set; }

    public string CreatedBy { get; set; } = null!;

    public DateTime ModifiedOn { get; set; }

    public string ModifiedBy { get; set; } = null!;
}
