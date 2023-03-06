namespace MikoEngine.Assets;

public interface IShader
{
    MKVector4 Vert(Span<float> input, Span<float> ouput);
    MKVector4 Frag(Span<float> input);
    int a2vSize
    {
        get;
    }
    int v2fSize
    {
        get;
    }
}