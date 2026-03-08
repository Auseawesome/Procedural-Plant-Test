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
    
    private Vector3 _scale = new(1, 1, 1);
    
    [Export]
    public Vector3 Scale{
        get => _scale;
        set
        {
            _scale = value;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }

    public override List<Vector3> GetPoints()
    {
        return Provider?.GetPoints().Select(pos => pos*Scale).ToList();
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