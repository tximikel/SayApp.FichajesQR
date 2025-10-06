using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Storage;
using ZXing.Net.Maui;

namespace SayApp.FichajesQR.LectorAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
                // Aquí comprobamos de nuevo el estado y activamos si se concedió
                if (status == PermissionStatus.Granted)
                {
                    barcodeReader.IsDetecting = true;
                    return;
                }
            }

            if (status == PermissionStatus.Granted)
            {
                barcodeReader.IsDetecting = true;
            }
            else
            {
                barcodeReader.IsDetecting = false;
                await DisplayAlert("Permiso requerido", "La app necesita acceso a la cámara para escanear códigos QR.", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            barcodeReader.IsDetecting = false;
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var qr = e.Results.FirstOrDefault();
            var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");

            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (qr != null)
                {
                    resultLabel.Text = $"[{timestamp}] QR detectado: {qr.Value}";
                    qrWarningLabel.Text = ""; // Sin BoundingBox, no podemos avisar si está lejos
                }
                else
                {
                    qrWarningLabel.Text = "No se detecta ningún QR. Acércalo a la cámara.";
                }
            });
        }
    }
}
