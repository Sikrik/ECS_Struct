using Unity.Entities;
using Unity.Burst;

[BurstCompile]
public partial struct EnemySpawnSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        // 仅在需要初始化时运行一次（逻辑示例）
        if (!SystemAPI.TryGetSingletonEntity<BattleManagerTag>(out Entity managerEntity)) return;

        // 获取包含所有 JSON 数据的 Buffer
        DynamicBuffer<EnemyConfigData> configBuffer = SystemAPI.GetBuffer<EnemyConfigData>(managerEntity);

        foreach (var config in configBuffer)
        {
            // 这里可以根据 config.EnemyID 配合 Prefab 进行实例化逻辑
            // UnityEngine.Debug.Log($"加载了怪物 ID: {config.EnemyID}, 血量: {config.MaxHealth}");
        }
        
        // 示例：运行一次后禁用系统
        state.Enabled = false;
    }
}