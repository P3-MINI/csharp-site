using System;

namespace Raytracing;

public class Lambertian : Material
{
    public Vector3 Albedo { get; set; }

    public Lambertian(Vector3 albedo)
    {
        Albedo = albedo;
    }

    public override bool Scatter(in Ray rIn, ref HitInfo hitInfo, out Vector3 attenuation, out Ray scattered)
    {
        Vector3 scatterDirection = hitInfo.Normal + Vector3.RandomUnitVector();

        // Catch degenerate scatter direction
        if (scatterDirection.NearZero())
        {
            scatterDirection = hitInfo.Normal;
        }

        scattered = new Ray(hitInfo.Position, scatterDirection);
        attenuation = Albedo;
        return true;
    }
}
