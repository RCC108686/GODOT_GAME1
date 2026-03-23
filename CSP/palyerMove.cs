using Godot;
using System;

public partial class palyerMove : CharacterBody2D
{
	[Export] public float Speed = 200.0f;
	[Export] public float RunSpeedExtra = 100.0f;
	private AnimatedSprite2D _anima; // 假设你使用 AnimatedSprite2D，或者用 AnimationPlayer
	private float _nowSpeed;
	
	public override void _Ready()
	{
		// 获取动画节点引用
		_anima = GetNode<AnimatedSprite2D>("Anima");
	}

	public override void _PhysicsProcess(double delta)
	{
		// 1. 获取输入方向
		// 在 Godot 4 中，推荐在 Input Map 中配置 "left", "right", "up", "down"
		Vector2 inputDirection = Input.GetVector("ui_left", "ui_right", "ui_up", "ui_down");

		// 2. 处理加速逻辑 (Shift 键)
		if (Input.IsActionPressed("ui_shift")) // 需要在 Input Map 配置 ui_shift 对应 Left Shift
		{
			_nowSpeed = Speed + RunSpeedExtra;
		}
		else
		{
			_nowSpeed = Speed;
		}

		// 3. 执行移动
		// 注意：Godot 的 Velocity 是内置属性，不需要手动 transform.position += ...
		if (inputDirection != Vector2.Zero)
		{
			Velocity = inputDirection * _nowSpeed;
			// 可以在这里播放跑动动画：_anima.Play("run");
		}
		else
		{
			Velocity = Vector2.Zero;
			// 可以在这里播放待机动画：_anima.Play("idle");
		}

		// MoveAndSlide 会根据 Velocity 自动处理移动和物理碰撞
		// 它会自动应用 delta，所以不需要像 Unity 那样手动计算
		MoveAndSlide();

		// 4. 消除抖动与日志记录
		if (inputDirection != Vector2.Zero)
		{
			// 如果需要记录距离，可以在这里累加：Velocity.Length() * (float)delta
			// GD.Print($"当前速度: {_nowSpeed}"); 
		}

}
}
