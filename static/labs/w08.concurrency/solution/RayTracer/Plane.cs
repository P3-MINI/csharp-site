using System;
using System.Numerics;

namespace RayTracer;

public class Plane
{
    public Vector3 Point { get; }
    public Vector3 Normal { get; }
    public ColorRGB Color { get; }

    public Plane(Vector3 point, Vector3 normal, ColorRGB color)
    {
        Point = point;
        Normal = Vector3.Normalize(normal);
        Color = color;
    }

    public bool Intersect(Ray ray, out float distance)
    {
        float denominator = Vector3.Dot(Normal, ray.Direction);

        if (MathF.Abs(denominator) < 0.0001f)
        {
            distance = 0;
            return false;
        }

        distance = Vector3.Dot(Point - ray.Origin, Normal) / denominator;

        return distance > 0.001f;
    }
}