using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class GradientProvider : PointProvider
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

    private Color _startColor = Colors.Black;

    [Export]
    public Color StartColor
    {
        get => _startColor;
        set
        {
            _startColor  = value;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }

    private Color _endColor = Colors.White;
    
    [Export]
    public Color EndColor
    {
        get => _endColor;
        set
        {
            _endColor = value;
            EmitSignal(PointProvider.SignalName.PointsUpdated);
        }
    }
    
    public override List<Point> GetPoints()
    {
        var pointList = _provider.GetPoints();

        var listPortion = 1.0f / pointList.Count;
        
        return pointList.Select((point, i) => point with
        {
            Color = StartColor.Lerp(EndColor, i * listPortion)
        }).ToList();
    }

    public override Aabb GetBounds()
    {
        return _provider.GetBounds();
    }
    
    private void ChainUpdated()
    {
        EmitSignal(PointProvider.SignalName.PointsUpdated);
    }
}