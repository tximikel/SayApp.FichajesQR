using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System.Threading.Tasks;

namespace SayApp.FichajesQR.LectorAndroid;

public static class Methods
{
    /// <summary>
    /// Comprueba y solicita permisos necesarios (cámara y vibración).
    /// Muestra un mensaje si el usuario los deniega.
    /// </summary>
    public static async Task AskForRequiredPermissionAsync()
    {
        // --- Cámara ---
        var camStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (camStatus != PermissionStatus.Granted)
        {
            camStatus = await Permissions.RequestAsync<Permissions.Camera>();
        }

        System.Diagnostics.Debug.WriteLine($"Permiso cámara: {camStatus}");

        if (camStatus != PermissionStatus.Granted)
        {
            await Shell.Current.DisplayAlert(
                "Permiso requerido",
                "Se necesita acceso a la cámara para escanear códigos QR.",
                "Aceptar");
            return;
        }

        // --- Vibración (opcional) ---
        var vibStatus = await Permissions.CheckStatusAsync<Permissions.Vibrate>();
        if (vibStatus != PermissionStatus.Granted)
        {
            await Permissions.RequestAsync<Permissions.Vibrate>();
        }

        // --- Puedes añadir aquí otros permisos opcionales ---
        // Ejemplo: almacenamiento si en el futuro guardas logs o imágenes
    }
}
