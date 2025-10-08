using BarcodeScanning;
using BarcodeScanning.Native.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
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

                Camera.CameraFacing = CameraFacing.Back;
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
            LblEstado.Text = "Estado: Escaneando...";
            System.Diagnostics.Debug.WriteLine("▶️ [BOTÓN] Cámara iniciada manualmente.");
        }

        private void BtnStop_Clicked(object sender, EventArgs e)
        {
            Camera.CameraEnabled = false;
            LblEstado.Text = "Estado: Detenido";
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
            try
            {
                System.Diagnostics.Debug.WriteLine("📸 [EVENT] OnDetectionFinished ejecutado.");
                var results = e.BarcodeResults?.ToArray() ?? Array.Empty<BarcodeResult>();
                _boxes.Clear();
                _hasDetection = results.Length > 0;

                if (_hasDetection)
                {
                    var r = results[0];
                    var value = r.DisplayValue ?? r.RawValue ?? string.Empty;
                    var box = r.PreviewBoundingBox;

                    System.Diagnostics.Debug.WriteLine($"✅ [QR DETECTADO] Valor={value}, Box={box.Width}x{box.Height}");

                    if (box.Width > 0 && box.Height > 0)
                    {
                        _boxes.Add(box);

                        double previewArea = Camera.Width * Camera.Height;
                        double qrArea = box.Width * box.Height;
                        double percent = previewArea > 0 ? qrArea / previewArea : 0;

                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            LblUltimo.Text = $"Último QR leído: {value}";

                            if (percent < 0.05)
                                LblEstado.Text = "📏 Acércalo un poco al lector";
                            else if (percent > 0.3)
                                LblEstado.Text = "📏 Aléjalo un poco";
                            else
                                LblEstado.Text = "✅ QR detectado correctamente";
                        });
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("🔎 [SCAN] Ningún código detectado en este frame.");
                    MainThread.BeginInvokeOnMainThread(() => LblEstado.Text = "Estado: Escaneando...");
                }

                Overlay.Invalidate();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ERROR] OnDetectionFinished: {ex}");
            }
        }

        // 🎨 Dibuja el marco de detección + bounding boxes
        public sealed class BoundingDrawable : IDrawable
        {
            private readonly IReadOnlyCollection<RectF> _boxes;
            private readonly Func<Color> _getGuideColor;

            public BoundingDrawable(IReadOnlyCollection<RectF> boxes, Func<Color> getGuideColor)
            {
                _boxes = boxes;
                _getGuideColor = getGuideColor;
            }

            public void Draw(ICanvas canvas, RectF dirtyRect)
            {
                var margin = 40f;
                var guide = new RectF(
                    dirtyRect.X + margin,
                    dirtyRect.Y + margin,
                    dirtyRect.Width - 2 * margin,
                    dirtyRect.Height - 2 * margin);

                // sombreado exterior
                canvas.FillColor = new Color(0, 0, 0, 0.4f);
                var path = new PathF();
                path.AppendRectangle(dirtyRect);
                path.AppendRoundedRectangle(guide, 16);
                path.Close();
                canvas.FillPath(path, WindingMode.EvenOdd);

                // marco principal
                canvas.StrokeColor = _getGuideColor();
                canvas.StrokeSize = 4;
                canvas.DrawRoundedRectangle(guide, 16);

                // recuadro QR detectado
                foreach (var b in _boxes)
                {
                    canvas.StrokeColor = Colors.Lime;
                    canvas.StrokeSize = 5;
                    canvas.DrawRoundedRectangle(b, 10);
                }
            }
        }
    }
}
