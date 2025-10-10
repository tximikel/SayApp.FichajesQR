using BarcodeScanning;
using BarcodeScanning.Native.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Storage;
using System.Collections.ObjectModel;

/*
🧠 Conclusión técnica
Tu pipeline actual tiene todos los componentes funcionando:
- Módulo	Estado	Observaciones
- Cámara (preview)	✅ OK	Fluido, visible y con cambio de cámara
- Permisos Android	✅ OK	Concedido correctamente
- LifecycleOwner	✅ OK	Vinculado (sin crash)
- MLKit / BarcodeScanning.Native.Maui	✅ OK	Decodifica QR establemente
- Overlay + Guía visual	✅ OK	Se actualiza dinámicamente
- Logs de diagnóstico	✅ OK	Permiten depurar cualquier fallo futuro
*/

namespace SayApp.FichajesQR.LectorAndroid
{
    public partial class MainPage : ContentPage
    {
        private readonly ObservableCollection<RectF> _boxes = new();
        private readonly BoundingDrawable _drawable;
        private bool _hasDetection;
        private float _bufferWidth = 0;
        private float _bufferHeight = 0;
        private bool _bufferFixed = false;
        private float _calibOffsetX = 0;
        private float _calibOffsetY = 0;
        private bool _calibrated = false;

