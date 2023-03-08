namespace MikoEngine;

public enum LightType
{
    Directional,
    Area,
    Point
}

public struct Light
{
    public MKVector3 Position;
    public MKVector3 Direction;
    public MKVector3 Color;
    public LightType Type;
    public float Intensity;
}