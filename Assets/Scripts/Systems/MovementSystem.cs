using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

/// <summary>
/// 基础位移系统：这是逻辑层与物理表现层的最后同步环节。
/// 职责：根据结算后的 Velocity 更新实体的渲染坐标。
/// </summary>
[BurstCompile]
public partial struct MovementSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. 捕获每帧增量时间
        float deltaTime = SystemAPI.Time.DeltaTime;

        // 2. 调度并行作业：计算所有带位移组件实体的坐标更新
        state.Dependency = new MoveJob 
        { 
            DeltaTime = deltaTime 
        }.ScheduleParallel(state.Dependency);
    }
}

/// <summary>
/// 高性能并行移动作业：完全脱离主线程运行。
/// </summary>
[BurstCompile]
public partial struct MoveJob : IJobEntity
{
    public float DeltaTime;

    // 逻辑：位置 = 位置 + (方向向量 * 实时速率 * 时间)
    public void Execute(ref LocalTransform transform, in MovementComponent movement)
    {
        transform.Position += movement.Velocity * movement.CurrentSpeed * DeltaTime;
    }
}