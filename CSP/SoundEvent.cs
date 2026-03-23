using Godot;
using System;
using System.Collections.Generic;

// 用于描述一个声音事件的结构体
public struct SoundEvent
{
    public Vector2 Position; // 声音来源位置
    public float Radius;     // 声音传播半径
    public ulong TimeStamp;  // 声音产生的时间戳（微秒）
}

public partial class SoundSystem : Node
{
    // 存储当前场景中所有活跃的声音事件
    private List<SoundEvent> _activeEvents = new List<SoundEvent>();
    
    // 声音事件的有效时间（秒），超过这个时间声音消失
    [Export] public float SoundDuration = 0.1f; 

    public override void _PhysicsProcess(double delta)
    {
        // 每一帧清理过时的声音事件
        ulong currentTime = Time.GetTicksUsec();
        // 将秒转换为微秒进行比较
        ulong durationUsec = (ulong)(SoundDuration * 1000000);

        _activeEvents.RemoveAll(e => (currentTime - e.TimeStamp) > durationUsec);
    }

    // 主角调用此方法发射声音
    public void EmitSound(Vector2 position, float radius)
    {
        _activeEvents.Add(new SoundEvent
        {
            Position = position,
            Radius = radius,
            TimeStamp = Time.GetTicksUsec()
        });
    }

    // --- 供敌人决策树调用的接口 ---

    // 检查某个位置是否能听到任何声音
    public bool CanHearSoundAt(Vector2 listenerPosition, out Vector2 closestSoundSource)
    {
        closestSoundSource = Vector2.Zero;
        float minDistance = float.MaxValue;
        bool heard = false;

        foreach (var sound in _activeEvents)
        {
            float distToSource = listenerPosition.DistanceTo(sound.Position);
            
            // 如果监听者在声音半径内
            if (distToSource <= sound.Radius)
            {
                heard = true;
                // 找到最近的声音源（可选，用于敌人转向）
                if (distToSource < minDistance)
                {
                    minDistance = distToSource;
                    closestSoundSource = sound.Position;
                }
            }
        }
        return heard;
    }
}