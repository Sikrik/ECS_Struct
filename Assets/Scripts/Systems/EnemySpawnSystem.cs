using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public partial struct EnemySpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 1. 获取包含 JSON 数据的 BattleManager 实体
        if (!SystemAPI.TryGetSingletonEntity<BattleManagerTag>(out Entity managerEntity)) return;
        
        // 2. 获取敌人预制体配置
        if (!SystemAPI.TryGetSingleton<EnemyPrefabConfig>(out var prefabConfig)) return;

        // 3. 获取数据 Buffer 和 ECB 指令缓冲区
        DynamicBuffer<EnemyConfigData> configBuffer = SystemAPI.GetBuffer<EnemyConfigData>(managerEntity);
        var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
            .CreateCommandBuffer(state.WorldUnmanaged);

        // 4. 遍历 JSON 配置并执行生成
        foreach (var config in configBuffer)
        {
            // 关键：像生成子弹一样实例化敌人实体
            Entity enemy = ecb.Instantiate(prefabConfig.EnemyPrefab);

            // 设置初始位置（根据 EnemyID 简单偏移）
            float3 spawnPos = new float3(config.EnemyID % 10 * 2f, 0, config.EnemyID / 10 * 2f);
            ecb.SetComponent(enemy, LocalTransform.FromPosition(spawnPos));

            // 将来自 JSON 的属性覆盖到实体的组件中
            ecb.SetComponent(enemy, new MovementComponent 
            { 
                BaseSpeed = config.MoveSpeed, 
                CurrentSpeed = config.MoveSpeed,
                Velocity = float3.zero 
            });

            ecb.SetComponent(enemy, new HealthComponent 
            { 
                MaxHealth = config.MaxHealth, 
                CurrentHealth = config.MaxHealth 
            });
        }
        
        // 5. 生成完成后禁用系统，防止重复生成
        state.Enabled = false;
    }
}