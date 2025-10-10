using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

// 👇 OJO: estos dos
using BarcodeScanning;               // Tipos: CameraView, BarcodeResult, BarcodeFormats, OnDetectionFinishedEventArg
using BarcodeScanning.Native.Maui;   // Extensiones/registro del paquete

namespace SayApp.FichajesQR.LectorAndroid
{
    public partial class MainPage : ContentPage
    {
        private readonly ObservableCollection<RectF> _boxes = new();
        private readonly BoundingDrawable _drawable;
        private bool _hasDetection;
        private bool _calibrated;
        private float _calibOffsetX, _calibOffsetY;

        public MainPage()
        {
            InitializeComponent();
            _drawable = new BoundingDrawable(_boxes, () => _hasDetection ? Colors.LimeGreen : Colors.Goldenrod);
            Overlay.Drawable = _drawable;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var camStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (camStatus != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.Camera>();

            _calibOffsetX = Preferences.Get("CalibOffsetX", 0f);
            _calibOffsetY = Preferences.Get("CalibOffsetY", 0f);
            _calibrated = Preferences.Get("CalibActive", false);

            LblEstado.Text = "Escaneando...";
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Camera.IsActive = false;
            _boxes.Clear();
            Overlay.Invalidate();
        }

		// ✅ Evento correcto para BarcodeScanning.Native.Maui 1.2.5
		private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
		{
			MainThread.BeginInvokeOnMainThread(() =>
			{
				if (e.BarcodeResults == null || e.BarcodeResults.Count == 0)
				{
					LblEstado.Text = "Escaneando...";
					return;
				}

				var first = e.BarcodeResults.FirstOrDefault();
				var value = first?.Text ?? "(sin valor)";

				LblUltimo.Text = $"Último QR leído: {value}";
				LblEstado.Text = "QR detectado correctamente";
			});
		}

        private void BtnStart_Clicked(object sender, EventArgs e)
        {
            Camera.IsActive = true;
            LblEstado.Text = "Escaneando...";
        }

        private void BtnStop_Clicked(object sender, EventArgs e)
        {
            Camera.IsActive = false;
            _boxes.Clear();
            _hasDetection = false;
            Overlay.Invalidate();
            LblEstado.Text = "Detenido";
        }

        private void BtnToggleCam_Clicked(object sender, EventArgs e)
        {
            Camera.CameraFacing = Camera.CameraFacing == CameraFacing.Back
                ? CameraFacing.Front
                : CameraFacing.Back;

            DisplayAlert("Cámara", $"Ahora: {Camera.CameraFacing}", "OK");
        }

        private void Overlay_Tapped(object sender, TappedEventArgs e)
        {
            if (_boxes.Count == 0) return;

            var box = _boxes[0];
            var point = e.GetPosition((View)sender);
            if (point == null) return;

            _calibOffsetX = (float)(point.Value.X - (box.X + box.Width / 2));
            _calibOffsetY = (float)(point.Value.Y - (box.Y + box.Height / 2));
            _calibrated = true;

            Preferences.Set("CalibOffsetX", _calibOffsetX);
            Preferences.Set("CalibOffsetY", _calibOffsetY);
            Preferences.Set("CalibActive", true);

            LblCalib.Text = $"Calibración activa: X={_calibOffsetX:0.0}, Y={_calibOffsetY:0.0}";
            LblCalib.TextColor = Colors.LimeGreen;
            LblEstado.Text = $"Calibración aplicada";
        }

        private void BtnResetCalib_Clicked(object sender, EventArgs e)
        {
            _calibOffsetX = 0;
            _calibOffsetY = 0;
            _calibrated = false;
            Preferences.Set("CalibOffsetX", 0f);
            Preferences.Set("CalibOffsetY", 0f);
            Preferences.Set("CalibActive", false);
            LblCalib.Text = "Calibración: Sin aplicar";
            LblCalib.TextColor = Colors.Goldenrod;
            LblEstado.Text = "Calibración desactivada";
        }

        // 🎨 Dibujo del marco de detección
        public sealed class BoundingDrawable : IDrawable
        {
            private readonly IReadOnlyCollection<RectF> _boxes;
            private readonly Func<Color> _getGuideColor;
            private float _scanPhase = 0;

            public BoundingDrawable(IReadOnlyCollection<RectF> boxes, Func<Color> getGuideColor)
            {
                _boxes = boxes;
                _getGuideColor = getGuideColor;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                _scanPhase = (_scanPhase + 0.02f) % 1f;

                var margin = 40f;
                var guide = new RectF(dirtyRect.X + margin, dirtyRect.Y + margin,
                                      dirtyRect.Width - 2 * margin, dirtyRect.Height - 2 * margin);

                canvas.FillColor = new Color(0, 0, 0, 0.45f);
                var path = new PathF();
                path.AppendRectangle(dirtyRect);
                path.AppendRoundedRectangle(guide, 18);
                path.Close();
                canvas.FillPath(path, WindingMode.EvenOdd);

                canvas.StrokeColor = _getGuideColor();
                canvas.StrokeSize = 4;
                canvas.DrawRoundedRectangle(guide, 18);

                foreach (var b in _boxes)
                {
                    canvas.StrokeColor = Colors.Lime;
                    canvas.StrokeSize = 3;
                    canvas.DrawRoundedRectangle(b, 12);

                    float lineY = b.Y + b.Height * _scanPhase;
                    canvas.DrawLine(b.X, lineY, b.X + b.Width, lineY);
                }
            }
        }
    }
}
