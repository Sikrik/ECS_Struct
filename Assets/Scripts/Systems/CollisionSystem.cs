using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    [BurstCompile]
    public partial struct CollisionSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 这里的 Query 增加了 WithNone<PendingDestroyTag>
            // 确保那些在这一帧已经被“寿命系统”标记要销毁的敌人不会再参与碰撞计算
            var enemyQuery = SystemAPI.QueryBuilder()
                .WithAll<LocalTransform, EnemyTag, HealthComponent, CollisionComponent>()
                .WithNone<PendingDestroyTag>() 
                .Build();
            
            var enemyEntities = enemyQuery.ToEntityArray(Allocator.TempJob);
            var enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            var enemyCollisions = enemyQuery.ToComponentDataArray<CollisionComponent>(Allocator.TempJob);

            // 使用 EndSimulation 阶段的 ECB 确保销毁在逻辑结束后执行
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            // 调度 Job
            state.Dependency = new CollisionJob
            {
                EnemyEntities = enemyEntities,
                EnemyTransforms = enemyTransforms,
                EnemyCollisions = enemyCollisions,
                Ecb = ecb,
                HealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(false),
                // 关键：在 Job 中也需要检查标签
                PendingLookup = SystemAPI.GetComponentLookup<PendingDestroyTag>(true) 
            }.ScheduleParallel(state.Dependency);

            // 安全释放
            enemyEntities.Dispose(state.Dependency);
            enemyTransforms.Dispose(state.Dependency);
            enemyCollisions.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    // 这里通过 IJobEntity 自动筛选带有这些组件且没有 PendingDestroyTag 的子弹
    [WithAll(typeof(BulletComponent), typeof(CollisionComponent))]
    [WithNone(typeof(PendingDestroyTag))]
    public partial struct CollisionJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> EnemyEntities;
        [ReadOnly] public NativeArray<LocalTransform> EnemyTransforms;
        [ReadOnly] public NativeArray<CollisionComponent> EnemyCollisions;
        
        public EntityCommandBuffer.ParallelWriter Ecb;
        
        public ComponentLookup<HealthComponent> HealthLookup;
        [ReadOnly] public ComponentLookup<PendingDestroyTag> PendingLookup;

        public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, in LocalTransform bulletTransform, in BulletComponent bullet, in CollisionComponent bulletCollision)
        {
            // 双重保险：如果在这一帧并行处理时，子弹已经被其他逻辑打上了标签，则跳过
            if (PendingLookup.HasComponent(entity)) return;

            for (int i = 0; i < EnemyTransforms.Length; i++)
            {
                Entity enemyEntity = EnemyEntities[i];
                
                // 如果敌人已经准备销毁了，不处理
                if (PendingLookup.HasComponent(enemyEntity)) continue;

                float combinedRadius = bulletCollision.Radius + EnemyCollisions[i].Radius;
                float checkThresholdSq = combinedRadius * combinedRadius;
                float distSq = math.distancesq(bulletTransform.Position, EnemyTransforms[i].Position);

                if (distSq < checkThresholdSq)
                {
                    // 扣血逻辑
                    if (HealthLookup.HasComponent(enemyEntity))
                    {
                        var health = HealthLookup[enemyEntity];
                        health.CurrentHealth -= bullet.Damage;
                        HealthLookup[enemyEntity] = health;
                    }

                    // 关键：不再直接 Destroy，而是打上标签通知 DestroySystem 处理
                    Ecb.AddComponent<PendingDestroyTag>(chunkIndex, entity);
                    break;
                }
            }
        }
    }
}