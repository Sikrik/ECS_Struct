using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// 相机跟随系统
    /// 负责让主相机平滑跟随玩家实体移动
    /// 在 PresentationSystemGroup 中更新，确保在渲染阶段执行以获得最流畅的视觉效果
    /// 使用 SystemBase 而非 ISystem，因为需要访问 Unity 传统 API（Camera.main）
    /// </summary>
    [UpdateInGroup(typeof(PresentationSystemGroup))] // 在渲染组更新，确保平滑
    public partial class CameraFollowSystem : SystemBase
    {
        /// <summary>
        /// 相机相对于玩家的偏移量
        /// 默认设置为：X轴无偏移，Y轴上方15米，Z轴后方8米
        /// 可根据游戏视角需求调整此值
        /// </summary>
        private float3 _offset = new float3(0, 15f, -8f);

        /// <summary>
        /// 系统更新方法，每帧调用一次
        /// 负责计算目标位置并平滑移动相机
        /// </summary>
        protected override void OnUpdate()
        {
            // 1. 获取带有 PlayerTag 组件的玩家实体
            // 如果场景中不存在玩家实体，则直接返回，避免后续错误
            if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player)) return;

            // 2. 获取玩家当前的变换组件，并计算相机的目标位置
            // 目标位置 = 玩家位置 + 偏移量
            var playerTransform = SystemAPI.GetComponent<LocalTransform>(player);
            float3 targetPos = playerTransform.Position + _offset;

            // 3. 获取主相机并更新其位置和朝向
            // 直接操作 Camera.main 是最简单的方法，适用于单相机场景
            if (Camera.main != null)
            {
                // 使用线性插值（Lerp）实现平滑跟随效果
                // lerp 参数中的 10f 控制跟随速度，数值越大跟随越紧密，越小越有延迟感
                // 乘以 DeltaTime 确保在不同帧率下跟随速度一致
                float3 currentPos = Camera.main.transform.position;
                Camera.main.transform.position = math.lerp(currentPos, targetPos, SystemAPI.Time.DeltaTime * 10f);
            
                // 让相机始终朝向玩家位置，确保玩家始终在视野中心
                Camera.main.transform.LookAt(playerTransform.Position);
            }
        }
    }
}