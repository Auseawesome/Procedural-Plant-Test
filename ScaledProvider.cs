using System.Collections.Generic;
using System.Linq;
using Godot;
using Vector3 = Godot.Vector3;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class ScaledProvider: PointProvider
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
    
    private Vector3 _positionScale = new(1, 1, 1);
    
    [Export]
    public Vector3 PositionScale {
        get => _positionScale;
        set
        {
            _positionScale = value;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }
    
    private float _sizeScale = 1;

    [Export] public float SizeScale {
        get => _sizeScale;
        set
        {
            _sizeScale = value;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }

    public override List<Point> GetPoints()
    {
        return Provider?.GetPoints().Select(pos => pos.Scaled(PositionScale, SizeScale)).ToList();
    }

    public override Aabb GetBounds()
    {
        if (Provider is null) return new Aabb();
        var position = Provider.GetBounds().Position;
        var size = Provider.GetBounds().Size;
        return new Aabb(position * PositionScale, size * PositionScale);
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