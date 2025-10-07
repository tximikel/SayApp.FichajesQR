
using BarcodeScanning;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.ObjectModel;

namespace SayApp.FichajesQR.LectorAndroid;

public partial class MainPage : ContentPage
{
    // Lista de rectángulos a pintar (si quieres dibujar bounding boxes reales más adelante)
    private readonly ObservableCollection<RectF> _boxes = new();
    private readonly BoundingDrawable _drawable;

    // Estado del visor (guía). Cambia a verde cuando detecta
    private bool _hasDetection;

    public MainPage()
    {
        InitializeComponent();
        _drawable = new BoundingDrawable(_boxes, getGuideColor: () => _hasDetection ? Color.FromArgb("#249429") : Color.FromArgb("#C9A72B"));
        Overlay.Drawable = _drawable;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Methods.AskForRequiredPermissionAsync(); // Pide permisos de cámara
        Camera.CameraEnabled = true;
        LblEstado.Text = "Estado: Escaneando...";
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Camera.CameraEnabled = false;
        LblEstado.Text = "Estado: Inactivo";
        _boxes.Clear();
        _hasDetection = false;
        Overlay.Invalidate();
    }

    private void BtnStart_Clicked(object sender, EventArgs e)
    {
        Camera.CameraEnabled = true;
        LblEstado.Text = "Estado: Escaneando...";
    }

    private void BtnStop_Clicked(object sender, EventArgs e)
    {
        Camera.CameraEnabled = false;
        LblEstado.Text = "Estado: Detenido";
        _boxes.Clear();
        _hasDetection = false;
        Overlay.Invalidate();
    }

    private void BtnToggleCam_Clicked(object sender, EventArgs e)
    {
        // TODO: Implementar cuando fijes la propiedad de selección de cámara de la librería
        DisplayAlert("Cámara", "Cambio de cámara pendiente de implementación en esta versión.", "OK");
    }

    private void Camera_OnDetectionFinished(object sender, OnDetectionFinishedEventArg e)
    {
        // Copia los resultados antes de cualquier operación async (evita condiciones de carrera)
        var results = e.BarcodeResults?.ToArray() ?? Array.Empty<BarcodeResult>();

        // Si quieres dibujar bounding boxes, añade aquí los RectF de cada resultado
        _boxes.Clear();
        // TODO: Cuando confirmes la propiedad exacta de la caja transformada, agrega:
        // foreach (var r in results) _boxes.Add(r.BoundingBox);

        _hasDetection = results.Length > 0;
        if (_hasDetection)
        {
            var first = results[0];
            LblUltimo.Text = $"Último QR leído: {first.DisplayValue}";
            LblEstado.Text = "Estado: Detectado";
        }
        else
        {
            LblEstado.Text = "Estado: Escaneando...";
        }

        Overlay.Invalidate();
    }
}

// Drawable del overlay: visor (guía) + bounding boxes si las hay
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
        // (1) Dibuja el visor (recuadro guía) en la zona derecha
        canvas.StrokeColor = _getGuideColor();
        canvas.StrokeSize = 4;
        var margin = 60f;
        var guide = new RectF(dirtyRect.X + margin, dirtyRect.Y + margin,
                              dirtyRect.Width - 2 * margin, dirtyRect.Height - 2 * margin);
        canvas.DrawRoundedRectangle(guide, 16);

        // (2) Dibuja las bounding boxes detectadas (si has rellenado _boxes)
        foreach (var b in _boxes)
        {
            canvas.StrokeColor = Colors.Lime;
            canvas.StrokeSize = 5;
            canvas.DrawRoundedRectangle(b, 10);
        }
    }
}
