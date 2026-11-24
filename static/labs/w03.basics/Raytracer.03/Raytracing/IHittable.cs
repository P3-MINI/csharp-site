namespace Raytracing;

public interface IHittable
{
    bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo);
}
