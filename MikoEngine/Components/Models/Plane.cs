namespace MikoEngine.Components;

public class Plane : ModelBase
{
    public Plane() =>
        VertexData = new[]
        {
            -0.5f, 0f, -0.5f, 0f, 0.5f, 0f,
            0.5f, 0f, -0.5f, 0f, 0.5f, 0f,
            0.5f, 0f, 0.5f, 0f, 0.5f, 0f,
            -0.5f, 0f, 0.5f, 0f, 0.5f, 0f
        };
}