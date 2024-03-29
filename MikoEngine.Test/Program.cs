﻿//#define MacOS
//#define save

using System.Diagnostics;
using MikoEngine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

const int height = 800;
const int width = 800;

string currentPath = AppDomain.CurrentDomain.BaseDirectory;

MKEngine engine = new(height, width);

Camera camera = new()
{
    Position = new(2f, 2f, 3f),
    Up = MKVector3.UnitY,
    Direction = MKVector3.Zero,
    Width = 1,
    Height = 1,
    zFarPlane = 5f,
    zNearPlane = 1f,
    Mode = ProjectionMode.Perspective
};

MKMatrix4x4 transform = MKMatrix4x4.Identity * 2f;
transform[4, 4] = 1;

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

Texture texture = engine.AddTexture(currentPath + "texture.jpg");
LitShader shader = new()
{
    Smoothness = 0.5f,
    Metallic = 1f,
    Texture = texture
};
var model = ModelCreator.Create(currentPath + "Cube.mkmodel")
                        .ApplyTransform(transform)
                        .UseShader(shader);

engine.SetCamera(camera)
      .AddLight(light0)
      .AddLight(light1)
      .AddModel(model);

System.Console.WriteLine(Process.GetCurrentProcess().WorkingSet64 / 1048576 + "MB");
engine.GetFrame();
engine.Dispose();
System.Console.WriteLine(Process.GetCurrentProcess().WorkingSet64 / 1048576 + "MB");
#if save
byte[] pixels = new byte[width * height * 3];
int index = 0;
for (int i = 0; i < height;i++)
    for (int j = 0; j < width; j++)
    {
        pixels[index * 3] = (byte)(frame[index].X * 255);
        pixels[index * 3 + 1] = (byte)(frame[index].Y * 255);
        pixels[index * 3 + 2] = (byte)(frame[index].Z * 255);
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
#endif