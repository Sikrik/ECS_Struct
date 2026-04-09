using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 玩家控制系统：负责输入响应、角色转向以及战斗开火行为。
/// </summary>
public partial class PlayerControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 1. 单例安全检查：寻找核心玩家
        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player)) return;
        
        // 2. 数据获取 (RefRW 获取读写权限, RefRO 获取只读权限)
        var input = SystemAPI.GetComponent<PlayerInputComponent>(player);
        var moveComp = SystemAPI.GetComponentRW<MovementComponent>(player);
        var transform = SystemAPI.GetComponentRW<LocalTransform>(player);

        // 3. 移动意图转化：将输入映射到 3D 逻辑速度
        moveComp.ValueRW.Velocity = new float3(input.Movement.x, 0, input.Movement.y);

        // 4. 角色平滑转向：面向移动方向
        if (math.lengthsq(input.Movement) > 0.01f)
        {
            float3 lookDir = new float3(input.Movement.x, 0, input.Movement.y);
            transform.ValueRW.Rotation = quaternion.LookRotationSafe(lookDir, math.up());
        }

        // 5. 射击逻辑处理器 (ECB 模式)
        if (Input.GetMouseButtonDown(0) && SystemAPI.TryGetSingleton<BulletPrefabConfig>(out var config))
        {
            // 通过 ECB 系统获取本帧末尾执行的指令缓冲区，解决并行化冲突
            var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSystem.CreateCommandBuffer(World.Unmanaged);

            // 执行子弹实例化
            Entity bullet = ecb.Instantiate(config.BulletPrefab);
            
            // 同步子弹发射位置与旋转
            ecb.SetComponent(bullet, LocalTransform.FromPositionRotation(
                transform.ValueRO.Position, 
                transform.ValueRO.Rotation));
            
            // 设置子弹初速度（沿发射者正前方）
            ecb.SetComponent(bullet, new MovementComponent {
                Velocity = math.forward(transform.ValueRO.Rotation),
                BaseSpeed = 20f,
                CurrentSpeed = 20f
            });

            // 初始化子弹特有组件与数据
            ecb.AddComponent(bullet, new BulletTag());
            ecb.AddComponent(bullet, new BulletComponent {
                Damage = 10f,
                MaxLifeTime = 3f,
                CurrentLifeTime = 0f
            });
        }
    }
}