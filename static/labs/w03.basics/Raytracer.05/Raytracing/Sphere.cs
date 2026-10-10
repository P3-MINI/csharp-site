using System;

namespace Raytracing;

public class Sphere : IHittable
{
    public Vector3 Center { get; set; }
    public float Radius { get; set; }
    public Material Mat { get; set; }

    public Sphere(Vector3 center, float radius, Material mat)
    {
        Center = center;
        Radius = radius;
        Mat = mat;
    }

    public bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo)
    {
        hitInfo = new HitInfo();
        Vector3 oc = ray.Origin - Center;
        float a = ray.Direction.LengthSquared();
        float halfB = Vector3.Dot(oc, ray.Direction);
        float c = oc.LengthSquared() - Radius * Radius;

        float discriminant = halfB * halfB - a * c;
        if (discriminant < 0) return false;
        float sqrtd = MathF.Sqrt(discriminant);

        // Find the nearest root that lies in the acceptable range.
        float root = (-halfB - sqrtd) / a;
        if (root < tMin || tMax < root)
        {
            root = (-halfB + sqrtd) / a;
            if (root < tMin || tMax < root)
            {
                return false;
            }
        }

        hitInfo.T = root;
        hitInfo.Position = ray.At(hitInfo.T);
        Vector3 outwardNormal = (hitInfo.Position - Center) / Radius;
        hitInfo.SetFaceNormal(ray, outwardNormal);
        hitInfo.Mat = Mat;
        
        return true;
    }
}
