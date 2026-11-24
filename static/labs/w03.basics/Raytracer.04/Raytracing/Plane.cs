using System;

namespace Raytracing;

public class Plane : IHittable
{
    public Vector3 Point { get; set; }
    public Vector3 Normal { get; set; }
    public Material Mat { get; set; }

    public Plane(Vector3 point, Vector3 normal, Material mat)
    {
        Point = point;
        Normal = normal.Normalize();
        Mat = mat;
    }

    public bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo)
    {
        hitInfo = new HitInfo();
        float denom = Vector3.Dot(Normal, ray.Direction);

        if (MathF.Abs(denom) > 1e-6f)
        {
            float t = Vector3.Dot(Point - ray.Origin, Normal) / denom;
            if (t > tMin && t < tMax)
            {
                hitInfo.T = t;
                hitInfo.Position = ray.At(t);
                hitInfo.SetFaceNormal(ray, Normal);
                hitInfo.Mat = Mat;
                return true;
            }
        }

        return false;
    }
}
