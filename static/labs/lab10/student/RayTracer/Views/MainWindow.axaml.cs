namespace WarehouseDelivery.Views;

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

    private void RenderButton_Click(object? sender, RoutedEventArgs e)
    {
        const int width = 800;
        const int height = 600;

        // Create the scene and camera.
        Scene scene = new Scene();

        Camera camera = new Camera(
            new Vector3(0, 1, 6),
            new Vector3(0, 0, 0),
            Vector3.UnitY
        );

        // Render the image.
        RayTracer rayTracer = new RayTracer(scene, camera);
        ColorRGB[,] pixels = rayTracer.Render(width, height, 16);

        // Create an Avalonia bitmap.
        WriteableBitmap bitmap = new WriteableBitmap(
            new PixelSize(width, height),
            new Avalonia.Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Opaque
        );

        // Copy the rendered pixels into the bitmap.
        using (var framebuffer = bitmap.Lock())
        {
            byte[] row = new byte[width * 4];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    ColorRGB color = pixels[x, y];

                    int i = x * 4;

                    row[i + 0] = color.B8;
                    row[i + 1] = color.G8;
                    row[i + 2] = color.R8;
                    row[i + 3] = 255;
                }

                Marshal.Copy(
                    row,
                    0,
                    IntPtr.Add(framebuffer.Address, y * framebuffer.RowBytes),
                    row.Length
                );
            }
        }

        // Display the bitmap.
        RenderImage.Source = bitmap;
    }
}
