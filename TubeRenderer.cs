using System.Collections.Generic;
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
		var bounds = Provider.GetBounds();
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

		// Tessellate Tube
		_tubeMesh.ClearSurfaces();
		for (var pointId = 0; pointId < points.Count - 1; pointId++)
		{
			var point = points[pointId];
			var color = new Color(point.Color.X, point.Color.Y, point.Color.Z);

			var ringNormal = pointId == 0
				? _calcRingNormal(point, points[pointId + 1])
				: _calcRingNormal(points[pointId - 1], points[pointId + 1]);
			var secondRingNormal = pointId == points.Count - 2
				? ringNormal
				: _calcRingNormal(point, points[pointId + 2]);

			var firstRingPositions = _generateRingPositions(points[pointId], ringNormal);
			var secondRingPositions = _generateRingPositions(points[pointId + 1], secondRingNormal);

			_tubeMesh.SurfaceBegin(Mesh.PrimitiveType.TriangleStrip);
			for (var ringSection = 0; ringSection <= _ringResolution + 1; ringSection++)
			{
				_tubeMesh.SurfaceSetColor(color);
				_tubeMesh.SurfaceSetNormal(secondRingPositions[ringSection % _ringResolution].Normal);
				_tubeMesh.SurfaceAddVertex(secondRingPositions[ringSection % _ringResolution].Position);
				_tubeMesh.SurfaceSetColor(color);
				_tubeMesh.SurfaceSetNormal(firstRingPositions[ringSection % _ringResolution].Normal);
				_tubeMesh.SurfaceAddVertex(firstRingPositions[ringSection % _ringResolution].Position);
			}

			_tubeMesh.SurfaceEnd();
		}
	}

	private Vector3 _calcRingNormal(Point before, Point after)
	{
		return (after.Position - before.Position).Normalized();
	}

	private List<RenderVertex> _generateRingPositions(Point centre, Vector3 normal)
	{
		var rotAxis = Vector3.Up.Cross(normal).Normalized();
		var angle = Vector3.Up.AngleTo(normal);
		
		List<RenderVertex> points = [];
		for (var pointId = 0; pointId < _ringResolution; pointId++)
		{
			var turnAngle = float.Tau * pointId / _ringResolution;
			var point = new RenderVertex(
				float.Cos(turnAngle) * centre.Size,
				0,
				float.Sin(turnAngle) * centre.Size
			);
			point = point.Rotated(rotAxis, angle);
			point += centre.Position;
			points.Add(point);
		}

		return points;

	}
}
