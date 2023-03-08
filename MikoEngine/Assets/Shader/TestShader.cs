namespace MikoEngine.Assets;

using MikoEngine.Components;

struct a2v
{
    public MKVector3 Position;
    public MKVector3 Normal;
}

struct v2f
{
    public MKVector3 FragPosition;
    public MKVector3 Normal;
}

class TestShader : Shader<a2v,v2f>
{
    public override MKVector4 Vert(ref a2v input, ref v2f output)
    {
        MKVector4 FragPosition = uni.modelTransform * (new MKVector4(input.Position, 1f));
        output.FragPosition = new(FragPosition.X, FragPosition.Y, FragPosition.Z);
        output.Normal = input.Normal;
        return WorldToScreenPos(input.Position);
    }

    public override MKVector4 Frag(ref v2f input)
    {
        float Distance = (input.FragPosition - uni.light.Position).Distance();
        float DistanceSquare = MKMath.Max(Distance * Distance, 1f);
        float Intensity = uni.light.Intensity / DistanceSquare;
        MKVector3 LightDirection = (uni.light.Position - input.FragPosition).Normalize();
        MKVector3 CameraDirection = (uni.camera.Position - input.FragPosition).Normalize();

        //lambert
        //MKVector3 diffuse = MKMath.Max(LightDirection * Normal, 0f) * Intensity * light.Color;
        //half-lambert
        MKVector3 diffuse = (MKMath.Max(LightDirection * input.Normal, 0f) * 0.5f + 0.5f) * Intensity * uni.light.Color;

        MKVector3 HalfwayDirection = (CameraDirection + LightDirection).Normalize();
        MKVector3 highlight = MathF.Pow(MKMath.Max(HalfwayDirection * input.Normal, 0f), 64) * Intensity * uni.light.Color;

        MKVector3 environment = new(50f, 0f, 0f);

        return new MKVector4(MKVector3.Lerp(diffuse + environment, new MKVector3(1f, 0f, 0f)) + highlight * 0.8f, 1f);
        //color = new float4(highlight, 1f);
        //color = new float4(255f) * (1f / DistanceSquare);
    }
}