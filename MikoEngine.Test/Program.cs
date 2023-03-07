using System.Diagnostics;
using MikoEngine;
using MikoEngine.Components;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const int height = 900;
const int width = 900;

Camera camera = new()
{
    Position = new(0f, 0f, 2f),
    Up = MKVector3.UnitY,
    Direction = MKVector3.Zero,
    Width = 1,
    Height = 1,
    zFarPlane = 5f,
    zNearPlane = 1f,
    Mode = ProjectionMode.Perspective
};

Model model = new Cube();

MKVector4 lightPosition = new(0f, 0.3f, 2f, 1f);
Light light = new()
{
    Position = new(lightPosition.X, lightPosition.Y, lightPosition.Z),
    Color = new(255f, 255f, 255f),
    Intensity = 1f
};


MKEngine engine = new(height, width);
engine.SetCamera(camera)
      .SetLight(light)
      .AddModel(model);

Stopwatch watch = new();
watch.Start();
var frame = engine.GetFrame();
watch.Stop();
Console.WriteLine("Test Used Time:" + watch.ElapsedMilliseconds);

using Image<Rgb24> image = Image.LoadPixelData<Rgb24>(frame, width, height);
image.SaveAsPng(@"D:\a.png");

MKMatrix4x4 rotate = new(MathF.Cos(0.017453f * 6), 0, -MathF.Sin(0.017453f * 6), 0,
                             0, 1, 0, 0,
                             MathF.Sin(0.017453f * 6), 0, MathF.Cos(0.017453f * 6), 0,
                             0, 0, 0, 1);