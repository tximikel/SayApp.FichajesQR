using ZXing.Net.Maui;

namespace SayApp.FichajesQR.LectorAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            var qr = e.Results.FirstOrDefault();
            if (qr != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    resultLabel.Text = $"QR detectado: {qr.Value}";
                    // Aquí puedes llamar a tu API para enviar el QR
                });
            }
        }
    }
}
