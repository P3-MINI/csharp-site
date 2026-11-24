using System;
using System.Numerics;

namespace RayTracer;

public class RayTracer
{
    private readonly Scene _scene;
    private readonly Camera _camera;

    public RayTracer(Scene scene, Camera camera)
    {
        _scene = scene;
        _camera = camera;
    }


    public void RenderProgressive(
    int width,
    int height,
    int maxSamplesPerPixel,
    IProgress<RenderFrame> progress)
    {
        ColorRGB[,] accumulated = new ColorRGB[width, height];
        Random random = new Random();

        for (int sample = 1; sample <= maxSamplesPerPixel; sample++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float sampleX = x + (float)random.NextDouble();
                    float sampleY = y + (float)random.NextDouble();

                    Ray ray = _camera.GetRay(
                        sampleX, sampleY, width, height);

                    accumulated[x, y] += Trace(ray);
                }
            }

            // Tworzymy osobną kopię obrazu dla danego etapu.
            ColorRGB[,] framePixels = new ColorRGB[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    framePixels[x, y] =
                        accumulated[x, y] * (1.0f / sample);
                }
            }

            progress.Report(new RenderFrame(framePixels, sample));
        }
    }


    private ColorRGB Trace(Ray ray)
    {
        float closestDistance = float.MaxValue;
        ColorRGB objectColor = new ColorRGB(0.1f, 0.1f, 0.1f);
        Vector3 hitPoint = default;
        Vector3 normal = default;

        foreach (Sphere sphere in _scene.Spheres)
        {
            if (sphere.Intersect(ray, out float distance) &&
                distance < closestDistance)
            {
                closestDistance = distance;
                hitPoint = ray.At(distance);
                normal = sphere.NormalAt(hitPoint);
                objectColor = sphere.Color;
            }
        }

        if (_scene.Ground.Intersect(ray, out float planeDistance) &&
            planeDistance < closestDistance)
        {
            closestDistance = planeDistance;
            hitPoint = ray.At(planeDistance);
            normal = _scene.Ground.Normal;
            objectColor = _scene.Ground.Color;
        }

        if (closestDistance == float.MaxValue)
        {
            return Background(ray);
        }

        return Shade(hitPoint, normal, objectColor);
    }

    private ColorRGB Shade(
        Vector3 point,
        Vector3 normal,
        ColorRGB objectColor)
    {
        Vector3 toLight =
            Vector3.Normalize(_scene.LightPosition - point);

        float diffuse =
            MathF.Max(0, Vector3.Dot(normal, toLight));

        bool shadowed = IsShadowed(point, toLight);

        if (shadowed)
            diffuse *= 0.15f;

        float ambient = 0.15f;

        return objectColor * (ambient + diffuse * 0.85f);
    }

    private bool IsShadowed(
        Vector3 point,
        Vector3 directionToLight)
    {
        float lightDistance =
            Vector3.Distance(
                point,
                _scene.LightPosition
            );

        Ray shadowRay = new Ray(
            point + directionToLight * 0.001f,
            directionToLight
        );

        foreach (Sphere sphere in _scene.Spheres)
        {
            if (sphere.Intersect(
                    shadowRay,
                    out float distance) &&
                distance < lightDistance)
            {
                return true;
            }
        }

        return false;
    }

    private ColorRGB Background(Ray ray)
    {
        float t = 0.5f * (ray.Direction.Y + 1.0f);

        ColorRGB top =
            new ColorRGB(0.35f, 0.55f, 0.85f);

        ColorRGB bottom =
            new ColorRGB(0.9f, 0.9f, 0.95f);

        return bottom * (1 - t) + top * t;
    }
}