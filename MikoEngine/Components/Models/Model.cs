﻿namespace MikoEngine;

public class Model
{
    internal IShader Shader = new LitShader();
    internal AllocSpan<float> Data;
    internal MKMatrix4x4 Transform;

    internal Model(int dataSize)
    {
        Data = new(dataSize);
    }
}