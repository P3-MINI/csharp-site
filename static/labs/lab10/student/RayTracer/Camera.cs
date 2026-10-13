using System;
using System.Numerics;

namespace RayTracer;

public class Camera
{
    public Vector3 Position { get; }
    public Vector3 Forward { get; }
    public Vector3 Up { get; }

    private readonly Vector3 _right;

    public Camera(
        Vector3 position,
        Vector3 target,
        Vector3 up)
    {
        Position = position;

        Forward = Vector3.Normalize(target - position);
        _right = Vector3.Normalize(Vector3.Cross(Forward, up));
        Up = Vector3.Normalize(Vector3.Cross(_right, Forward));
    }

    public Ray GetRay(
    float sampleX,
    float sampleY,
    int width,
    int height)
    {
        float aspectRatio = (float)width / height;
        float fov = MathF.PI / 3.0f;

        float px =
            (2.0f * (sampleX / width) - 1.0f)
            * aspectRatio
            * MathF.Tan(fov / 2.0f);

        float py =
            (1.0f - 2.0f * (sampleY / height))
            * MathF.Tan(fov / 2.0f);

        Vector3 direction =
            Forward + _right * px + Up * py;

        return new Ray(Position, direction);
    }
}