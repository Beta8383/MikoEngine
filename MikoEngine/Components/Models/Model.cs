namespace MikoEngine;

using MikoEngine.Assets;

public abstract class Model
{
    public IShader Shader = new LitShader();
    public float[] Data = Array.Empty<float>();
    public MKMatrix4x4 Transform = MKMatrix4x4.Identity;
}