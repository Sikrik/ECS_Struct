using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

[UpdateInGroup(typeof(PresentationSystemGroup))] // 在渲染组更新，确保平滑
public partial class CameraFollowSystem : SystemBase
{
    // 定义相机的偏移量（例如在玩家上方 10 米，后方 5 米）
    private float3 _offset = new float3(0, 15f, -8f);

    protected override void OnUpdate()
    {
        // 1. 获取玩家实体
        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player)) return;

        // 2. 获取玩家当前位置
        var playerTransform = SystemAPI.GetComponent<LocalTransform>(player);
        float3 targetPos = playerTransform.Position + _offset;

        // 3. 获取主相机并更新（直接操作 Camera.main 是最快的方法）
        if (Camera.main != null)
        {
            // 平滑跟随（可选）
            float3 currentPos = Camera.main.transform.position;
            Camera.main.transform.position = math.lerp(currentPos, targetPos, SystemAPI.Time.DeltaTime * 10f);
            
            // 始终看向玩家位置
            Camera.main.transform.LookAt(playerTransform.Position);
        }
    }
}