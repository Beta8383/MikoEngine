namespace MikoEngine.Assets;

using MikoEngine.Components;

struct Fragmentdata
{
    public MKVector3 FragPosition;
    public MKVector3 Normal;
    public float YLerp;
}

class DefaultFragmentShader : IFragmentShader<Fragmentdata>
{
    public static void main(in Fragmentdata data, Light light, Camera camera, out MKVector4 color)
    {
        float Distance = (data.FragPosition - light.Position).Distance();
        float DistanceSquare = MKMath.Max(Distance * Distance, 1f);
        float Intensity = light.Intensity / DistanceSquare;

        MKVector3 LightDirection = (light.Position - data.FragPosition).Normalize();
        //lambert
        //MKVector3 diffuse = MKMath.Max(LightDirection * Normal, 0f) * Intensity * light.Color;
        //half-lambert
        MKVector3 diffuse = (MKMath.Max(LightDirection * data.Normal, 0f) * 0.5f + 0.5f) * Intensity * light.Color;

        MKVector3 CameraDirection = (camera.Position - data.FragPosition).Normalize();
        MKVector3 HalfwayDirection = (CameraDirection + LightDirection).Normalize();
        MKVector3 highlight = MathF.Pow(MKMath.Max(HalfwayDirection * data.Normal, 0f), 64) * Intensity * light.Color;

        MKVector3 environment = new(50f, 0f, 0f);

        float rReflect = data.YLerp * 1f;
        float GReflect = (1f - data.YLerp) * 1f;

        color = new MKVector4(new MKVector3(rReflect, GReflect, 0f) % (diffuse + environment) + highlight * 0.8f, 1f);
        //color = new MKVector4(highlight, 1f);
        //color = new MKVector4(255f) * (1f / DistanceSquare);
    }
}