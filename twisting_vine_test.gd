extends Path3D

const unscaled_a := 1.00005507808
const unscaled_b := 0.55342925736
const unscaled_c := 0.998737689 - unscaled_a

@export var radius: float:
	set(value):
		radius = value;
		if (is_node_ready()):
			update_curve();
			update_mesh();
@export var length: float:
	set(value):
		length = value;
		if (is_node_ready()):
			update_curve();
			update_mesh();
@export var mesh: Mesh
@export var quarter_turns: int:
	set(value):
		quarter_turns = value;
		if (is_node_ready()):
			update_curve();
			update_mesh();

var instance_cache: Array[RID];

func _ready() -> void:
	curve.bake_interval = 0.05
	
	update_curve()
	update_mesh()

func update_curve() -> void:
	curve.clear_points();

	var a := unscaled_a * radius;
	var b := unscaled_b * radius;
	var c := unscaled_c * radius;

	var lenGrad := length/quarter_turns/PI

	var pointA := position + Vector3(a, 0, 0);
	var pointAIn := Vector3(c, -lenGrad, -b)
	var pointAOut := Vector3(c, lenGrad, b)
	var pointB := position + Vector3(0, 0, a);
	var pointBIn := Vector3(b, -lenGrad, c)
	var pointBOut := Vector3(-b, lenGrad, c)
	var pointC := position + Vector3(-a, 0, 0);
	var pointCIn := Vector3(-c, -lenGrad, b)
	var pointCOut := Vector3(-c, lenGrad, -b)
	var pointD := position + Vector3(0, 0, -a);
	var pointDIn := Vector3(-b, -lenGrad, -c)
	var pointDOut := Vector3(b, lenGrad, -c)
	
	for i in quarter_turns+1:
		if i % 4 == 0:
			curve.add_point(pointA + Vector3(0, length/quarter_turns*(i-quarter_turns/2.0), 0), pointAIn, pointAOut);
		elif i % 4 == 1:
			curve.add_point(pointB + Vector3(0, length/quarter_turns*(i-quarter_turns/2.0), 0), pointBIn, pointBOut);
		elif i % 4 == 2:
			curve.add_point(pointC + Vector3(0, length/quarter_turns*(i-quarter_turns/2.0), 0), pointCIn, pointCOut);
		else:
			curve.add_point(pointD + Vector3(0, length/quarter_turns*(i-quarter_turns/2.0), 0), pointDIn, pointDOut);

func update_mesh() -> void:
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
			RenderingServer.instance_set_visible(instance_cache[point_id], true);
	
	if (len(instance_cache) > len(baked_points)):
		for i in range(len(instance_cache)-1, len(baked_points)-1, -1):
			RenderingServer.instance_set_visible(instance_cache[i], false);
