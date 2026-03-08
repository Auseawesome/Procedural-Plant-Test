extends Path3D

const unscaled_a := 1.00005507808
const unscaled_b := 0.55342925736
const unscaled_c := 0.998737689 - unscaled_a

@export var radius: float

@export var length: float

@export var centre: Vector3

@export var rotation_angles: Vector3

var mesh := ImmediateMesh.new()

func _ready() -> void:
	var a := unscaled_a * radius;
	var b := unscaled_b * radius;
	var c := unscaled_c * radius;
	
	var zGradient := sqrt(c*c+b*b) * length/4

	var pointA := centre + Vector3(a, 0, 0);
	var pointAIn := Vector3(c, -b, -zGradient)
	var pointAOut := Vector3(c, b, zGradient)
	var pointB := centre + Vector3(0, a, length/4);
	var pointBIn := Vector3(b, c, -zGradient)
	var pointBOut := Vector3(-b, c, zGradient)
	var pointC := centre + Vector3(-a, 0, length/2);
	var pointCIn := Vector3(-c, b, -zGradient)
	var pointCOut := Vector3(-c, -b, zGradient)
	var pointD := centre + Vector3(0, -a, length*3/4);
	var pointDIn := Vector3(-b, -c, -zGradient)
	var pointDOut := Vector3(b, -c, zGradient)

	curve.bake_interval = 0.01
	
	curve.add_point(pointA, Vector3.ZERO, pointAOut);
	curve.add_point(pointB, pointBIn, pointBOut);
	curve.add_point(pointC, pointCIn, pointCOut);
	curve.add_point(pointD, pointDIn, pointDOut);
	curve.add_point(pointA + Vector3(0, 0, length), pointAIn, Vector3.ZERO);

	$"../MeshInstance3D".mesh = mesh;

func _process(delta: float) -> void:
	var rotation_quat = Quaternion.from_euler(delta*rotation_angles*PI/180)
	mesh.clear_surfaces()
	mesh.surface_begin(ImmediateMesh.PRIMITIVE_LINE_STRIP)

	for point_id in curve.point_count:
		curve.set_point_position(point_id, rotation_quat * curve.get_point_position(point_id));
		curve.set_point_in(point_id, rotation_quat * curve.get_point_in(point_id));
		curve.set_point_out(point_id, rotation_quat * curve.get_point_out(point_id));

	for point in curve.get_baked_points():
		mesh.surface_add_vertex(point)
	
	mesh.surface_end()
