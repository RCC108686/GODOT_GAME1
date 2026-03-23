using Godot;
using System;

public partial class SpreadVisualizer : Node2D
{
	// Called when the node enters the scene tree for the first time.
	[Export] public bool ShowGuide = true;       // 是否显示辅助线
    [Export] public Color LineColor = new Color(1, 0, 0, 0.5f); // 颜色（半透明红）
    [Export] public float LineLength = 500f;     // 线条长度
    
    // 引用主脚本获取参数
    private Buttenfly _weapon;

    public override void _Ready()
    {
        // 尝试获取父节点或同级节点的 Buttenfly 脚本
        _weapon = GetParent<Buttenfly>();
    }

    public override void _Process(double delta)
    {
        if (ShowGuide)
        {
            QueueRedraw(); // 每一帧重新绘制，跟随鼠标旋转
        }
    }

    public override void _Draw()
    {
        if (!ShowGuide || _weapon == null) return;

        // 绘制三条核心参考线
        // 1. 中心准星线 (均值线)
        DrawLine(Vector2.Zero, Vector2.Right * LineLength, LineColor, 1.0f);

        // 2. 正态分布的“标准差”边界线 (约68%的子弹落在内圈)
        // 对应代码中的 GD.Randfn(0, MaxSpread)
        float sigmaAngle = Mathf.DegToRad(_weapon.MaxSpread);
        DrawSpreadLine(sigmaAngle, LineColor);
        DrawSpreadLine(-sigmaAngle, LineColor);

        // 3. 绘制外围弱色范围 (3倍标准差，几乎100%的子弹都在这里)
        Color outerColor = new Color(LineColor.R, LineColor.G, LineColor.B, 0.1f);
        DrawSpreadLine(sigmaAngle * 3, outerColor);
        DrawSpreadLine(-sigmaAngle * 3, outerColor);
    }

    private void DrawSpreadLine(float angleRad, Color color)
    {
        Vector2 direction = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
        DrawLine(Vector2.Zero, direction * LineLength, color, 1.0f);
    }
}
