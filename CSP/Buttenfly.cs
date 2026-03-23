using Godot;
using System;

public partial class Buttenfly : Node2D
{
    // 定义左键射击模式
    public enum FireMode { Semi, Auto }
    // 定义右键特殊模式
    public enum SpecialMode { Explosive, Precise, NoMore }

    [ExportGroup("快慢机设置")]
    [Export] public bool CanSwitchMode = true;        // 是否允许按T切换模式
    [Export] public FireMode CurrentFireMode = FireMode.Semi; // 当前模式（单发/连发）

    [ExportGroup("射击基础设置")]
    [Export] public float FireRate = 0.1f;            // 射击间隔
    [Export] public float Speed = 800f;               // 子弹速度
    [Export] public float MaxSpread = 5.0f;           // 基础散布角度
    [Export] public int MaxAmmo = 10;                 // 弹匣容量
    [Export] public PackedScene BulletPrefab;         // 子弹预制件

    [ExportGroup("右键特殊模式")]
    [Export] public SpecialMode SpecialFire = SpecialMode.NoMore; // 在编辑器里直接选右键是啥

    [ExportGroup("UI与引用")]
    [Export] public Texture2D[] AmmoSprites;          // 弹匣贴图集


    [ExportGroup("连发设置")]
    [Export] public int BurstCount = 3;        // 自定义连发数，编辑器可调
    [Export] public float BurstInterval = 0.08f; // 连发每发之间的间隔

    [Export] public float ExecutSpread = 80f; //连发惩罚


    [ExportGroup("精度设置")]
    [Export] public float spreadBonus = 0.3f;

      //  float Speeds;


    private int _currentAmmo;
    private float _nextFireTime = 0f;
    private Marker2D _muzzle;
    private Node _player;

    public override void _Ready()
    {
        _currentAmmo = MaxAmmo;
        _muzzle = GetNodeOrNull<Marker2D>("Muzzle");
        _player = GetTree().GetFirstNodeInGroup("Player"); 
        
     //   Speeds = Speed*100;
        
        PushUpdateUI();
    }

    public override void _Process(double delta)
    {
        // 枪械朝向：始终指向鼠标
        LookAt(GetGlobalMousePosition());
        HandleInput((float)delta);
    }

    private void HandleInput(float delta)
    {
        if (_nextFireTime > 0) _nextFireTime -= delta;

        // 1. 快慢机切换 (T键)
        if (CanSwitchMode && Input.IsActionJustPressed("switch_fire_mode"))
        {
            SwitchFireMode();
        }

        // 2. 左键射击判定 (单发 vs 连发)
        bool tryingToFire = false;
        if (CurrentFireMode == FireMode.Auto)
            tryingToFire = Input.IsActionPressed("fire");     // 连发模式：长按
        else
            tryingToFire = Input.IsActionJustPressed("fire"); // 单发模式：点按

        if (tryingToFire && _nextFireTime <= 0 && _currentAmmo > 0)
        {
            Shoot();
        }

        // 3. 右键特殊模式判定 (specialfire)
        if (Input.IsActionJustPressed("specialfire") && _nextFireTime <= 0 && _currentAmmo > 0)
        {
           if (SpecialFire == SpecialMode.Explosive)
            {
                ExecuteBurst(); // 现在内部有弹药检查了
            }
          else
            {
                ExecuteSpecialMode();
            }
        }

        // 4. 装弹 (R键)
        if (Input.IsActionJustPressed("reload"))
        {
            Reload();
            GD.Print($"[换弹] : { _currentAmmo}发");
    
        }
    }

    private void SwitchFireMode()
    {
        CurrentFireMode = (CurrentFireMode == FireMode.Semi) ? FireMode.Auto : FireMode.Semi;
        GD.Print($"[快慢机] 切换至: {CurrentFireMode}");
    }

    private void ExecuteSpecialMode()
    {
        switch (SpecialFire)
        {
            case SpecialMode.Explosive:
                ExecuteBurst(); // 爆发射击
                break;
            case SpecialMode.Precise:
                Shoot(spreadBonus);   // 精确射击：减少散布
                break;
            case SpecialMode.NoMore:
                Shoot();        // 普通射击
                break;
        }
    }

    /// <summary>
    /// 射击方法：生成子弹并设置其位置、方向和速度
    /// </summary>
    /// <param name="spreadBonus">散布加成值，正值增加散布，负值减少散布</param>
    private void Shoot(float spreadBonus = 0)
    {
        // 安全检查：确保子弹预制件存在
        if (BulletPrefab == null) return;

    // --- 修改部分：正态分布散布计算 ---
    // 1. 获取基础的正态分布随机值 (均值为0，标准差为1)
    // GD.Randfn(mean, deviation) 
    // 我们将 MaxSpread 作为标准差，这样约 68% 的子弹会落在 MaxSpread 范围内
          float randomAngle = (float)GD.Randfn(0, MaxSpread + spreadBonus); 
    
    // 2. 将随机角度加到当前的枪口旋转上
         float finalRotation = GlobalRotationDegrees + randomAngle;
        // 实例化子弹并添加到场景树
        Node2D bullet = BulletPrefab.Instantiate<Node2D>();
        GetTree().Root.AddChild(bullet);
        
        // 设置子弹初始位置：优先使用枪口位置，否则使用武器自身位置
        bullet.GlobalPosition = _muzzle != null ? _muzzle.GlobalPosition : GlobalPosition;
        
        // 设置子弹旋转角度：武器朝向 + 散布偏移
        bullet.GlobalRotationDegrees = finalRotation;

        // 如果子弹有SetVelocity方法，设置子弹速度
        // 速度方向为子弹当前旋转方向，大小为Speed
        if (bullet.HasMethod("SetVelocity"))
            bullet.Call("SetVelocity", Vector2.FromAngle(bullet.GlobalRotation) * Speed);

        // 更新弹药数和射击冷却
        _currentAmmo--;
        _nextFireTime = FireRate;
        GD.Print($"发射 : { _currentAmmo}发");
        PushUpdateUI();
    }

    private async void ExecuteBurst()
    {
   
        int actualShots = Math.Min(BurstCount, _currentAmmo);

        for (int i = 0; i < actualShots; i++)
        {
            // 再次保险检查：防止异步执行期间弹药被其他逻辑（如丢弃/重置）修改
            if (_currentAmmo <= 0) break;

            Shoot(ExecutSpread); 

            await ToSignal(GetTree().CreateTimer(BurstInterval), "timeout");
        }
        
        
        _nextFireTime = FireRate;
    }







    private void Reload()
    {
        _currentAmmo = MaxAmmo;
        PushUpdateUI();
    }

    private void PushUpdateUI()
    {
        if (_player != null && _player.HasMethod("UpdateWeaponUI"))
        {
            _player.Call("UpdateWeaponUI", _currentAmmo, MaxAmmo, AmmoSprites);
        }
    }
}