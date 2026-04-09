using Unity.Entities;
using Unity.Mathematics;

// ==========================================
// 1. 移动与空间状态 (Movement & Spatial)
// ==========================================

/// <summary>
/// 基础移动组件：存储物理位移所需的核心数值。
/// 使用 float3 (SIMD 优化) 替代 Vector3 以获得更高的计算吞吐量。
/// </summary>
public struct MovementComponent : IComponentData
{
    public float3 Velocity;      // 当前逻辑移动向量 (方向 * 1)
    public float BaseSpeed;     // 静态配置的基础移动速率
    public float CurrentSpeed;  // 动态结算后的实时移动速率
}

/// <summary>
/// 玩家输入快照：将主线程的输入意图缓存到逻辑实体中。
/// </summary>
public struct PlayerInputComponent : IComponentData
{
    public float2 Movement;     // 归一化后的水平面移动向量 (X, Z)
}

// ==========================================
// 2. 战斗与生命周期 (Combat & Lifecycle)
// ==========================================

/// <summary>
/// 战斗基础属性：管理实体的生命存续状态。
/// </summary>
public struct HealthComponent : IComponentData
{
    public float CurrentHealth; // 当前剩余血量
    public float MaxHealth;     // 最大血量上限
}

/// <summary>
/// 子弹性能参数：控制子弹的破坏力与存在时长。
/// </summary>
public struct BulletComponent : IComponentData
{
    public float Damage;           // 该子弹命中的伤害强度
    public float MaxLifeTime;      // 子弹可飞行的最大时长（秒）
    public float CurrentLifeTime;  // 已飞行的时长计数
}

// ==========================================
// 3. 全局配置单例 (Global Config)
// ==========================================

/// <summary>
/// 子弹实例化配置：作为单例存在，存储经烘焙后的实体预制体。
/// </summary>
public struct BulletPrefabConfig : IComponentData
{
    public Entity BulletPrefab; // 子弹实体模板句柄
}

// ==========================================
// 4. 类型筛选器 (Tag Components)
// 不占内存的标志位，用于高效的实体查询筛选。
// ==========================================
public struct PlayerTag : IComponentData { } // 标记唯一玩家
public struct EnemyTag  : IComponentData { } // 标记敌人单位
public struct BulletTag : IComponentData { } // 标记子弹实体