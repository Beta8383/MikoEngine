using System.Diagnostics;
using MikoEngine;
using MikoEngine.Components;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const int height = 900;
const int width = 900;

Camera camera = new()
{
    Position = new(2f, 2f, 2f),
    Up = MKVector3.UnitY,
    Direction = MKVector3.Zero,
    Width = 1,
    Height = 1,
    zFarPlane = 5f,
    zNearPlane = 1f,
    Mode = ProjectionMode.Perspective
};

Model model = new Sphere();
model.Transform = MKMatrix4x4.Identity * 1.5f;
model.Transform[4, 4] = 1;

MKVector4 lightPosition = new(3f, 3f, 3f, 3f);
Light light = new()
{
    Position = new(lightPosition.X, lightPosition.Y, lightPosition.Z),
    Color = new(255f, 255f, 255f),
    Intensity = 10f
};


MKEngine engine = new(height, width);
engine.SetCamera(camera)
      .SetLight(light)
      .AddModel(model);

var frame = engine.GetFrame();

using Image<Rgb24> image = Image.LoadPixelData<Rgb24>(frame, width, height);
image.SaveAsPng(@"D:\a.png");

MKMatrix4x4 rotate = new(MathF.Cos(0.017453f * 6), 0, -MathF.Sin(0.017453f * 6), 0,
                             0, 1, 0, 0,
                             MathF.Sin(0.017453f * 6), 0, MathF.Cos(0.017453f * 6), 0,
                             0, 0, 0, 1);