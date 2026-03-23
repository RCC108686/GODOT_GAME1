using Godot;
using System;

public partial class Bag : Node
{
	// Called when the node enters the scene tree for the first time.
[Export] public PackedScene[] Weapons = new PackedScene[3];

    public void AddWeapon(PackedScene newWeapon)
    {
        for (int i = 0; i < Weapons.Length; i++)
        {
            if (Weapons[i] == null)
            {
                Weapons[i] = newWeapon;
                GD.Print($"武器已添加到槽位: {i + 1}");
                return;
            }
        }
        GD.Print("背包已满");
    }

    public void RemoveWeapon(int index)
    {
        if (index < 0 || index >= Weapons.Length || Weapons[index] == null) return;
        Weapons[index] = null;
    }

    public PackedScene GetWeapon(int index)
    {
        if (index < 0 || index >= Weapons.Length) return null;
        return Weapons[index];
    }
}
