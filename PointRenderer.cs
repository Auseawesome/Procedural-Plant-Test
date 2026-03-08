using System.Collections.Generic;
using System.Drawing;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class PointRenderer: Node3D
{
    [Export] public PointProvider Provider;
    
    private Mesh _pointMesh;

    [Export]
    public Mesh PointMesh
    {
        get => _pointMesh;
        set
        {
            _pointMesh = value;
            RenderMesh();
        }
    }

    private readonly List<Rid> _instanceCache = [];

    public override void _Ready()
    {
        RenderMesh();
    }

    public override void _ExitTree()
    {
        Provider.PointsUpdated -= RenderMesh;
        foreach (var renderInstance in _instanceCache)
        {
            RenderingServer.FreeRid(renderInstance);
        }
    }

    public void RenderMesh()
    {
        if (Provider is null) return;
        if (!IsInsideTree()) return;
        
        var points = Provider.GetPoints();
        
        for (var pointId = 0; pointId < points.Count; pointId++)
        {
            var pointPos = points[pointId];
            
            Transform3D transform = new(new Basis(), pointPos);

            if (_instanceCache.Count < pointId + 1)
            {
                var renderInstance = RenderingServer.InstanceCreate();
                RenderingServer.InstanceSetScenario(renderInstance, GetWorld3D().Scenario);
                RenderingServer.InstanceSetBase(renderInstance, PointMesh.GetRid());
                RenderingServer.InstanceSetTransform(renderInstance, transform);
                
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