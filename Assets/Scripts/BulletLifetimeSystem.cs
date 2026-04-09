using Unity.Entities;
using Unity.Burst;

/// <summary>
/// 子弹寿命系统：负责计时并在子弹到期时将其从实体世界中移除
/// </summary>
[BurstCompile]
public partial struct BulletLifetimeSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 获取支持并行写入的 ECB
        var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

        float deltaTime = SystemAPI.Time.DeltaTime;

        // 调度 Job 并行处理
        new BulletLifetimeJob
        {
            DeltaTime = deltaTime,
            Ecb = ecb
        }.ScheduleParallel();
    }
}

[BurstCompile]
public partial struct BulletLifetimeJob : IJobEntity
{
    public float DeltaTime;
    public EntityCommandBuffer.ParallelWriter Ecb;

    /// <summary>
    /// 对所有带有 BulletComponent 的实体执行生命周期检查
    /// </summary>
    public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, ref BulletComponent bullet)
    {
        bullet.CurrentLifeTime += DeltaTime;
        
        if (bullet.CurrentLifeTime >= bullet.MaxLifeTime)
        {
            // 时间结束，安全销毁实体
            Ecb.DestroyEntity(chunkIndex, entity);
        }
    }
}