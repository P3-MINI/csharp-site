namespace Raytracing;

public class EmissiveMaterial : Material
{
    public Vector3 Color { get; set; }

    public EmissiveMaterial(Vector3 color)
    {
        Color = color;
    }

    public override bool Scatter(in Ray rIn, ref HitInfo hitInfo, out Vector3 attenuation, out Ray scattered)
    {
        attenuation = new Vector3(0, 0, 0);
        scattered = new Ray();
        return false;
    }

    public override Vector3 Emit()
    {
        return Color;
    }
}
