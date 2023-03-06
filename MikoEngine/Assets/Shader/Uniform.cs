namespace MikoEngine.Assets;

using MikoEngine.Components;

public struct Uniform
{
    public readonly MKMatrix4x4 modelTransform, projectionTransform, cameraTransform;
    public readonly Light light;
    public readonly Camera camera;
}