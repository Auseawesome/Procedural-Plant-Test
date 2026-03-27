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

	private int _ringResolution = 10;

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
	
	private Material _surfaceMaterial;

	[Export]
	public Material SurfaceMaterial
	{
		get => _surfaceMaterial;
		set
		{
			_surfaceMaterial = value;
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
		
		var firstCircleNormal = CalcRingNormal(points[0], points[1]);
		
		var nextFirstPoint = GenerateCirclePoint(firstCircleNormal);
		
		// Tessellate Tube
		_tubeMesh.ClearSurfaces();
		
		for (var circlePoint = 0; circlePoint < _ringResolution; circlePoint++)
		{
			var firstPoint = nextFirstPoint;
			var secondPoint = GenerateCirclePoint(firstCircleNormal, (circlePoint+1)%_ringResolution);
			
			nextFirstPoint = secondPoint;
			
			var circleNormal = firstCircleNormal;

			_tubeMesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);
			
			RenderTubeVertex(secondPoint, points[0]);
			RenderTubeVertex(firstPoint, points[0]);
			
			for (var pointId = 1; pointId < points.Count; pointId++)
			{
				var nextCircleNormal = pointId == points.Count - 1
					? CalcRingNormal(points[pointId - 1], points[pointId])
					: CalcRingNormal(points[pointId - 1], points[pointId + 1]);
				
				var spinAxis = circleNormal.Cross(nextCircleNormal).Normalized();
				var spinAngle = circleNormal.AngleTo(nextCircleNormal);
				
				firstPoint = firstPoint.Rotated(spinAxis, spinAngle);
				secondPoint = secondPoint.Rotated(spinAxis, spinAngle);
				
				RenderTubeVertex(secondPoint, points[pointId]);
				RenderTubeVertex(firstPoint, points[pointId]);
				
				circleNormal = nextCircleNormal;
			}

			_tubeMesh.SurfaceEnd();
			_tubeMesh.SurfaceSetMaterial(circlePoint, _surfaceMaterial);
		}
	}

	private static Vector3 CalcRingNormal(Point before, Point after)
	{
		return (after.Position - before.Position).Normalized();
	}
	
	private RenderVertex GenerateCirclePoint(Vector3 startNormal, int index = 0)
	{
		var turnAngle = float.Tau * index / _ringResolution;
		
		var flatPoint = new RenderVertex(
			float.Cos(turnAngle),
			0,
			float.Sin(turnAngle)
		);
		
		if (startNormal == Vector3.Up) return flatPoint;
		
		var spinAxis = startNormal.Cross(Vector3.Up).Normalized();
		var spinAngle = startNormal.AngleTo(Vector3.Up);
		
		return flatPoint.Rotated(spinAxis, spinAngle);
	}

	private void RenderTubeVertex(RenderVertex tubeVertex, Point ringCenter)
	{
		_tubeMesh.SurfaceSetColor(ringCenter.Color);
		_tubeMesh.SurfaceSetNormal(tubeVertex.Normal);
		_tubeMesh.SurfaceAddVertex(tubeVertex.Position * ringCenter.Size + ringCenter.Position);
	}
}
