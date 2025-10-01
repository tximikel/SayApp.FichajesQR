using System;
using System.Collections.Generic;

namespace SayApp.FichajesQR.Data.DbContexts.Suasor.Entities;

public partial class SaymaDelegacionColaborador
{
    public byte[] Timestamp { get; set; } = null!;

    public string CodColaborador { get; set; } = null!;

    public string CodDelegacion { get; set; } = null!;

    public DateTime FechaDeAlta { get; set; }

    public DateTime FechaDeBaja { get; set; }

    public string CodMotivoBaja { get; set; } = null!;
}
