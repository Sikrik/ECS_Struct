using Unity.Entities;
using Unity.Mathematics;

public struct MovementComponent : IComponentData
{
    public float3 Velocity;      // 当前逻辑移动向量 (方向 * 1)
    public float BaseSpeed;     // 静态配置的基础移动速率
    public float CurrentSpeed;  // 动态结算后的实时移动速率
}

public struct PlayerInputComponent : IComponentData
{
    public float2 Movement;     // 归一化后的水平面移动向量 (X, Z)
}

public struct HealthComponent : IComponentData
{
    public float CurrentHealth; // 当前剩余血量
    public float MaxHealth;     // 最大血量上限
}

public struct BulletComponent : IComponentData
{
    public float Damage;           // 该子弹命中的伤害强度
    public float MaxLifeTime;      // 子弹可飞行的最大时长（秒）
    public float CurrentLifeTime;  // 已飞行的时长计数
}
