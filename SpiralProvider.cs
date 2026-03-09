using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class SpiralProvider: PointProvider
{
    private float _turns = 2;

    [Export(PropertyHint.Range, "0,16,or_greater")]
    public float Turns
    {
        get => _turns;
        set
        {
            _turns = float.Max(value, 0f);
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }
    
    private int _pointCount = 200;

    [Export(PropertyHint.Range, "0,1000,or_greater")]
    public int PointCount
    {
        get => _pointCount;
        set
        {
            _pointCount = int.Max(value, 0);
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }
    
    public override List<Point> GetPoints()
    {
        List<Point> points = [];
        for (var pointId = 0; pointId < PointCount; pointId++)
        {
            var turnAngle = _turns*float.Tau*pointId/PointCount;
            points.Add(new Point(new Vector3(
                (float.Cos(turnAngle)+1.0f)/2.0f,
                ((float) pointId) / PointCount,
                (float.Sin(turnAngle)+1.0f)/2.0f
            )));
        }
        return points;
    }

    public override Aabb GetBounds()
    {
        return new Aabb(Vector3.Zero, Vector3.One);
    }
}