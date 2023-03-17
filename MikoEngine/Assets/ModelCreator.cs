namespace MikoEngine;

using System.IO;
using System.Runtime.InteropServices;

public static class ModelCreator
{
    public static Model Create(string path)
    {
        using FileStream stream = new(path, FileMode.Open, FileAccess.Read);
        Model model = new((int)stream.Length / sizeof(float));
        stream.Read(MemoryMarshal.Cast<float, byte>(model.Data));
        return model;
    }

    public static Model UseShader(this Model model, IShader shader)
    {
        model.Shader = shader;
        return model;
    }

    public static Model ApplyTransform(this Model model, MKMatrix4x4 matrix4X4)
    {
        model.Transform = matrix4X4;
        return model;
    }
}