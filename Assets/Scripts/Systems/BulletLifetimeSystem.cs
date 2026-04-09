using Components;
using Unity.Burst;
using Unity.Entities;

namespace Systems
{
    /// <summary>
    /// 子弹寿命系统：负责计时并在子弹到期时将其从实体世界中移除
    /// 该系统使用 ECS DOTS 架构，支持 Burst 编译和并行处理
    /// 在 EndSimulation 阶段执行，确保所有逻辑更新完成后再处理子弹销毁
    /// </summary>
    [BurstCompile]
    public partial struct BulletLifetimeSystem : ISystem
    {
        /// <summary>
        /// 系统更新方法，每帧调用一次
        /// 负责调度子弹寿命检查的并行任务
        /// </summary>
        /// <param name="state">系统状态引用，用于访问 ECS 世界和相关系统</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 获取支持并行写入的 ECB（Entity Command Buffer）
            // ECB 允许在并行 Job 中安全地执行实体操作（如销毁）
            // 使用 EndSimulationEntityCommandBufferSystem 确保在帧结束时执行命令
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // 获取上一帧的时间增量，用于累加子弹存活时间
            float deltaTime = SystemAPI.Time.DeltaTime;

            // 调度 Job 并行处理所有带有 BulletComponent 的实体
            // ScheduleParallel 会自动利用多核 CPU 进行并行计算
            new BulletLifetimeJob
            {
                DeltaTime = deltaTime,
                Ecb = ecb
            }.ScheduleParallel();
        }
    }

    /// <summary>
    /// 子弹寿命检查 Job
    /// 使用 IJobEntity 接口，自动筛选并处理所有带有 BulletComponent 的实体
    /// 支持 Burst 编译以获得最佳性能
    /// </summary>
    [BurstCompile]
    public partial struct BulletLifetimeJob : IJobEntity
    {
        /// <summary>
        /// 时间增量，用于累加子弹的存活时间
        /// </summary>
        public float DeltaTime;
        
        /// <summary>
        /// 并行实体命令缓冲区
        /// 用于安全地在并行 Job 中销毁实体
        /// chunkIndex 参数确保命令的正确排序和执行
        /// </summary>
        public EntityCommandBuffer.ParallelWriter Ecb;

        /// <summary>
        /// 对每个带有 BulletComponent 的实体执行生命周期检查
        /// 该方法会被自动调用，无需手动遍历实体
        /// </summary>
        /// <param name="entity">当前处理的实体引用</param>
        /// <param name="chunkIndex">实体所在块索引，用于 ECB 并行写入</param>
        /// <param name="bullet">子弹组件的引用，包含寿命信息</param>
        public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref BulletComponent bullet)
        {
            // 累加子弹的当前存活时间
            bullet.CurrentLifeTime += DeltaTime;
        
            // 检查子弹是否超过最大存活时间
            if (bullet.CurrentLifeTime >= bullet.MaxLifeTime)
            {
                // 子弹寿命到期，通过 ECB 安全销毁实体
                // 使用 chunkIndex 确保并行写入的安全性
                // 满足销毁条件时，不再 Destroy，而是打标签
                if (bullet.CurrentLifeTime >= bullet.MaxLifeTime)
                {
                    Ecb.AddComponent<PendingDestroyTag>(chunkIndex, entity);
                }
            }
        }
    }
}
