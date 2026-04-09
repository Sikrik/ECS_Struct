using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;

/// <summary>
/// 怪物 AI 系统：计算敌人的寻路意图。
/// </summary>
[BurstCompile]
public partial struct EnemyTrackingSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. 高频获取玩家位置单例
        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity playerEntity)) return;
        float3 playerPos = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

        // 2. 调度并行 Job 驱动所有怪物单位
        state.Dependency = new EnemyTrackJob 
        { 
            PlayerPos = playerPos 
        }.ScheduleParallel(state.Dependency);
    }
}

[BurstCompile]
public partial struct EnemyTrackJob : IJobEntity
{
    public float3 PlayerPos;

    // 逻辑：计算向心向量并归一化
    public void Execute(ref MovementComponent movement, in LocalTransform transform, in EnemyTag tag)
    {
        float3 direction = PlayerPos - transform.Position;
        float distSq = math.lengthsq(direction);

        // 距离阈值判定：防止单位重叠时的物理抖动
        if (distSq > 0.1f)
        {
            movement.Velocity = math.normalize(direction);
        }
        else
        {
            movement.Velocity = float3.zero;
        }
    }
}