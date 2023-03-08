namespace MikoEngine;

public enum ProjectionMode
{
    Orthographic,
    Perspective
}

public struct Camera
{
    public MKVector3 Position, Up, Direction;
    public float zNearPlane, zFarPlane, Width, Height;
    public ProjectionMode Mode;
}