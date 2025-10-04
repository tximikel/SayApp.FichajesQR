using ZXing;
using ZXing.Common;
using SkiaSharp;

namespace SayApp.FichajesQR.LectorAndroid
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnScanClicked(object sender, EventArgs e)
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo != null)
            {
                using var stream = await photo.OpenReadAsync();
                using var skStream = new SKManagedStream(stream);
                var bitmap = SKBitmap.Decode(skStream);

                if (bitmap != null)
                {
                    // Convertir SKBitmap a array de bytes (formato RGB)
                    var width = bitmap.Width;
                    var height = bitmap.Height;
                    var pixels = bitmap.Bytes;

                    // ZXing espera formato RGB24 (3 bytes por píxel)
                    var luminanceSource = new RGBLuminanceSource(pixels, width, height, RGBLuminanceSource.BitmapFormat.RGB32);

                    var binarizer = new HybridBinarizer(luminanceSource);
                    var binaryBitmap = new BinaryBitmap(binarizer);

                    var reader = new ZXing.MultiFormatReader();
                    var result = reader.decode(binaryBitmap);

                    resultLabel.Text = result?.Text ?? "No se detectó QR";
                }
                else
                {
                    resultLabel.Text = "No se pudo procesar la imagen";
                }
            }
        }
    }
}
