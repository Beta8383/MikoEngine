namespace MikoEngine.Assets;

using MikoEngine.Components;

public struct Uniform
{
    public MKMatrix4x4 modelTransform, projectionTransform, cameraTransform;
    public Light light;
    public Camera camera;
}