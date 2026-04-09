using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 玩家控制系统：负责输入响应、角色转向以及战斗开火行为。
/// 继承自 SystemBase 以便在主线程捕获 UnityEngine.Input。
/// </summary>
public partial class PlayerControlSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // 1. 单例安全检查：寻找核心玩家实体
        if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player)) return;
        
        // 2. 捕获每帧的硬件输入
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        float2 moveInput = new float2(moveX, moveY);
        
        // 归一化输入向量，防止斜向移动加速
        if (math.lengthsq(moveInput) > 1f)
        {
            moveInput = math.normalize(moveInput);
        }

        // 3. 获取组件的读写权限
        var inputComp = SystemAPI.GetComponentRW<PlayerInputComponent>(player);
        var moveComp = SystemAPI.GetComponentRW<MovementComponent>(player);
        var transform = SystemAPI.GetComponentRW<LocalTransform>(player);

        // 4. 将输入同步到组件，并转化位逻辑速度
        inputComp.ValueRW.Movement = moveInput;
        moveComp.ValueRW.Velocity = new float3(moveInput.x, 0, moveInput.y);

        // 5. 角色平滑转向：面向移动方向
        if (math.lengthsq(moveInput) > 0.01f)
        {
            float3 lookDir = new float3(moveInput.x, 0, moveInput.y);
            transform.ValueRW.Rotation = quaternion.LookRotationSafe(lookDir, math.up());
        }

        // 6. 射击逻辑处理器 (使用 ECB 模式)
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

            // 初始化子弹特有标记与属性
            ecb.AddComponent(bullet, new BulletTag());
            ecb.AddComponent(bullet, new BulletComponent {
                Damage = 10f,
                MaxLifeTime = 3f,
                CurrentLifeTime = 0f
            });
        }
    }
}