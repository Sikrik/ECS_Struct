using Components;
using Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Systems
{
    /// <summary>
    /// 敌人生成系统
    /// 负责从 JSON 配置数据中读取敌人信息，并在游戏开始时批量生成敌人实体
    /// 该系统仅执行一次，生成完成后自动禁用以避免重复生成
    /// 使用 Burst 编译以提高批量生成的性能
    /// </summary>
    [BurstCompile]
    public partial struct EnemySpawnSystem : ISystem
    {
        /// <summary>
        /// 系统更新方法，在游戏开始时执行一次
        /// 读取 BattleManager 中的敌人配置数据，实例化所有敌人实体
        /// </summary>
        /// <param name="state">系统状态引用，用于访问 ECS 世界和相关组件</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 获取带有 BattleManagerTag 的管理器实体
            // 该实体存储了从 JSON 加载的敌人配置数据
            // 如果管理器不存在，说明数据尚未加载完成，直接返回
            if (!SystemAPI.TryGetSingletonEntity<BattleManagerTag>(out Entity managerEntity)) return;
        
            // 2. 获取敌人预制体配置单例
            // EnemyPrefabConfig 包含了敌人预制体的引用，用于实例化
            if (!SystemAPI.TryGetSingleton<EnemyPrefabConfig>(out var prefabConfig)) return;

            // 3. 获取敌人配置数据缓冲区和实体命令缓冲区（ECB）
            // DynamicBuffer 存储了所有敌人的配置信息（从 JSON 解析而来）
            DynamicBuffer<EnemyConfigData> configBuffer = SystemAPI.GetBuffer<EnemyConfigData>(managerEntity);
            
            // 使用 BeginSimulationEntityCommandBufferSystem 确保在帧开始时执行生成操作
            // ECB 允许在当前帧安全地批量创建和修改实体
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            // 4. 遍历所有敌人配置并执行实例化
            foreach (var config in configBuffer)
            {
                // 关键步骤：通过 ECB 实例化敌人实体
                // 类似于子弹生成，使用预制体快速创建具有完整组件结构的实体
                Entity enemy = ecb.Instantiate(prefabConfig.EnemyPrefab);

                // 根据 EnemyID 计算生成位置，形成网格状排列
                // X 轴：EnemyID 对 10 取余后乘以 2，实现横向排列
                // Z 轴：EnemyID 除以 10 后乘以 2，实现纵向排列
                // 例如：ID=0 -> (0,0,0), ID=1 -> (2,0,0), ID=10 -> (0,0,2)
                float3 spawnPos = new float3(config.EnemyID % 10 * 2f, 0, config.EnemyID / 10 * 2f);
                ecb.SetComponent(enemy, LocalTransform.FromPosition(spawnPos));

                // 将来自 JSON 配置的属性值应用到敌人的 MovementComponent 组件
                // BaseSpeed 和 CurrentSpeed 都设置为配置中的移动速度
                // Velocity 初始化为零向量，敌人初始静止
                ecb.SetComponent(enemy, new MovementComponent 
                { 
                    BaseSpeed = config.MoveSpeed, 
                    CurrentSpeed = config.MoveSpeed,
                    Velocity = float3.zero 
                });

                // 将来自 JSON 配置的生命值应用到敌人的 HealthComponent 组件
                // MaxHealth 和 CurrentHealth 都设置为配置中的最大生命值
                // 确保敌人生成时处于满血状态
                ecb.SetComponent(enemy, new HealthComponent 
                { 
                    MaxHealth = config.MaxHealth, 
                    CurrentHealth = config.MaxHealth 
                });
            }
        
            // 5. 所有敌人生成完成后，禁用当前系统
            // 这是一次性初始化系统，防止在后续帧中重复生成敌人
            // 如需重新生成，需要手动启用此系统或重新加载场景
            state.Enabled = false;
        }
    }
}
