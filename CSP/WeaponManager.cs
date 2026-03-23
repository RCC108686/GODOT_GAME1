using Godot;
using System;

public partial class WeaponManager : Node
{
// 1. 将 PlayerBag 改为通过 [Export] 拖动赋值
    [Export] public Bag PlayerBag; 

    // 2. 将 _gunSlot 改为 [Export] 并在编辑器中指定挂载点节点
    [Export] private Node2D _gunSlot;               

    private PackedScene _currentWeaponRes; 
    private Node2D _weaponInstance;        

    public override void _Ready()
    {
        // 移除了硬编码的 GetNode<Node2D>("../GunSlot")
        
        // 增加安全检查，防止忘记在编辑器里拖动赋值
        if (PlayerBag == null)
        {
            GD.PrintErr("错误：未在编辑器中为 WeaponManager 分配 PlayerBag！");
            return;
        }
        if (_gunSlot == null)
        {
            GD.PrintErr("错误：未在编辑器中为 WeaponManager 分配 GunSlot！");
            return;
        }

        InitializeFirstWeapon();
    }

    public override void _Process(double delta)
    {
        // 鼠标滚轮切换武器 (Godot 4 推荐在 Input Map 配置 mouse_wheel_up/down)
        if (Input.IsActionJustReleased("wheel_up")) 
        {
            SwapWeapon(1);
        }
        else if (Input.IsActionJustReleased("wheel_down"))
        {
            SwapWeapon(-1);
        }

        // 按下 C 键丢弃 (需要在 Input Map 配置 "drop_weapon")
        if (Input.IsActionJustPressed("drop_weapon"))
        {
            DropWeapon();
        }
    }

    private void InitializeFirstWeapon()
    {
        foreach (var weapon in PlayerBag.Weapons)
        {
            if (weapon != null)
            {
                _currentWeaponRes = weapon;
                SpawnWeapon(_currentWeaponRes);
                return;
            }
        }
    }

    private void SpawnWeapon(PackedScene weaponPrefab)
    {
        if (weaponPrefab == null) return;

        // 1. 销毁旧实例 (对应 Unity 的 Destroy)
        if (_weaponInstance != null)
        {
            _weaponInstance.QueueFree(); 
        }

        // 2. 实例化 (对应 Unity 的 Instantiate)
        _weaponInstance = weaponPrefab.Instantiate<Node2D>();

        // 3. 挂载到槽位 (对应 Unity 的 SetParent)
        _gunSlot.AddChild(_weaponInstance);

        // 4. 重置位置
        _weaponInstance.Position = Vector2.Zero;
    }

    private void SwapWeapon(int direction)
    {
        int currentIndex = GetCurrentWeaponIndex();
        // 这里沿用你 Unity 脚本里的寻找下一个有效索引的逻辑
        int nextIndex = FindNextValidWeaponIndex(currentIndex);

        if (nextIndex != -1)
        {
            _currentWeaponRes = PlayerBag.Weapons[nextIndex];
            SpawnWeapon(_currentWeaponRes);
        }
    }

    private int GetCurrentWeaponIndex()
    {
        for (int i = 0; i < PlayerBag.Weapons.Length; i++)
        {
            if (PlayerBag.Weapons[i] == _currentWeaponRes) return i;
        }
        return -1;
    }

    // 寻找下一个逻辑同你之前的代码...
    private int FindNextValidWeaponIndex(int currentIndex)
    {
        for (int i = 1; i <= PlayerBag.Weapons.Length; i++)
        {
            int nextIndex = (currentIndex + i) % PlayerBag.Weapons.Length;
            if (PlayerBag.Weapons[nextIndex] != null) return nextIndex;
        }
        return -1;
    }

private void DropWeapon()
{
    // 1. 安全检查：如果手里本来就没枪，直接返回
    if (!GodotObject.IsInstanceValid(_weaponInstance))
    {
        GD.Print("手里没有武器可以丢弃");
        return;
    }

    int currentIndex = GetCurrentWeaponIndex();
    if (currentIndex != -1)
    {
        // 2. 从背包数据中移除引用 (对应你 Unity 的 bag.RemoveWeapon)
        PlayerBag.RemoveWeapon(currentIndex);

        // 3. 销毁场景中的武器实例
        // 使用 QueueFree() 确保安全销毁，并将引用设为 null 防止空指针
        _weaponInstance.QueueFree();
        _weaponInstance = null;
        _currentWeaponRes = null;

        GD.Print($"武器已从槽位 {currentIndex + 1} 丢弃");

        // 4. 自动尝试切换到下一把可用的武器
        int nextIndex = FindNextValidWeaponIndex(currentIndex);
        if (nextIndex != -1)
        {
            _currentWeaponRes = PlayerBag.Weapons[nextIndex];
            SpawnWeapon(_currentWeaponRes);
        }
        else
        {
            GD.Print("背包已空，进入徒手状态");
        }
    }
}


}
