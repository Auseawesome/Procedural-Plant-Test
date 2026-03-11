using Godot;

namespace ProceduralPlantTest;

public readonly record struct RenderVertex(Vector3 Position, Vector3 Normal)
{
    public RenderVertex(
        float x, float y, float z,
        float nX, float nY, float nZ
    ): this(
        new Vector3(x, y, z),
        new Vector3(nX, nY, nZ)
    ) { }
    
    public RenderVertex(Vector3 Position): this(Position, Position.Normalized()) { }
    
    public RenderVertex(float x, float y, float z): this(new Vector3(x, y, z)) { }

    public RenderVertex Rotated(Vector3 axis, float angle)
    {
        return new RenderVertex(Position.Rotated(axis, angle), Normal.Rotated(axis, angle));
    }
    
    // Modify position with Vector3
    public static RenderVertex operator +(RenderVertex a, Vector3 b) => a with {Position = a.Position + b};

    public static RenderVertex operator -(RenderVertex a, Vector3 b) => a with {Position = a.Position - b};

    public static RenderVertex operator *(RenderVertex a, Vector3 b) => a with {Position = a.Position * b};

    public static RenderVertex operator /(RenderVertex a, Vector3 b) => a with {Position = a.Position / b};
}