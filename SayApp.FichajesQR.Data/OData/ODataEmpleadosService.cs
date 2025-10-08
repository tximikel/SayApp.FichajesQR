using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace SayApp.FichajesQR.Data.OData;

public class ODataEmpleadosService
{
    private readonly HttpClient _httpClient;

    public ODataEmpleadosService(string baseUrl, string username, string password)
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(baseUrl);
        var byteArray = System.Text.Encoding.ASCII.GetBytes($"{username}:{password}");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
    }

    public async Task<List<ODataEmpleado>> GetEmpleadosAsync(string filtroOData = "")
    {
        var url = "ListaEmpleadosGestHoraria?$format=json";
        if (!string.IsNullOrWhiteSpace(filtroOData))
            url += $"&{filtroOData}";

        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var root = JsonDocument.Parse(json).RootElement;
        var empleados = root.GetProperty("value").Deserialize<List<ODataEmpleado>>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return empleados ?? new List<ODataEmpleado>();
    }
}