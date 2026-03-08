extends Path3D

const unscaled_a := 1.00005507808
const unscaled_b := 0.55342925736
const unscaled_c := 0.998737689 - unscaled_a

@export var radius: float

@export var length: float

@export var rotation_angles: Vector3

@export var mesh: Mesh

var instance_cache: Array[RID]

func _ready() -> void:
	var a := unscaled_a * radius;
	var b := unscaled_b * radius;
	var c := unscaled_c * radius;
	
	var zGradient := sqrt(c*c+b*b) * length/8

	var pointA := position + Vector3(a, 0, -length/2);
	var pointAIn := Vector3(c, -b, -zGradient)
	var pointAOut := Vector3(c, b, zGradient)
	var pointB := position + Vector3(0, a, -length/4);
	var pointBIn := Vector3(b, c, -zGradient)
	var pointBOut := Vector3(-b, c, zGradient)
	var pointC := position + Vector3(-a, 0, 0);
	var pointCIn := Vector3(-c, b, -zGradient)
	var pointCOut := Vector3(-c, -b, zGradient)
	var pointD := position + Vector3(0, -a, length/4);
	var pointDIn := Vector3(-b, -c, -zGradient)
	var pointDOut := Vector3(b, -c, zGradient)

	curve.bake_interval = 0.05
	
	curve.add_point(pointA, Vector3.ZERO, pointAOut);
	curve.add_point(pointB, pointBIn, pointBOut);
	curve.add_point(pointC, pointCIn, pointCOut);
	curve.add_point(pointD, pointDIn, pointDOut);
	curve.add_point(pointA + Vector3(0, 0, length), pointAIn, Vector3.ZERO);

func _process(delta: float) -> void:
	var rotation_quat = Quaternion.from_euler(delta*rotation_angles*PI/180)

	for point_id in curve.point_count:
		curve.set_point_position(point_id, rotation_quat * curve.get_point_position(point_id));
		curve.set_point_in(point_id, rotation_quat * curve.get_point_in(point_id));
		curve.set_point_out(point_id, rotation_quat * curve.get_point_out(point_id));

	var baked_points = curve.get_baked_points();

	for point_id in len(baked_points):
		var xform = Transform3D(Basis(), baked_points[point_id])

		if len(instance_cache) < point_id + 1:
			var render_instance = RenderingServer.instance_create();
			RenderingServer.instance_set_scenario(render_instance, get_world_3d().scenario);

			RenderingServer.instance_set_base(render_instance, mesh);
			RenderingServer.instance_set_transform(render_instance, xform);

			instance_cache.append(render_instance);
		else:
			RenderingServer.instance_set_transform(instance_cache[point_id], xform);
