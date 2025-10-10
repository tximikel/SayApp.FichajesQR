using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;
using System.Linq;
using BarcodeScanning; // ✅ v1.2.5

namespace SayApp.FichajesQR.LectorAndroid
{
    public partial class MainPage : ContentPage
    {
        private readonly ObservableCollection<RectF> _boxes = new();
        private readonly BoundingDrawable _drawable;
        private bool _hasDetection;
        private bool _calibrated;
        private float _calibOffsetX, _calibOffsetY;

        // Tamaño fuente del frame que usa MLKit (fallback típico)
        const float SourceWidth = 1080f;
        const float SourceHeight = 1920f;

        public MainPage()
        {
            InitializeComponent();

            _drawable = new BoundingDrawable(_boxes, () => _hasDetection ? Colors.LimeGreen : Colors.Goldenrod);
            Overlay.Drawable = _drawable;

            Camera.OnDetectionFinished += Camera_OnDetectionFinished;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var camStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (camStatus != PermissionStatus.Granted)
                await Permissions.RequestAsync<Permissions.Camera>();

            // ✅ Forzar cámara frontal y detección activa
            Camera.CameraFacing = CameraFacing.Front;
            Camera.CameraEnabled = true;

            // Cargar calibración guardada
            _calibOffsetX = Preferences.Get("CalibOffsetX", 0f);
            _calibOffsetY = Preferences.Get("CalibOffsetY", 0f);
            _calibrated = Preferences.Get("CalibActive", false);

            LblEstado.Text = "Escaneando...";

            // ❌ Quitamos el cuadro temporal para no confundir el overlay
            _boxes.Clear();
            Overlay.Invalidate();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Camera.OnDetectionFinished -= Camera_OnDetectionFinished;
            _boxes.Clear();
            Overlay.Invalidate();
        }

        private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var results = e.BarcodeResults?.ToArray() ?? Array.Empty<BarcodeResult>();

                _boxes.Clear();
                _hasDetection = results.Length > 0;

                if (!_hasDetection)
                {
                    LblEstado.Text = "Escaneando...";
                    Overlay.Invalidate();
                    return;
                }

                float viewW = (float)Overlay.Width;
                float viewH = (float)Overlay.Height;

                // Escala tipo "aspect fit" del frame fuente al área del Overlay
                float scale = Math.Min(viewW / SourceWidth, viewH / SourceHeight);
                float scaledW = SourceWidth * scale;
                float scaledH = SourceHeight * scale;
                float offX = (viewW - scaledW) / 2f;
                float offY = (viewH - scaledH) / 2f;

                bool mirrorX = Camera.CameraFacing == CameraFacing.Front;

                foreach (var r in results)
                {
                    var b = r.BoundingBox; // coords en píxeles del frame fuente

                    // 1) Escalar a la superficie escalada
                    float x = offX + b.X * scale;
                    float y = offY + b.Y * scale;
                    float w = b.Width * scale;
                    float h = b.Height * scale;

                    // 2) Espejar en X (dentro del rectángulo escalado), para cámara frontal
                    if (mirrorX)
                    {
                        x = offX + (scaledW - (x - offX) - w);
                    }

                    // 3) Aplicar offset de calibración (si lo tienes activo)
                    if (_calibrated)
                    {
                        x += _calibOffsetX;
                        y += _calibOffsetY;
                    }

                    _boxes.Add(new RectF(x, y, w, h));
                }

                var first = results[0];
                var value = first.DisplayValue ?? first.RawValue ?? "(sin valor)";
                LblUltimo.Text = $"Último QR leído: {value}";
                LblEstado.Text = $"Detectado {results.Length} código(s)";

                Overlay.Invalidate();
            });
        }

        private void BtnStart_Clicked(object sender, EventArgs e)
        {
            Camera.CameraEnabled = true;
            LblEstado.Text = "Escaneando...";
        }

        private void BtnStop_Clicked(object sender, EventArgs e)
        {
            Camera.CameraEnabled = false;
            _boxes.Clear();
            _hasDetection = false;
            Overlay.Invalidate();
            LblEstado.Text = "Detenido";
        }

        private void BtnToggleCam_Clicked(object sender, EventArgs e)
        {
            Camera.CameraFacing = Camera.CameraFacing == CameraFacing.Front
                ? CameraFacing.Back
                : CameraFacing.Front;

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
            Preferences.Set("CalibDate", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            LblCalib.Text = $"Calibración activa: X={_calibOffsetX:0.0}, Y={_calibOffsetY:0.0}\n{DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            LblCalib.TextColor = Colors.LimeGreen;
            LblEstado.Text = $"Calibración aplicada: {_calibOffsetX:0.0}, {_calibOffsetY:0.0}";
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

        // 🎨 Dibujo del overlay (ya recibe cajas en coordenadas de pantalla)
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

                canvas.FillColor = new Color(0, 0, 0, 0.35f);
                canvas.FillRectangle(dirtyRect);

                foreach (var b in _boxes)
                {
                    // Recuadro
                    canvas.StrokeColor = _getGuideColor();
                    canvas.StrokeSize = 3;
                    canvas.DrawRoundedRectangle(b, 12);

                    // “Láser”
                    float lineY = b.Y + b.Height * _scanPhase;
                    canvas.StrokeColor = Colors.Red;
                    canvas.StrokeSize = 2;
                    canvas.DrawLine(b.X, lineY, b.X + b.Width, lineY);
                }
            }
        }
    }
}
