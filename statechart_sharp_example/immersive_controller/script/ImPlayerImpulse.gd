extends Node

var im_player: ImPlayer;


func _ready():
	im_player = get_parent() as ImPlayer;


func _ri_player_impulse(duct: StatechartDuct):
	var delta = duct.PhysicsDelta;
	var vel: Vector3 = im_player.Vel;
	for i: int in range(im_player.get_slide_collision_count()):
		var collision = im_player.get_slide_collision(i);
		var body = collision.get_collider();
		if body is RigidBody3D:
			if body.get_collision_layer_value(3):
				var impluse_dir = -collision.get_normal();
				var implse_force = max(impluse_dir.dot(vel), 0.0) * 400.0 * delta;
				body.apply_central_impulse(impluse_dir * implse_force);

