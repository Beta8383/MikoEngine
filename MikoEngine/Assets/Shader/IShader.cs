namespace MikoEngine.Assets;

public interface IShader
{
    MKVector4 Vert(Span<float> input, Span<float> ouput);
    MKVector4 Frag(Span<float> input);
    int a2vLength{ get; }
    int v2fLength{ get; }
    
    Uniform uni
    {
        get;
        set;
    }
}