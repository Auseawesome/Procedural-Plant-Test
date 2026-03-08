extends Node3D

func _ready() -> void:
	Input.mouse_mode = Input.MOUSE_MODE_CAPTURED

func _process(delta: float) -> void:
	var horizontal_vector := Input.get_vector("move_left", "move_right", "move_forwards", "move_backwards");
	var vertical_magnitude = Input.get_action_strength("move_up") - Input.get_action_strength("move_down");
	
	position += (transform.basis.get_rotation_quaternion() * Vector3(horizontal_vector.x, 0, horizontal_vector.y) + Vector3(0, vertical_magnitude, 0)) * delta * 10;

func _input(event: InputEvent) -> void:
	if event is InputEventMouseMotion:
		rotation.y += -event.relative.x / 200;
		rotation.x = clamp(rotation.x - event.relative.y / 200, -PI/2, PI/2);
