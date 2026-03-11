using Godot;

namespace ProceduralPlantTest;

public readonly record struct Point(Vector3 Position, float Size, Color Color)
{
    public Point(Vector3 Position) : this(Position, 1, Colors.Black) { }
    
    // Modify position with Vector3
    public static Point operator +(Point a, Vector3 b) => a with {Position = a.Position + b};

    public static Point operator -(Point a, Vector3 b) => a with {Position = a.Position - b};

    public static Point operator *(Point a, Vector3 b) => a with {Position = a.Position * b};

    public static Point operator /(Point a, Vector3 b) => a with {Position = a.Position / b};
    
    // Transform position with Quaternion and Basis
    public static Point operator *(Point a, Quaternion b) => a with {Position = a.Position * b};

    public static Point operator *(Point a, Basis b) => a with {Position = a.Position * b};

    // Modify size with float and int
    public static Point operator +(Point a, float b) => a with {Size = a.Size + b};

    public static Point operator -(Point a, float b) => a with {Size = a.Size - b};

    public static Point operator *(Point a, float b) => a with {Size = a.Size * b};

    public static Point operator /(Point a, float b) => a with {Size = a.Size / b};

    public static Point operator +(Point a, int b) => a with {Size = a.Size + b};

    public static Point operator -(Point a, int b) => a with {Size = a.Size - b};

    public static Point operator *(Point a, int b) => a with {Size = a.Size * b};

    public static Point operator /(Point a, int b) => a with {Size = a.Size / b};

    public Vector3 GetSizeVector() => new(Size, Size, Size);

    public Point Scaled(Vector3 posScale, float sizeScale)
    {
        return this with
        {
            Position = Position * posScale,
            Size = Size * sizeScale
        };
    }

    public Vector3 SolidColor()
    {
        return new Vector3(Color.R, Color.G, Color.B);
    }
}