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
            // 1. 增加对 CollisionComponent 的查询
            var enemyQuery = SystemAPI.QueryBuilder().WithAll<LocalTransform, EnemyTag, HealthComponent, CollisionComponent>().Build();
            
            var enemyEntities = enemyQuery.ToEntityArray(Allocator.TempJob);
            var enemyTransforms = enemyQuery.ToComponentDataArray<LocalTransform>(Allocator.TempJob);
            // 获取敌人的半径数组
            var enemyCollisions = enemyQuery.ToComponentDataArray<CollisionComponent>(Allocator.TempJob);

            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();

            state.Dependency = new CollisionJob
            {
                EnemyEntities = enemyEntities,
                EnemyTransforms = enemyTransforms,
                EnemyCollisions = enemyCollisions, // 传入敌人半径
                Ecb = ecb,
                HealthLookup = SystemAPI.GetComponentLookup<HealthComponent>(false)
            }.ScheduleParallel(state.Dependency);

            enemyEntities.Dispose(state.Dependency);
            enemyTransforms.Dispose(state.Dependency);
            enemyCollisions.Dispose(state.Dependency);
        }
    }

    [BurstCompile]
    public partial struct CollisionJob : IJobEntity
    {
        [ReadOnly] public NativeArray<Entity> EnemyEntities;
        [ReadOnly] public NativeArray<LocalTransform> EnemyTransforms;
        [ReadOnly] public NativeArray<CollisionComponent> EnemyCollisions;
        public EntityCommandBuffer.ParallelWriter Ecb;
        public ComponentLookup<HealthComponent> HealthLookup;

        // 这里的 Execute 会自动筛选带有 CollisionComponent 的子弹
        public void Execute(Entity entity, [ChunkIndexInQuery] int chunkIndex, in LocalTransform bulletTransform, in BulletComponent bullet, in CollisionComponent bulletCollision)
        {
            for (int i = 0; i < EnemyTransforms.Length; i++)
            {
                // 动态计算：两个物体的碰撞半径之和
                float combinedRadius = bulletCollision.Radius + EnemyCollisions[i].Radius;
                float checkThresholdSq = combinedRadius * combinedRadius;

                float distSq = math.distancesq(bulletTransform.Position, EnemyTransforms[i].Position);

                if (distSq < checkThresholdSq)
                {
                    Entity enemyEntity = EnemyEntities[i];
                    if (HealthLookup.HasComponent(enemyEntity))
                    {
                        var health = HealthLookup[enemyEntity];
                        health.CurrentHealth -= bullet.Damage;
                        HealthLookup[enemyEntity] = health;
                    }

                    Ecb.DestroyEntity(chunkIndex, entity);
                    break;
                }
            }
        }
    }
}