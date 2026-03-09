using System.Collections.Generic;
using System.Linq;
using Godot;
using Vector3 = Godot.Vector3;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class ArcProvider: PointProvider
{
    private PointProvider _provider;
    
    [Export]
    public PointProvider Provider
    {
        get => _provider;
        set
        {
            if (_provider is not null) _provider.PointsUpdated -= ChainUpdated;
            _provider = value;
            _provider.PointsUpdated += ChainUpdated;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }
    
    private float _arcAngle = 90f;

    [Export(PropertyHint.Range,"0,360,or_greater")] public float ArcAngle {
        get => _arcAngle;
        set
        {
            _arcAngle = float.Max(value, 0f);
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }

    private Point _modifyPoint(Point point)
    {
        var arcRadius = Provider.GetBounds().Size.Y;
        
        var arcRadians = point.Position.Y/arcRadius * float.DegreesToRadians(_arcAngle);
        return point with
        {
            Position =
                new Vector3(float.Cos(arcRadians), float.Sin(arcRadians), 0)*arcRadius +
                new Vector3(point.Position.X, 0, point.Position.Z).Rotated(Vector3.Back, arcRadians)
        };
    }

    public override List<Point> GetPoints()
    {
        return Provider?.GetPoints().Select(_modifyPoint).ToList();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete) Provider.PointsUpdated -= ChainUpdated;
    }

    private void ChainUpdated()
    {
        EmitSignal(PointProvider.SignalName.PointsUpdated);
    }
}