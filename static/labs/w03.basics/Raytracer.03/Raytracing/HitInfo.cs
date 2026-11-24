namespace Raytracing;

public struct HitInfo
{
    public Vector3 Position;
    public Vector3 Normal;
    public float T;
    public bool FrontFace;

    public void SetFaceNormal(Ray r, Vector3 outwardNormal)
    {
        FrontFace = Vector3.Dot(r.Direction, outwardNormal) < 0;
        Normal = FrontFace ? outwardNormal : -outwardNormal;
    }
}
