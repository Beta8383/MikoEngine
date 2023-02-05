namespace MikoEngine.Assets;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using MikoEngine;

class ModelLoader
{
    public void ReadModel(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException();
        if (Path.GetExtension(path).ToLower() != ".mkmodel")
            throw new Exception();

        List<MKVector3> vectors = new();
        List<MKVector3> normals = new();
        List<MKVector2> uvs = new();

        using StreamReader reader = new(path, Encoding.UTF8);
        while (reader.Peek() == -1)
        {
            ReadOnlySpan<char> line = reader.ReadLine().AsSpan();
            if (line.Length < 3)
                continue;

            bool success = SplitBySpace(line, out float[] data);
            if (!success)
                continue;

            if (line[0] is 'v' or 'n')
            {
                if (data.Length is not 3)
                    continue;
                MKVector3 vec = new()
                {
                    X = data[0],
                    Y = data[1],
                    Z = data[2],
                };
                if (line[0] is 'v')
                    vectors.Add(vec);
                else normals.Add(vec);
            }

            if (line[0] is 'u')
            {
                if (data.Length is not 2)
                    continue;
                MKVector2 vec = new()
                {
                    X = data[0],
                    Y = data[1],
                };
                uvs.Add(vec);
            }
        }

    }

    private bool SplitBySpace(ReadOnlySpan<char> str, out float[] result)
    {
        List<float> data = new(5);
        try
        {
            int preSpace = 0;
            for (int i = 0; i < str.Length; i++)
                if (str[i] == ' ')
                {
                    if (preSpace + 1 != i)
                        data.Add(float.Parse(str.Slice(preSpace + 1, i - preSpace - 1)));
                    preSpace = i;
                }
            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            result = data.ToArray();
        }
    }
}