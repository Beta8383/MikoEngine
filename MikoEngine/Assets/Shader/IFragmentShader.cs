namespace MikoEngine.Assets;

using MikoEngine.Components;

public interface IFragmentShader<FragmentData> where FragmentData : struct
{
    static abstract void main(in FragmentData InPara, Light light, Camera camera, out MKVector4 color);
}