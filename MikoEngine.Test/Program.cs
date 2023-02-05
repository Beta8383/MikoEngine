//#define buildmodel
//#define Test

using System.Diagnostics;
using MikoEngine;
using MikoEngine.Components;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;

#if buildmodel
#region buildmodel
internal class Program
{
    static void WritePoint(MKVector3 point)
    {
        Console.WriteLine($"{point.X.ToString("f8")}f, {point.Y.ToString("f8")}f, {point.Z.ToString("f8")}f, {point.X.ToString("f8")}f, {point.Y.ToString("f8")}f, {point.Z.ToString("f8")}f,");
    }

    private static void Main(string[] args)
    {
        int ver = 9;
        int hor = 48;
        MKVector3[,] point = new MKVector3[ver, hor + 1];

        float R = 0.5f;
        float alpha, beta;
        for (int i = 0; i < ver; i++)
        {
            alpha = -MathF.PI / 180f * 90f / ver * i;
            float y = R * MathF.Sin(alpha);
            float cR = R * MathF.Cos(alpha);
            for (int j = 0; j < hor; j++)
            {
                beta = 2f * MathF.PI / hor * j;
                float x = cR * MathF.Cos(beta);
                float z = cR * MathF.Sin(beta);
                point[i, j] = new(x, y, z);
            }
            point[i, hor] = point[i, hor - 1];
        }

        for (int i = 0; i < ver - 1; i++)
        {
            for (int j = 0; j <= hor - 1; j++)
            {
                WritePoint(point[i, j]);
                WritePoint(point[i + 1, j]);
                WritePoint(point[i + 1, j + 1]);

                WritePoint(point[i, j]);
                WritePoint(point[i + 1, j + 1]);
                WritePoint(point[i, j + 1]);
            }
        }

        for (int j = 0; j <= hor - 1; j++)
        {
            WritePoint(point[ver - 1, j]);
            WritePoint(point[ver - 1, j + 1]);
            WritePoint(new(0f, -0.5f, 0f));
        }

        Console.ReadKey();
    }
}
#endregion
#elif !Test
const int height = 900;
const int width = 900;

MKEngine engine = new(height, width);
Camera camera = new()
{
    Position = new(0f, 0f, 4f),
    Up = MKVector3.UnitY,
    Direction = MKVector3.Zero,
    Width = 5,
    Height = 5,
    zFarPlane = 10f,
    zNearPlane = 1f,
    Mode = ProjectionMode.Perspective
};
engine.SetCamera(camera);
Sphere model = new();
model.ModelTransform[1, 1] = model.ModelTransform[2, 2] = model.ModelTransform[3, 3] = 7f;
engine.AddModel(model);

MKVector4 lightPosition = new(3f, 1.5f, 6f, 1f);
Light light = new()
{
    Position = new(lightPosition.X, lightPosition.Y, lightPosition.Z),
    Color = new(255f, 255f, 255f),
    Intensity = 5f
};
engine.SetLight(light);

const int frameDelay = 10;

Stopwatch watch = new();
watch.Start();
var start = engine.GetFrame();
// Create empty image.
using Image<Rgb24> gif = Image.LoadPixelData<Rgb24>(start, width, height);

// Set the delay until the next image is displayed.
GifFrameMetadata metadata = gif.Frames.RootFrame.Metadata.GetGifMetadata();
metadata.FrameDelay = frameDelay;

MKMatrix4x4 rotate = new(MathF.Cos(0.017453f * 6), 0, -MathF.Sin(0.017453f * 6), 0,
                             0, 1, 0, 0,
                             MathF.Sin(0.017453f * 6), 0, MathF.Cos(0.017453f * 6), 0,
                             0, 0, 0, 1);

for (int i = 0; i < 30; i++)
{
    //lightPosition = rotate * lightPosition;
    //light.Position.X = lightPosition.X;
    //light.Position.Y = lightPosition.Y;
    //light.Position.Z = lightPosition.Z;
    light.Position.X -= 0.2f;
    engine.SetLight(light);
    var a = engine.GetFrame();

    // Create a color image, which will be added to the gif.
    using Image<Rgb24> image = Image.LoadPixelData<Rgb24>(a, width, height);

    // Set the delay until the next image is displayed.
    metadata = image.Frames.RootFrame.Metadata.GetGifMetadata();
    metadata.FrameDelay = frameDelay;

    // Add the color image to the gif.
    gif.Frames.AddFrame(image.Frames.RootFrame);
}

watch.Stop();
System.Console.WriteLine(watch.ElapsedMilliseconds);
gif.SaveAsGif(@"D:\a.gif");


#else
//2448
for (int i = 0; i < 4896; i++)
    System.Console.Write(i + ", ");
Console.ReadLine();
#endif