        public MainPage()
        {
            InitializeComponent();

            System.Diagnostics.Debug.WriteLine("📷 [INIT] MainPage constructor iniciado.");

#if ANDROID
            // Vincular LifecycleOwner tan pronto como el Handler esté disponible
            Camera.HandlerChanged += (s, e) =>
            {
                try
                {
                    var lifecycleOwner = Platform.CurrentActivity as AndroidX.Lifecycle.ILifecycleOwner;
                    if (lifecycleOwner != null && Camera?.Handler?.PlatformView != null)
                    {
                        var view = Camera.Handler.PlatformView;
                        var prop = view.GetType().GetProperty("LifecycleOwner");
                        if (prop != null)
                        {
                            prop.SetValue(view, lifecycleOwner);
                            System.Diagnostics.Debug.WriteLine("✅ [INIT] LifecycleOwner asignado (HandlerChanged).");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("⚠️ [WARN] Propiedad LifecycleOwner no encontrada en PlatformView.");
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ [WARN] No se pudo obtener lifecycleOwner o PlatformView es nulo.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] LifecycleOwner HandlerChanged: {ex.Message}");
                }
            };
#endif

            _drawable = new BoundingDrawable(_boxes, () => _hasDetection ? Color.FromArgb("#28a745") : Color.FromArgb("#C9A72B"));
            Overlay.Drawable = _drawable;

            // Suscribir eventos de cámara para seguimiento
            Camera.OnDetectionFinished += Camera_OnDetectionFinished;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine("📷 [ON_APPEARING] Página apareciendo...");
#if ANDROID
            // Vincula ciclo de vida Android → evita “Lifecycle is not set”
            var lifecycleOwner = Platform.CurrentActivity as AndroidX.Lifecycle.ILifecycleOwner;
            if (lifecycleOwner != null)
            {
                try
                {
                    var view = Camera.Handler?.PlatformView;
                    var prop = view?.GetType().GetProperty("LifecycleOwner");
                    if (prop != null)
                    {
                        prop.SetValue(view, lifecycleOwner);
                        System.Diagnostics.Debug.WriteLine("✅ [ON_APPEARING] LifecycleOwner vinculado correctamente al CameraView.");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("⚠️ [WARN] Propiedad LifecycleOwner no encontrada en PlatformView.");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[ERROR] Falló vinculación de LifecycleOwner: {ex.Message}");
                }
            }
#endif

            // 🪪 Permisos de cámara
            await Methods.AskForRequiredPermissionAsync();
            var camStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
            System.Diagnostics.Debug.WriteLine($"[0:] Permiso cámara: {camStatus}");

            if (camStatus == PermissionStatus.Granted)
            {
                await Task.Delay(800); // 🔄 pequeña espera para que se estabilice el preview

                Camera.CameraFacing = CameraFacing.Front;
                Camera.CameraEnabled = true;
                Camera.VibrationOnDetected = true;

                // 👇 habilita todos los formatos (inicializa correctamente MLKit)
                //Camera.BarcodeSymbologies = BarcodeFormats.QRCode;
                Camera.BarcodeSymbologies = BarcodeFormats.All;
                System.Diagnostics.Debug.WriteLine($"✅ [CAMERA] Barcode formats activos: {Camera.BarcodeSymbologies}");

                LblEstado.Text = "Estado: Escaneando...";
                System.Diagnostics.Debug.WriteLine("✅ [CAMERA] Cámara activada correctamente.");
            }
            else
            {
                Camera.CameraEnabled = false;
                LblEstado.Text = "Permiso de cámara denegado";
                System.Diagnostics.Debug.WriteLine("🚫 [CAMERA] Permiso de cámara denegado.");
            }

            _calibOffsetX = Preferences.Get("CalibOffsetX", 0f);
            _calibOffsetY = Preferences.Get("CalibOffsetY", 0f);
            _calibrated = Preferences.Get("CalibActive", false);

            if (_calibrated)
            {
                var calibDateStr = Preferences.Get("CalibDate", string.Empty);
                DateTime.TryParse(calibDateStr, out var calibDate);

                LblCalib.Text = $"Calibración activa: X={_calibOffsetX:0.0}, Y={_calibOffsetY:0.0}\n{calibDate:dd/MM/yyyy HH:mm:ss}";
                LblCalib.TextColor = Colors.LimeGreen;
                LblEstado.Text = $"Calibración aplicada: {_calibOffsetX:0.0}, {_calibOffsetY:0.0}";
            }
            else
            {
                LblCalib.Text = "Calibración: Sin aplicar";
                LblCalib.TextColor = Colors.Goldenrod;
                LblEstado.Text = "Calibración desactivada";
            }
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            Camera.CameraEnabled = false;
            LblEstado.Text = "Estado: Inactivo";
            _boxes.Clear();
            _hasDetection = false;
            Overlay.Invalidate();
            System.Diagnostics.Debug.WriteLine("📷 [ON_DISAPPEARING] Cámara desactivada y overlay limpiado.");
        }

        private void BtnStart_Clicked(object sender, EventArgs e)
        {
            Camera.CameraEnabled = true;
            //LblEstado.Text = "Estado: Escaneando...";
            System.Diagnostics.Debug.WriteLine("▶️ [BOTÓN] Cámara iniciada manualmente.");
        }

        private void BtnStop_Clicked(object sender, EventArgs e)
        {
            Camera.CameraEnabled = false;
            //LblEstado.Text = "Estado: Detenido";
            _boxes.Clear();
            _hasDetection = false;
            Overlay.Invalidate();
            System.Diagnostics.Debug.WriteLine("⏹️ [BOTÓN] Cámara detenida manualmente.");
        }

        private void BtnToggleCam_Clicked(object sender, EventArgs e)
        {
            Camera.CameraFacing = Camera.CameraFacing == CameraFacing.Front
                ? CameraFacing.Back
                : CameraFacing.Front;

            DisplayAlert("Cámara", $"Ahora: {Camera.CameraFacing}", "OK");
            System.Diagnostics.Debug.WriteLine($"🔄 [BOTÓN] Cámara cambiada a {Camera.CameraFacing}");
        }

        private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
        {
            var results = e.BarcodeResults?.ToArray() ?? Array.Empty<BarcodeResult>();
            _boxes.Clear();
            _hasDetection = results.Length > 0;

            if (!_hasDetection)
            {
                LblEstado.Text = "Estado: Escaneando...";
                Overlay.Invalidate();
                return;
            }

            var r = results[0];
            var value = r.DisplayValue ?? r.RawValue ?? string.Empty;
            var box = r.PreviewBoundingBox;

            // --- 🧭 1️⃣ Actualizar buffer de referencia ---
            float maxX = box.X + box.Width;
            float maxY = box.Y + box.Height;
            if (!_bufferFixed)
            {
                if (maxX > _bufferWidth) _bufferWidth = maxX;
                if (maxY > _bufferHeight) _bufferHeight = maxY;

                if (_bufferWidth > 1000 && _bufferHeight > 700)
                {
                    _bufferFixed = true;
                    System.Diagnostics.Debug.WriteLine($"[BUFFER] Fijado: {_bufferWidth}x{_bufferHeight}");
                }
            }
            if (_bufferWidth < 1 || _bufferHeight < 1)
                return;

            // --- 🧭 2️⃣ Normalizar orientación (portrait / landscape) ---
            var orientation = DeviceDisplay.MainDisplayInfo.Orientation;
            bool isPortrait = orientation == DisplayOrientation.Portrait;

            // MLKit suele entregar bounding box en landscape natural del sensor
            // Si estamos en portrait, rotamos 90º para alinear coordenadas
            if (isPortrait && _bufferWidth > _bufferHeight)
            {
                box = new RectF(
                    box.Y,
                    _bufferHeight - box.X - box.Width,
                    box.Height,
                    box.Width
                );
            }

            // --- 🧭 3️⃣ Reflejo horizontal si cámara frontal ---
            if (Camera.CameraFacing == CameraFacing.Front)
            {
                box = new RectF(
                    _bufferWidth - box.X - box.Width,
                    box.Y,
                    box.Width,
                    box.Height
                );
            }

            // --- 🧭 4️⃣ Escalado y ajuste de bandas negras ---
            float overlayWidth = (float)Overlay.Width;
            float overlayHeight = (float)Overlay.Height;
            float bufferAspect = _bufferWidth / _bufferHeight;
            float overlayAspect = overlayWidth / overlayHeight;

            float scale;
            float offsetX = 0, offsetY = 0;

            if (overlayAspect > bufferAspect)
            {
                // overlay más ancho → bandas verticales (pillarbox)
                scale = overlayHeight / _bufferHeight;
                float usedWidth = _bufferWidth * scale;
                offsetX = (overlayWidth - usedWidth) / 2;
            }
            else
            {
                // overlay más alto → bandas horizontales (letterbox)
                scale = overlayWidth / _bufferWidth;
                float usedHeight = _bufferHeight * scale;
                offsetY = (overlayHeight - usedHeight) / 2;
            }

            // --- 🧭 5️⃣ Aplicar escalado + calibración ---
            var transformedBox = new RectF(
                box.X * scale + offsetX + (_calibrated ? _calibOffsetX : 0),
                box.Y * scale + offsetY + (_calibrated ? _calibOffsetY : 0),
                box.Width * scale,
                box.Height * scale
            );

            _boxes.Add(transformedBox);

            // --- 🧭 6️⃣ Actualizar estado y UI ---
            LblUltimo.Text = $"Último QR leído: {value}";
            LblEstado.Text = "QR detectado correctamente";

            Overlay.Invalidate();
        }

        private void Overlay_Tapped(object sender, TappedEventArgs e)
        {
            if (_boxes.Count == 0) return;

            var box = _boxes[0];
            var boxCenterX = box.X + box.Width / 2;
            var boxCenterY = box.Y + box.Height / 2;

            var point = e.GetPosition((View)sender);
            var touchX = point?.X ?? 0;
            var touchY = point?.Y ?? 0;

            _calibOffsetX = (float)(touchX - boxCenterX);
            _calibOffsetY = (float)(touchY - boxCenterY);
            _calibrated = true;

            Preferences.Set("CalibOffsetX", _calibOffsetX);
            Preferences.Set("CalibOffsetY", _calibOffsetY);
            Preferences.Set("CalibActive", true);

            // Actualiza el label de calibración y el estado
            LblCalib.Text = $"Calibración activa: X={_calibOffsetX:0.0}, Y={_calibOffsetY:0.0}\n{DateTime.Now:dd/MM/yyyy HH:mm:ss}";
            LblCalib.TextColor = Colors.LimeGreen;
            LblEstado.Text = $"Calibración aplicada: {_calibOffsetX:0.0}, {_calibOffsetY:0.0}";

            var now = DateTime.Now;
            Preferences.Set("CalibDate", now.ToString("yyyy-MM-dd HH:mm:ss"));

            System.Diagnostics.Debug.WriteLine($"[CALIBRACIÓN] Offset: {_calibOffsetX}, {_calibOffsetY}");
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

        // 🎨 Dibuja el marco de detección + bounding boxes con suavizado y animación
        public sealed class BoundingDrawable : IDrawable
        {
            private readonly IReadOnlyCollection<RectF> _boxes;
            private readonly Func<Color> _getGuideColor;

            // 🧭 Estado para animación suave
            private readonly Dictionary<int, RectF> _previousBoxes = new();
            private readonly float _lerpSpeed = 0.25f; // 0.0f = sin interpolación, 1.0f = salto instantáneo
            private float _scanPhase = 0;

            public BoundingDrawable(IReadOnlyCollection<RectF> boxes, Func<Color> getGuideColor)
            {
                _boxes = boxes;
                _getGuideColor = getGuideColor;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                // Actualizar animación del escáner
                _scanPhase = (_scanPhase + 0.02f) % 1f;

                // --- 🟩 Área guía general ---
                var margin = 40f;
                var guide = new RectF(
                    dirtyRect.X + margin,
                    dirtyRect.Y + margin,
                    dirtyRect.Width - 2 * margin,
                    dirtyRect.Height - 2 * margin);

                // sombreado exterior (oscurece fuera del marco)
                canvas.FillColor = new Color(0, 0, 0, 0.45f);
                var path = new PathF();
                path.AppendRectangle(dirtyRect);
                path.AppendRoundedRectangle(guide, 18);
                path.Close();
                canvas.FillPath(path, WindingMode.EvenOdd);

                // marco guía visible
                canvas.StrokeColor = _getGuideColor();
                canvas.StrokeSize = 4;
                canvas.DrawRoundedRectangle(guide, 18);

                // --- 🟨 Dibujar bounding boxes con interpolación suave ---
                int index = 0;
                foreach (var b in _boxes)
                {
                    var target = b;
                    RectF current;

                    if (_previousBoxes.TryGetValue(index, out var prev))
                    {
                        // interpolación lineal entre posición anterior y actual
                        current = new RectF(
                            Lerp(prev.X, target.X, _lerpSpeed),
                            Lerp(prev.Y, target.Y, _lerpSpeed),
                            Lerp(prev.Width, target.Width, _lerpSpeed),
                            Lerp(prev.Height, target.Height, _lerpSpeed)
                        );
                        _previousBoxes[index] = current;
                    }
                    else
                    {
                        _previousBoxes[index] = target;
                        current = target;
                    }

                    // borde verde con transparencia
                    canvas.StrokeColor = new Color(0, 1, 0, 0.9f);
                    canvas.StrokeSize = 5;
                    canvas.DrawRoundedRectangle(current, 12);

                    // efecto escáner animado (línea horizontal que cruza el QR)
                    float lineY = current.Y + current.Height * _scanPhase;
                    canvas.StrokeColor = Colors.Lime;
                    canvas.StrokeSize = 2;
                    canvas.DrawLine(current.X, lineY, current.X + current.Width, lineY);

                    index++;
                }

                // limpiar cajas sobrantes si desaparecen
                var activeKeys = _boxes.Select((_, i) => i).ToHashSet();
                var obsolete = _previousBoxes.Keys.Except(activeKeys).ToList();
                foreach (var key in obsolete)
                    _previousBoxes.Remove(key);
            }

            private static float Lerp(float from, float to, float t) =>
                from + (to - from) * t;
        }
    }
}
