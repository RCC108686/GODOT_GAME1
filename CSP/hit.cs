using Godot;
using System;

public partial class hit : Node2D
{
	//
	// 当前血量
    [Export] public float CurrentHealth = 100f;
    // 当前护甲值
    [Export] public float Armor = 50f;
public override void _Process(double delta)
    {
        if (CurrentHealth <= 0)
        {
            GD.Print("目标已被摧毁");
            QueueFree(); // 血量归零，销毁对象
        }
    }

    // 接收伤害的方法
    public void TakeDamage(float incomingDamage, float armorPenetration)
    {
        // 计算护甲削弱
        Armor -= armorPenetration;
        if (Armor < 0) Armor = 0; // 护甲值不能为负

        // 计算实际伤害
        float effectiveDamage = incomingDamage - Armor;
        if (effectiveDamage < 0) effectiveDamage = 0; // 伤害不能为负

        // 扣除血量
        if (effectiveDamage > 0)
        {
            CurrentHealth -= effectiveDamage;
            GD.Print($"受到伤害: {effectiveDamage}, 剩余血量: {CurrentHealth}");
        }
        else
        {
            GD.Print("刮痧刮不了一点");
        }
    }
}
