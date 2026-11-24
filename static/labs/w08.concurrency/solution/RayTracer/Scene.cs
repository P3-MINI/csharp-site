using System;
using System.Collections.Generic;
using System.Numerics;

namespace RayTracer;

public class Scene
{
    public List<Sphere> Spheres { get; } = new();
    public Plane Ground { get; }

    public Vector3 LightPosition { get; }

    public Scene()
    {
        Random random = new Random();

        int sphereCount = random.Next(5, 10);

        for (int i = 0; i < sphereCount; i++)
        {
            float radius = 0.3f + (float)random.NextDouble() * 0.6f;

            float x = -2.5f + (float)random.NextDouble() * 5.0f;
            float z = -1.0f - (float)random.NextDouble() * 3.0f;
            float y = -1.0f + radius;

            ColorRGB color = new ColorRGB(
                0.2f + (float)random.NextDouble() * 0.8f,
                0.2f + (float)random.NextDouble() * 0.8f,
                0.2f + (float)random.NextDouble() * 0.8f
            );

            Spheres.Add(new Sphere(
                new Vector3(x, y, z),
                radius,
                color
            ));
        }

        Ground = new Plane(
            new Vector3(0, -1, 0),
            Vector3.UnitY,
            new ColorRGB(0.65f, 0.65f, 0.65f)
        );

        LightPosition = new Vector3(-3, 5, 4);
    }
}