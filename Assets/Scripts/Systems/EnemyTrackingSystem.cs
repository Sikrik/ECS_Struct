using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    /// <summary>
    /// 敌人追踪系统（怪物 AI 系统）
    /// 负责计算所有敌人的移动方向，使其朝向玩家移动
    /// 使用简单的向心向量算法实现基础追踪 AI
    /// 支持 Burst 编译和并行处理，可同时高效更新大量敌人单位
    /// </summary>
    [BurstCompile]
    public partial struct EnemyTrackingSystem : ISystem
    {
        /// <summary>
        /// 系统更新方法，每帧调用一次
        /// 获取玩家位置并调度并行 Job 更新所有敌人的移动意图
        /// </summary>
        /// <param name="state">系统状态引用，用于访问 ECS 世界和依赖管理</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 获取带有 PlayerTag 的玩家实体位置
            // 这是所有敌人追踪的目标点
            // 如果玩家不存在，跳过本帧更新以避免错误
            if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity playerEntity)) return;
            float3 playerPos = SystemAPI.GetComponent<LocalTransform>(playerEntity).Position;

            // 2. 调度并行 Job 驱动所有带有 EnemyTag 的敌人单位
            // 将玩家位置传递给 Job，每个敌人独立计算朝向玩家的移动方向
            // ScheduleParallel 自动利用多核 CPU 并行处理所有敌人
            // 通过 state.Dependency 维护任务依赖链，确保正确的执行顺序
            state.Dependency = new EnemyTrackJob 
            { 
                PlayerPos = playerPos 
            }.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    /// 敌人追踪 Job
    /// 对每个带有 EnemyTag 和 MovementComponent 的实体执行追踪逻辑
    /// 使用 Burst 编译以获得最佳性能
    /// </summary>
    [BurstCompile]
    public partial struct EnemyTrackJob : IJobEntity
    {
        /// <summary>
        /// 玩家当前位置（追踪目标点）
        /// 由主系统在 OnUpdate 中传入
        /// </summary>
        public float3 PlayerPos;

        /// <summary>
        /// 对每个敌人执行追踪逻辑
        /// 计算朝向玩家的归一化方向向量，并设置到 Velocity 中
        /// 后续的 MovementSystem 会根据此速度实际移动敌人
        /// </summary>
        /// <param name="movement">敌人移动组件引用，用于写入计算出的速度向量</param>
        /// <param name="transform">敌人当前变换组件，提供位置信息</param>
        /// <param name="tag">敌人标签组件，用于筛选实体（无需读取数据，仅作为标记）</param>
        public void Execute(ref MovementComponent movement, in LocalTransform transform, in EnemyTag tag)
        {
            // 计算从敌人指向玩家的方向向量
            float3 direction = PlayerPos - transform.Position;
            
            // 使用长度平方进行距离判断，避免开方运算，提高性能
            // lengthsq 返回的是向量长度的平方值
            float distSq = math.lengthsq(direction);

            // 距离阈值判定：防止敌人与玩家重叠时产生物理抖动或除零错误
            // 当距离大于 0.316 (sqrt(0.1)) 时才进行移动
            if (distSq > 0.1f)
            {
                // 归一化方向向量，得到单位长度的移动方向
                // normalize 会将向量长度缩放到 1，保持方向不变
                // 后续 MovementSystem 会乘以实际速度值
                movement.Velocity = math.normalize(direction);
            }
            else
            {
                // 距离过近时停止移动，避免与玩家位置完全重合导致的抖动
                // 这也能模拟敌人到达目标后的停止行为
                movement.Velocity = float3.zero;
            }
        }
    }
}
