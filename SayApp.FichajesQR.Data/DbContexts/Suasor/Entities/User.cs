using System;
using System.Collections.Generic;

namespace SayApp.FichajesQR.Data.DbContexts.Suasor.Entities;

public class User
{
    public int Id { get; set; }
    public string UserName { get; set; } = null!;
    public string WindowsSecurityId { get; set; } = null!;
}
