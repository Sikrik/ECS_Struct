using Unity.Entities;
using Unity.Mathematics;

namespace Components
{
    /// <summary>
    /// 移动组件：存储实体的移动相关属性
    /// </summary>
    public struct MovementComponent : IComponentData
    {
        public float3 Velocity;      // 当前逻辑移动向量 (方向 * 1)
        public float BaseSpeed;     // 静态配置的基础移动速率
        public float CurrentSpeed;  // 动态结算后的实时移动速率
    }

    /// <summary>
    /// 玩家输入组件：记录玩家的移动输入指令
    /// </summary>
    public struct PlayerInputComponent : IComponentData
    {
        public float2 Movement;     // 归一化后的水平面移动向量 (X, Z)
    }

    /// <summary>
    /// 血量组件：管理实体的生命值状态
    /// </summary>
    public struct HealthComponent : IComponentData
    {
        public float CurrentHealth; // 当前剩余血量
        public float MaxHealth;     // 最大血量上限
    }

    /// <summary>
    /// 子弹组件：定义子弹的伤害和生命周期属性
    /// </summary>
    public struct BulletComponent : IComponentData
    {
        public float Damage;           // 该子弹命中的伤害强度
        public float MaxLifeTime;      // 子弹可飞行的最大时长（秒）
        public float CurrentLifeTime;  // 已飞行的时长计数
    }
    /// <summary>
    /// 碰撞属性组件：定义实体的物理碰撞半径
    /// </summary>
    public struct CollisionComponent : IComponentData
    {
        public float Radius; // 碰撞半径
    }
}