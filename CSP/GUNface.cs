using Godot;
using System;

public partial class GUNface : Polygon2D
{
	// Called when the node enters the scene tree for the first time.
[Export] public float AngleAdjustment = 0f; 

    public override void _Process(double delta)
    {
        MouseRotate();
    }

    private void MouseRotate()
    {
        // 1. 获取全局鼠标位置
        // 在 Godot 中，直接调用 GetGlobalMousePosition() 即可获取世界坐标
        // 替代了 Unity 的 Camera.main.ScreenToWorldPoint(Input.mousePosition)
        Vector2 mousePosition = GetGlobalMousePosition();

        // 2. 使用 Godot 内置的 LookAt 方法
        // 该方法会自动计算角度并旋转节点，使其正 X 轴（右侧）指向目标点
        // 替代了 Unity 复杂的 Mathf.Atan2 和 Quaternion.Euler 计算
        LookAt(mousePosition);

        // 3. 应用角度微调
        // 如果你的三角形是垂直画的，可能需要在此处加减 90 度
        if (AngleAdjustment != 0)
        {
            RotationDegrees += AngleAdjustment;
        }
    }
}
