namespace SayApp.FichajesQR.Data.OData;

public class ODataEmpleado
{
    public string Cod_Empleado { get; set; }
    public string Cod_Empresa { get; set; }
    public DateTime Fecha_alta { get; set; }
    public DateTime Fecha_baja { get; set; }
    public string Cod_Externo { get; set; }
    public DateTime Fecha_ult_modificacion { get; set; }
    public string Nombre { get; set; }
    public string Primer_apellido { get; set; }
    public string Segundo_apellido { get; set; }
    public string Nombre_empresa { get; set; }
    public string AuxiliaryIndex1 { get; set; }
}