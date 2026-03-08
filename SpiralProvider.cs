using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class SpiralProvider: PointProvider
{
    /// Initial radius of the circle as determined by https://spencermortensen.com/articles/bezier-circle/
    private const float InitRadius = 1.00005507808f * 0.5f;
    /// Distance of control point from node tangential to the curve from https://spencermortensen.com/articles/bezier-circle/
    private const float TanHandleLen = 0.55342925736f * 0.5f;
    /// Distance of control point from node normal to the curve from https://spencermortensen.com/articles/bezier-circle/
    private const float HandleRadOffset = 0.998737689f * 0.5f - InitRadius;
    
    private float _quarterTurns = 4;

    [Export]
    public float QuarterTurns
    {
        get => _quarterTurns;
        set
        {
            _quarterTurns = value;
            UpdateCurve();
        }
    }

    private Curve3D _spiralCurve = new();

    public SpiralProvider()
    {
        _spiralCurve.BakeInterval = 0.05f;
        UpdateCurve();
    }
    
    public override List<Vector3> GetPoints()
    {
        return _spiralCurve.GetBakedPoints().ToList();
    }

    public void UpdateCurve()
    {
        _spiralCurve.ClearPoints();

        var quadLen = 1.0f / _quarterTurns;
        var yHandle = quadLen / float.Pi;

        Vector3 posXPoint = new(InitRadius, 0, 0);
        Vector3 posXIn = new(HandleRadOffset, -yHandle, -TanHandleLen);
        Vector3 posXOut = new(HandleRadOffset, yHandle, TanHandleLen);
        
        Vector3 posZPoint = new(0, 0, InitRadius);
        Vector3 posZIn = new(TanHandleLen, -yHandle, HandleRadOffset);
        Vector3 posZOut = new(-TanHandleLen, yHandle, HandleRadOffset);
        
        Vector3 negXPoint = new(-InitRadius, 0, 0);
        Vector3 negXIn = new(-HandleRadOffset, -yHandle, TanHandleLen);
        Vector3 negXOut = new(-HandleRadOffset, yHandle, -TanHandleLen);
        
        Vector3 negZPoint = new(0, 0, -InitRadius);
        Vector3 negZIn = new(-TanHandleLen, -yHandle, -HandleRadOffset);
        Vector3 negZOut = new(TanHandleLen, yHandle, -HandleRadOffset);

        for (var i = 0; i < _quarterTurns + 1; i++)
        {
            Vector3 lenOffset = new(0, quadLen * i, 0);
            switch (i % 4)
            {
                case 0:
                    _spiralCurve.AddPoint(posXPoint + lenOffset, posXIn, posXOut);
                    break;
                case 1:
                    _spiralCurve.AddPoint(posZPoint + lenOffset, posZIn, posZOut);
                    break;
                case 2:
                    _spiralCurve.AddPoint(negXPoint + lenOffset, negXIn, negXOut);
                    break;
                case 3:
                    _spiralCurve.AddPoint(negZPoint + lenOffset, negZIn, negZOut);
                    break;
            }
        }
        
        EmitSignal(PointProvider.SignalName.PointsUpdated);
    }
}