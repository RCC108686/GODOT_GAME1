using Godot;
using System;

public partial class butten : Area2D
{
	[Export] public float HHP = 1.0f; // 伤害
    [Export] public float AP = 1.0f;  // 穿甲/威力
    [Export] public float LifeTime = 1.0f; // 存活时间
    
    public Vector2 Velocity = Vector2.Zero; // 速度向量

    public override void _Ready()
    {
        // 对应 Unity 的 Destroy(gameObject, time)
        var timer = GetTree().CreateTimer(LifeTime);
        timer.Timeout += () => QueueFree();

        // 连接碰撞信号
        AreaEntered += OnAreaEntered;
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        // 处理位移
        Position += Velocity * (float)delta;
    }

    // 设置速度的方法（供 buttenfly.cs 调用）
    public void SetVelocity(Vector2 newVelocity)
    {
        Velocity = newVelocity;
    }

    // 处理碰撞逻辑
    private void OnBodyEntered(Node body)
    {
        if (body.IsInGroup("wall"))
        {
            GD.Print("击中墙壁");
            // 假设墙体有 hitee 方法
            if (body.HasMethod("hitee")) body.Call("hitee", AP);
            QueueFree(); // 销毁子弹
        }
        else if (body.IsInGroup("EM"))
        {
            GD.Print("击中敌人");
            // 假设敌人有 TakeDamage 方法
            if (body.HasMethod("TakeDamage")) body.Call("TakeDamage", HHP, AP);
            QueueFree();
        }
        else if (!body.IsInGroup("Player"))
        {
            QueueFree();
        }
    }

    private void OnAreaEntered(Area2D area)
    {
        // 如果敌人或墙体也是 Area2D，逻辑同上
        if (area.IsInGroup("EM"))
        {
            if (area.HasMethod("TakeDamage")) area.Call("TakeDamage", HHP, AP);
            QueueFree();
        }
    }

    public void ChangePower(float change)
    {
        HHP += change;
    }


}
