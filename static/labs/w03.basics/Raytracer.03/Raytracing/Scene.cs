using System.Collections.Generic;

namespace Raytracing;

public class Scene : IHittable
{
    public List<IHittable> Objects { get; } = new List<IHittable>();

    public void Add(IHittable obj)
    {
        Objects.Add(obj);
    }

    public void Clear()
    {
        Objects.Clear();
    }

    public bool Hit(Ray ray, float tMin, float tMax, out HitInfo hitInfo)
    {
        HitInfo tempHitInfo;
        bool hitAnything = false;
        float closestSoFar = tMax;
        hitInfo = new HitInfo();

        foreach (var obj in Objects)
        {
            if (obj.Hit(ray, tMin, closestSoFar, out tempHitInfo))
            {
                hitAnything = true;
                closestSoFar = tempHitInfo.T;
                hitInfo = tempHitInfo;
            }
        }

        return hitAnything;
    }
}
