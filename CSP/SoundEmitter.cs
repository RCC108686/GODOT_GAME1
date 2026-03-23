using Godot;
using System;

public partial class SoundEmitter : Node2D
{
    [ExportGroup("声音半径设置")]
    [Export] public float IdleRadius = 0f;          // 静止时的声音（通常为0）
    [Export] public float WalkRadius = 200f;        // 移动时的声音范围
    [Export] public float ShootRadius = 800f;       // 开枪时的声音范围

    [ExportGroup("可视化设置")]
    [Export] public Color SoundRingColor = new Color(1, 1, 1, 0.3f); // 半透明白色
    [Export] public float RingFadeSpeed = 5f;        // 圆环消失速度

    private float _currentDrawRadius = 0f;
    private float _targetDrawRadius = 0f;
    private float _ringAlpha = 0f;
    
    // 引用主角的物理节点，用于检测速度
    private CharacterBody2D _playerBody;
    // 全局声音系统单例的引用
    private SoundSystem _soundSystem;

    public override void _Ready()
    {
        // 假设父节点是 CharacterBody2D
        _playerBody = GetParent<CharacterBody2D>();
        
        // 获取全局单例
        _soundSystem = GetNode<SoundSystem>("/root/SoundSystem");
        
        if (_soundSystem == null)
        {
            GD.PrintErr("错误：未找到全局 SoundSystem Autoload！");
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_playerBody == null || _soundSystem == null) return;

        float emitRadius = IdleRadius;

        // 1. 检测移动声音
        if (_playerBody.Velocity.Length() > 10f) // 有明显移动
        {
            emitRadius = WalkRadius;
        }

        // 2. 将声音信号发送给全局系统
        if (emitRadius > 0)
        {
            _soundSystem.EmitSound(GlobalPosition, emitRadius);
            
            // 用于可视化绘制
            TriggerRingVisual(emitRadius);
        }
    }

    // --- 供开枪脚本调用的接口 ---
    
    // 在 Buttenfly.cs 的 Shoot() 方法里调用此方法
    public void EmitShootSound()
    {
        if (_soundSystem != null)
        {
            _soundSystem.EmitSound(GlobalPosition, ShootRadius);
            TriggerRingVisual(ShootRadius);
        }
    }

    // --- 可视化绘制逻辑 ---

    private void TriggerRingVisual(float radius)
    {
        // 如果新声音半径更大，则更新
        if (radius > _targetDrawRadius)
        {
            _targetDrawRadius = radius;
        }
        _ringAlpha = 1.0f; // 重置透明度为不透明
    }

    public override void _Process(double delta)
    {
        // 每一帧更新圆环的透明度和大小，使其产生淡出效果
        if (_ringAlpha > 0)
        {
            _ringAlpha -= RingFadeSpeed * (float)delta;
            // 圆环半径轻微平滑扩张
            _currentDrawRadius = Mathf.Lerp(_currentDrawRadius, _targetDrawRadius, 10 * (float)delta);
            
            if (_ringAlpha <= 0)
            {
                _ringAlpha = 0;
                _targetDrawRadius = 0;
                _currentDrawRadius = 0;
            }
            QueueRedraw(); // 触发 _Draw
        }
    }

    public override void _Draw()
    {
        if (_ringAlpha > 0 && _currentDrawRadius > 0)
        {
            Color drawColor = SoundRingColor;
            drawColor.A = _ringAlpha * SoundRingColor.A; // 应用淡出透明度
            
            // 绘制空心圆环
            DrawCircle(Vector2.Zero, _currentDrawRadius, drawColor, false, 4.0f); 
        }
    }
}