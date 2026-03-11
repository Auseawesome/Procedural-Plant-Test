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

	private void UpdateMesh()
	{
		if (Provider is null) return;
		if (!IsInsideTree()) return;

		var points = Provider.GetPoints();
		
		// Initialize variables on first update
		_tubeMesh ??= new ImmediateMesh();
		if (!_renderingInstance.IsValid)
		{
			_renderingInstance = RenderingServer.InstanceCreate();
			RenderingServer.InstanceSetBase(_renderingInstance, _tubeMesh.GetRid());
			RenderingServer.InstanceSetScenario(_renderingInstance, GetWorld3D().Scenario);
			RenderingServer.InstanceSetTransform(_renderingInstance, Transform3D.Identity);
		}
		
		// Create first ring from flat circle
		var previousCircleNormal = CalcRingNormal(points[0], points[1]);
		var previousCircle = GenerateOriginalRing(previousCircleNormal);
		
		// Tessellate Tube
		_tubeMesh.ClearSurfaces();
		for (var pointId = 0; pointId < points.Count - 1; pointId++)
		{
			var nextCircleNormal = pointId == points.Count - 2
				? previousCircleNormal
				: CalcRingNormal(points[pointId], points[pointId + 2]);
				
			var nextCircle = pointId == points.Count - 2
				? previousCircle
				: GenerateRingPositions(previousCircle, previousCircleNormal, nextCircleNormal);
			
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

	private static Vector3 CalcRingNormal(Point before, Point after)
	{
		return (after.Position - before.Position).Normalized();
	}

	private static List<RenderVertex> GenerateRingPositions(List<RenderVertex> previousCircle, Vector3 previousNormal, Vector3 normal)
	{
		var rotAxis = previousNormal.Cross(normal).Normalized();
		var angle = previousNormal.AngleTo(normal);
		
		return previousCircle.Select(vertex => vertex.Rotated(rotAxis, angle)).ToList();
	}
	
	private List<RenderVertex> GenerateOriginalRing(Vector3 startNormal)
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

		return GenerateRingPositions(points, Vector3.Up, startNormal);
	}

	private void RenderTubeVertex(RenderVertex tubeVertex, Point ringCenter)
	{
		_tubeMesh.SurfaceSetColor(ringCenter.Color);
		_tubeMesh.SurfaceSetNormal(tubeVertex.Normal);
		_tubeMesh.SurfaceAddVertex(tubeVertex.Position * ringCenter.Size + ringCenter.Position);
	}
}
