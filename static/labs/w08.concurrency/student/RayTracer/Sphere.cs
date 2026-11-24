using System;
using System.Numerics;

namespace RayTracer;

public class Sphere
{
    public Vector3 Center { get; }
    public float Radius { get; }
    public ColorRGB Color { get; }

    public Sphere(Vector3 center, float radius, ColorRGB color)
    {
        Center = center;
        Radius = radius;
        Color = color;
    }

    public bool Intersect(Ray ray, out float distance)
    {
        Vector3 oc = ray.Origin - Center;

        float a = Vector3.Dot(ray.Direction, ray.Direction);
        float b = 2.0f * Vector3.Dot(oc, ray.Direction);
        float c = Vector3.Dot(oc, oc) - Radius * Radius;

        float discriminant = b * b - 4 * a * c;

        if (discriminant < 0)
        {
            distance = 0;
            return false;
        }

        float sqrt = MathF.Sqrt(discriminant);

        float t1 = (-b - sqrt) / (2 * a);
        float t2 = (-b + sqrt) / (2 * a);

        distance = float.MaxValue;

        if (t1 > 0.001f)
            distance = t1;

        if (t2 > 0.001f && t2 < distance)
            distance = t2;

        return distance != float.MaxValue;
    }

    public Vector3 NormalAt(Vector3 point)
    {
        return Vector3.Normalize(point - Center);
    }
}