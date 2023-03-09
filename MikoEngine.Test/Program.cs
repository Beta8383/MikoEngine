#define MacOS

using MikoEngine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const int height = 5000;
const int width = 5000;

Camera camera = new()
{
    Position = new(0f, 0f, 3f),
    Up = MKVector3.UnitY,
    Direction = MKVector3.Zero,
    Width = 1,
    Height = 1,
    zFarPlane = 5f,
    zNearPlane = 1f,
    Mode = ProjectionMode.Perspective
};

#if MacOS
using Image<Rgb24> textureImage = Image.Load<Rgb24>(@"/Users/beta/Desktop/texture.jpg");
#else
using Image<Rgb24> textureImage = Image.Load<Rgb24>(@"D:\texture.jpg");
#endif

Model model = new Cube();
model.Transform = MKMatrix4x4.Identity * 2f;
model.Transform[4, 4] = 1;
model.Shader = new LitShader()
{
    Smoothness = 0.8f,
    Metallic = 1f,
#if MacOS
    Texture = TextureCreator.Create(@"/Users/beta/Desktop/texture.jpg")
#else
    Texture = TextureCreator.Create(@"D:\texture.jpg")
#endif
};

MKVector4 lightPosition = new(0f, 0f, 3f, 1f);
Light light0 = new()
{
    Position = new(lightPosition.X, lightPosition.Y, lightPosition.Z),
    Color = new(1f, 1f, 1f),
    Intensity = 2f,
    Direction = new(0.5f, 0.5f, 1),
    Type = LightType.Point
};
Light light1 = new()
{
    Position = new(lightPosition.X, lightPosition.Y, lightPosition.Z),
    Color = new(1f, 1f, 1f),
    Intensity = 1f,
    Type = LightType.Area
};

MKEngine engine = new(height, width);
engine.SetCamera(camera)
      //.AddLight(light0)
      .AddLight(light1)
      .AddModel(model);

var frame = engine.GetFrame();
byte[] pixels = new byte[width * height * 3];
int index = 0;
for (int i = 0; i < height;i++)
    for (int j = 0; j < width; j++)
    {
        pixels[index] = (byte)(frame[index] * 255);
        index++;
        pixels[index] = (byte)(frame[index] * 255);
        index++;
        pixels[index] = (byte)(frame[index] * 255);
        index++;
    }
using Image<Rgb24> image = Image.LoadPixelData<Rgb24>(pixels, width, height);
#if MacOS
image.SaveAsPng(@"/Users/beta/Desktop/a.png");
#else
image.SaveAsPng(@"D:\a.png");
#endif
MKMatrix4x4 rotate = new(MathF.Cos(0.017453f * 6), 0, -MathF.Sin(0.017453f * 6), 0,
                             0, 1, 0, 0,
                             MathF.Sin(0.017453f * 6), 0, MathF.Cos(0.017453f * 6), 0,
                             0, 0, 0, 1);