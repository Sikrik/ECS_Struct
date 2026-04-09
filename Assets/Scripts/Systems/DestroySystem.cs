using Components;
using Unity.Burst;
using Unity.Entities;

namespace Systems
{
    // 放在 LateSimulationGroup 确保所有逻辑判定都已结束
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [BurstCompile]
    public partial struct DestroySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 使用同步 ECB 确保在本帧彻底清理
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);

            // 寻找所有带标记的实体并销毁
            foreach (var (tag, entity) in SystemAPI.Query<PendingDestroyTag>().WithEntityAccess())
            {
                ecb.DestroyEntity(entity);
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}