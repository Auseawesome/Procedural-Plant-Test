using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ProceduralPlantTest;

[GlobalClass]
public partial class TubeRenderer : Node3D
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

	private int _ringResolution = 5;

	[Export]
	public int RingResolution
	{
		get => _ringResolution;
		set
		{
			_ringResolution = value;
			UpdateMesh();
		}
	}

	private Rid _renderingInstance;
	private ImmediateMesh _tubeMesh;
	
	public override void _Ready()
	{
		UpdateMesh();
	}

	public override void _ExitTree()
	{
		Provider.PointsUpdated -= UpdateMesh;
		
		RenderingServer.FreeRid(_renderingInstance);
	}

	public void UpdateMesh()
	{
		if (Provider is null) return;
		if (!IsInsideTree()) return;

		var points = Provider.GetPoints();
		if (_tubeMesh is null)
		{
			_tubeMesh = new ImmediateMesh();
		}
		if (!_renderingInstance.IsValid)
		{
			_renderingInstance = RenderingServer.InstanceCreate();
			RenderingServer.InstanceSetBase(_renderingInstance, _tubeMesh.GetRid());
			RenderingServer.InstanceSetScenario(_renderingInstance, GetWorld3D().Scenario);
		}

		Transform3D transform = new(Basis.Identity, Vector3.Zero);
		RenderingServer.InstanceSetTransform(_renderingInstance, transform);
		
		var previousCircleNormal = _calcRingNormal(points[0], points[1]);
		var previousCircle = _generateOriginalRing(previousCircleNormal);
		
		// Tessellate Tube
		_tubeMesh.ClearSurfaces();
		for (var pointId = 0; pointId < points.Count - 1; pointId++)
		{
			var nextCircleNormal = pointId == points.Count - 2
				? previousCircleNormal
				: _calcRingNormal(points[pointId], points[pointId + 2]);
			
			var nextCircle = _generateRingPositions(previousCircle, previousCircleNormal.Normalized(), nextCircleNormal);
			
			_tubeMesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);
			for (var ringSection = 0; ringSection <= _ringResolution + 1; ringSection++)
			{
				RenderTubeVertex(nextCircle[ringSection % _ringResolution], points[pointId + 1]);
				RenderTubeVertex(previousCircle[ringSection % _ringResolution], points[pointId]);
			}

			_tubeMesh.SurfaceEnd();
			previousCircle = nextCircle;
			previousCircleNormal = nextCircleNormal;
		}
	}

	private Vector3 _calcRingNormal(Point before, Point after)
	{
		return (after.Position - before.Position).Normalized();
	}

	private List<RenderVertex> _generateRingPositions(List<RenderVertex> previousCircle, Vector3 previousNormal, Vector3 normal)
	{
		var rotAxis = previousNormal.Cross(normal).Normalized();
		var angle = previousNormal.AngleTo(normal);
		
		return previousCircle.Select(vertex => vertex.Rotated(rotAxis, angle)).ToList();
	}
	
	private List<RenderVertex> _generateOriginalRing(Vector3 startNormal)
	{
		// Generate flat ring
		List<RenderVertex> points = [];
		for (var pointId = 0; pointId < _ringResolution; pointId++)
		{
			var turnAngle = float.Tau * pointId / _ringResolution;
			var point = new RenderVertex(
				float.Cos(turnAngle),
				0,
				float.Sin(turnAngle)
			);
			points.Add(point);
		}

		return _generateRingPositions(points, Vector3.Up, startNormal);
	}

	private void RenderTubeVertex(RenderVertex tubeVertex, Point ringCenter)
	{
		_tubeMesh.SurfaceSetColor(ringCenter.Color);
		_tubeMesh.SurfaceSetNormal(tubeVertex.Normal);
		_tubeMesh.SurfaceAddVertex(tubeVertex.Position * ringCenter.Size + ringCenter.Position);
	}
}
