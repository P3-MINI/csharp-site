namespace Raytracing;

public struct Vector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 operator -(Vector3 v) => new Vector3(-v.X, -v.Y, -v.Z);
    
    public static Vector3 operator +(Vector3 u, Vector3 v) => new Vector3(u.X + v.X, u.Y + v.Y, u.Z + v.Z);
    
    public static Vector3 operator -(Vector3 u, Vector3 v) => new Vector3(u.X - v.X, u.Y - v.Y, u.Z - v.Z);
    
    public static Vector3 operator *(Vector3 u, Vector3 v) => new Vector3(u.X * v.X, u.Y * v.Y, u.Z * v.Z);
    
    public static Vector3 operator *(float t, Vector3 v) => new Vector3(t * v.X, t * v.Y, t * v.Z);
    
    public static Vector3 operator *(Vector3 v, float t) => t * v;
    
    public static Vector3 operator /(Vector3 v, float t) => (1.0f / t) * v;

    public float Length() => MathF.Sqrt(LengthSquared());

    public float LengthSquared() => X * X + Y * Y + Z * Z;

    public static float Dot(Vector3 u, Vector3 v) => u.X * v.X + u.Y * v.Y + u.Z * v.Z;

    public static Vector3 Cross(Vector3 u, Vector3 v) => new Vector3(
        u.Y * v.Z - u.Z * v.Y,
        u.Z * v.X - u.X * v.Z,
        u.X * v.Y - u.Y * v.X);

    public Vector3 Normalize() => this / Length();

    public bool NearZero()
    {
        float s = 1e-8f;
        return (MathF.Abs(X) < s) && (MathF.Abs(Y) < s) && (MathF.Abs(Z) < s);
    }

    public static Vector3 RandomInUnitSphere()
    {
        while (true)
        {
            Vector3 p = new Vector3(
                (float)Random.Shared.NextDouble() * 2 - 1,
                (float)Random.Shared.NextDouble() * 2 - 1,
                (float)Random.Shared.NextDouble() * 2 - 1);
            if (p.LengthSquared() < 1)
                return p;
        }
    }

    public static Vector3 RandomUnitVector()
    {
        while (true)
        {
            Vector3 p = RandomInUnitSphere();
            if (!p.NearZero())
                return p.Normalize();
        }
    }

    public override string ToString() => $"{X} {Y} {Z}";
}