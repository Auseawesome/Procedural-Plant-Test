using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest.Provider.Collector;

public partial class SubsectionProvider : PointProvider
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

    private float _startPortion;

    [Export]
    public float StartPortion
    {
        get => _startPortion;
        set
        {
            _startPortion = value;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }
    
    public override List<Point> GetPoints()
    {
        throw new System.NotImplementedException();
    }

    public override Aabb GetBounds()
    {
        throw new System.NotImplementedException();
    }
    
    private void ChainUpdated()
    {
        EmitSignal(PointProvider.SignalName.PointsUpdated);
    }
}