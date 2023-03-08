namespace MikoEngine;

public class Plane : Model
{
    public Plane() =>
        Data = new[]
        {
            -0.5f, 0f, -0.5f, 0f, 0.5f, 0f,
            0.5f, 0f, -0.5f, 0f, 0.5f, 0f,
            0.5f, 0f, 0.5f, 0f, 0.5f, 0f,
            -0.5f, 0f, 0.5f, 0f, 0.5f, 0f
        };
}