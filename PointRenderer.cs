using System.Collections.Generic;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class PointRenderer: Node3D
{
    private PointProvider _provider;

    [Export]
    public PointProvider Provider
    {
        get => _provider;
        set
        {
            _provider = value;
            _provider.PointsUpdated += UpdateMesh;
            UpdateMesh();
        }
    }
    
    private Mesh _pointMesh;

    [Export]
    public Mesh PointMesh
    {
        get => _pointMesh;
        set
        {
            _pointMesh = value;
            UpdateMesh();
        }
    }

    private bool _centered = true;

    [Export]
    public bool Centered
    {
        get => _centered;
        set
        {
            _centered = value;
            UpdateMesh();
        }
    }

    private readonly List<Rid> _instanceCache = [];

    public override void _Ready()
    {
        UpdateMesh();
    }

    public override void _ExitTree()
    {
        Provider.PointsUpdated -= UpdateMesh;
        foreach (var renderInstance in _instanceCache)
        {
            RenderingServer.FreeRid(renderInstance);
        }
    }

    public void UpdateMesh()
    {
        if (Provider is null) return;
        if (!IsInsideTree()) return;
        
        var points = Provider.GetPoints();
        var bounds = Provider.GetBounds();
        
        for (var pointId = 0; pointId < points.Count; pointId++)
        {
            var point = points[pointId];
            
            var position = point.Position;
            if (_centered)
                position = position - bounds.Position - bounds.Size/2;
            
            Transform3D transform = new(Basis.Identity.Scaled(point.GetSizeVector()), position);

            if (_instanceCache.Count < pointId + 1)
            {
                var renderInstance = RenderingServer.InstanceCreate();
                RenderingServer.InstanceSetScenario(renderInstance, GetWorld3D().Scenario);
                RenderingServer.InstanceSetBase(renderInstance, PointMesh.GetRid());
                RenderingServer.InstanceSetTransform(renderInstance, transform);
                RenderingServer.InstanceGeometrySetShaderParameter(renderInstance, "color", point.Color);
                
                _instanceCache.Add(renderInstance);
            }
            else
            {
                RenderingServer.InstanceSetTransform(_instanceCache[pointId], transform);
            }
        }
        
        // Remove excess points
        if (_instanceCache.Count <= points.Count) return;

        for (var pointId = _instanceCache.Count - 1; pointId >= points.Count; pointId--)
        {
            RenderingServer.FreeRid(_instanceCache[pointId]);
            _instanceCache.RemoveAt(pointId);
        }
    }
}