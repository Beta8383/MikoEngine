namespace MikoEngine.Components;

public enum LightType
{
    Directional,
    Area
}

public struct Light
{
    public MKVector3 Position;
    public MKVector3 Color;
    public LightType Type;
    public float Intensity;
}