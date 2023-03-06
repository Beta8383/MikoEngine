namespace MikoEngine.Components;

public unsafe class Scene
{
    List<Light> lights = new();
    List<Camera> cameras = new();
    int cameraIndex = -1;

    MKMatrix4x4 viewportTransform = MKMatrix4x4.Identity,
                cameraTransform = MKMatrix4x4.Identity,
                projectionTransform = MKMatrix4x4.Identity;

    public Scene AddLight(Light light)
    {
        lights.Add(light);
        return this;
    }

    public Scene AddModel<T>(ModelData<T> data, delegate*<in T, MKMatrix4x4, MKMatrix4x4, MKMatrix4x4, out MKVector4, void> shader) where T : unmanaged
    {
        
        return this;
    }

    public Scene AddCamera(Camera camera)
    {
        cameras.Add(camera);
        return this;
    }

    public Scene SetCamera(int index)
    {
        if (index < 0 || index > cameras.Count())
            throw new Exception();
        cameraIndex = index;
        return this;
    }
}