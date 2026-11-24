namespace RayTracer.Views;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using global::RayTracer;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private WriteableBitmap CreateBitmap(ColorRGB[,] pixels)
    {
        int width = pixels.GetLength(0);
        int height = pixels.GetLength(1);

        WriteableBitmap bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Avalonia.Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque
        );

        using (var framebuffer = bitmap.Lock())
        {
            byte[] row = new byte[width * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ColorRGB color = pixels[x, y];
                    int i = x * 4;

                    row[i] = color.B8;
                    row[i + 1] = color.G8;
                    row[i + 2] = color.R8;
                    row[i + 3] = 255;
                }

                Marshal.Copy(
                    row,
                    0,
                    IntPtr.Add(
                        framebuffer.Address,
                        y * framebuffer.RowBytes
                    ),
                    row.Length
                );
            }
        }

        return bitmap;
    }

    private async void RenderButton_Click(
    object? sender,
    RoutedEventArgs e)
    {
        const int width = 800;
        const int height = 600;
        const int samplesPerPixel = 4;

        RenderButton.IsEnabled = false;
        RenderProgress.Value = 0;
        ProgressText.Text = "Starting render...";

        try
        {
            // Create a new scene and camera for this render.
            Scene scene = new Scene();

            Camera camera = new Camera(
                new Vector3(0, 1, 6),
                new Vector3(0, 0, 0),
                Vector3.UnitY
            );

            RayTracer rayTracer = new RayTracer(scene, camera);

            // Progress<T> captures the current UI synchronization context.
            IProgress<RenderFrame> progress =
                new Progress<RenderFrame>(frame =>
                {
                    // Display the newly generated frame.
                    RenderImage.Source = CreateBitmap(frame.Pixels);

                    RenderProgress.Value =
                        frame.SamplesPerPixel * 100.0 / samplesPerPixel;

                    ProgressText.Text =
                        $"Rendering: {frame.SamplesPerPixel} samples per pixel";
                });

            // Perform the expensive CPU-bound work on a worker thread.
            await Task.Run(() =>
                rayTracer.RenderProgressive(
                    width,
                    height,
                    samplesPerPixel,
                    progress
                )
            );

            RenderProgress.Value = 100;
            ProgressText.Text = "Rendering complete.";
        }
        catch (Exception ex)
        {
            ProgressText.Text = $"Rendering failed: {ex.Message}";
        }
        finally
        {
            RenderButton.IsEnabled = true;
        }
    }
}

