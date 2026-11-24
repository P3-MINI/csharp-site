using System;

namespace RayTracer;

public readonly struct ColorRGB
{
    public float R { get; }
    public float G { get; }
    public float B { get; }

    public ColorRGB(float r, float g, float b)
    {
        R = r;
        G = g;
        B = b;
    }

    public static ColorRGB operator +(ColorRGB a, ColorRGB b)
    {
        return new ColorRGB(
            a.R + b.R,
            a.G + b.G,
            a.B + b.B
        );
    }

    public static ColorRGB operator *(ColorRGB color, float value)
    {
        return new ColorRGB(
            color.R * value,
            color.G * value,
            color.B * value
        );
    }

    public ColorRGB Clamp()
    {
        return new ColorRGB(
            Math.Clamp(R, 0, 1),
            Math.Clamp(G, 0, 1),
            Math.Clamp(B, 0, 1)
        );
    }

    public byte R8 => (byte)(Math.Clamp(R, 0, 1) * 255);
    public byte G8 => (byte)(Math.Clamp(G, 0, 1) * 255);
    public byte B8 => (byte)(Math.Clamp(B, 0, 1) * 255);
}