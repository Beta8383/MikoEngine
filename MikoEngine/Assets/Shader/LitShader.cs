namespace MikoEngine;

using static MikoEngine.MKMath;

public struct a2v
{
    public MKVector3 Position;
    public MKVector3 Normal;
    public MKVector2 Texcoord;
}

public struct v2f
{
    public MKVector3 Position;
    public MKVector3 Normal;
    public MKVector2 UV;
}

public class LitShader : Shader<a2v, v2f>
{
    public Texture Texture;

    public float smoothness;
    public float Smoothness
    {
        get => smoothness;
        set => smoothness = Math.Clamp(value, 0f, 1f);
    }

    public float metallic;
    public float Metallic
    {
        get => metallic;
        set => metallic = Math.Clamp(value, 0f, 1f);
    }

    public override MKVector4 Vert(ref a2v input, ref v2f output)
    {
        MKVector4 FragPosition = modelTransform * new MKVector4(input.Position, 1f);
        output.Position = new(FragPosition.X, FragPosition.Y, FragPosition.Z);
        output.Normal = input.Normal;
        output.UV = input.Texcoord;
        return WorldToScreenPos(input.Position);
    }

    MKVector4 HalfLambert(Light light, MKVector3 position, MKVector3 normal, MKVector4 color)
    {
        if (light.Intensity == 0)
            return MKVector4.Zero;

        if (light.Type == LightType.Area)
            return MKVector4.Lerp(new(light.Color * light.Intensity ,1f), color) * smoothness;
       
        MKVector3 LightDirection;
        float intensity;
        if (light.Type is LightType.Point)
        {
            LightDirection = light.Position - position;
            float Distance = LightDirection.Distance();
            float DistanceSquare = Max(Distance * Distance, 1f);
            LightDirection = LightDirection.Normalize();
            intensity = light.Intensity / DistanceSquare;
        }
        else
        {
            LightDirection = (light.Position - light.Direction).Normalize();
            intensity = light.Intensity;
        }

        MKVector3 CameraDirection = (camera.Position - position).Normalize();
        MKVector3 HalfwayDirection = (CameraDirection + LightDirection).Normalize();

        //lambert
        //MKVector3 diffuse = MKMath.Max(LightDirection * normal, 0f) * Intensity * light.Color;
        //half-lambert
        MKVector3 diffuse = (Max(LightDirection * normal, 0f) * 0.5f + 0.5f) * intensity * light.Color;
        MKVector3 highlight = MathF.Pow(Max(HalfwayDirection * normal, 0f), 64) * intensity * light.Color;
        return MKVector4.Lerp(new(diffuse, 1f), color) * smoothness + new MKVector4(highlight, 1f);
    }

    public override MKVector4 Frag(ref v2f input)
    {
        MKVector4 texColor = TexelColor(Texture, input.UV);
        return HalfLambert(light0, input.Position, input.Normal, texColor) +
               HalfLambert(light1, input.Position, input.Normal, texColor) +
               HalfLambert(light2, input.Position, input.Normal, texColor) +
               HalfLambert(light3, input.Position, input.Normal, texColor);
    }

}