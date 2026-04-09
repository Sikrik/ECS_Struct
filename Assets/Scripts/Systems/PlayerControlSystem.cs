using Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    /// <summary>
    /// 玩家控制系统：负责输入响应、角色转向以及战斗开火行为。
    /// 继承自 SystemBase 以便在主线程捕获 UnityEngine.Input。
    /// 
    /// 该系统是游戏中唯一直接处理用户输入的 ECS 系统，包含以下核心功能：
    /// - 键盘输入处理（WASD/方向键移动）
    /// - 鼠标输入处理（左键射击）
    /// - 角色朝向控制（平滑转向移动方向）
    /// - 子弹生成与初始化
    /// 
    /// 由于需要访问 Unity 传统 Input API，因此使用 SystemBase 而非 ISystem
    /// </summary>
    public partial class PlayerControlSystem : SystemBase
    {
        /// <summary>
        /// 系统更新方法，每帧调用一次
        /// 处理玩家输入并更新玩家实体的状态
        /// </summary>
        protected override void OnUpdate()
        {
            // 1. 单例安全检查：寻找带有 PlayerTag 的核心玩家实体
            // 如果场景中不存在玩家实体，跳过本帧更新以避免错误
            if (!SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player)) return;
        
            // 2. 捕获每帧的硬件输入（键盘 WASD 或方向键）
            // GetAxisRaw 返回 -1、0、1 的离散值，无平滑过渡
            float moveX = Input.GetAxisRaw("Horizontal");  // A/D 或 左/右箭头
            float moveY = Input.GetAxisRaw("Vertical");    // W/S 或 上/下箭头
            float2 moveInput = new float2(moveX, moveY);
        
            // 归一化输入向量，防止斜向移动时速度过快
            // 例如：(1, 1) 的长度约为 1.414，归一化后变为 (0.707, 0.707)
            // 这样确保对角线移动速度与直线移动一致
            if (math.lengthsq(moveInput) > 1f)
            {
                moveInput = math.normalize(moveInput);
            }

            // 3. 获取玩家组件的读写权限
            // GetComponentRW 返回 ComponentRefRW，允许安全地读写组件数据
            var inputComp = SystemAPI.GetComponentRW<PlayerInputComponent>(player);
            var moveComp = SystemAPI.GetComponentRW<MovementComponent>(player);
            var transform = SystemAPI.GetComponentRW<LocalTransform>(player);

            // 4. 将输入同步到 PlayerInputComponent 组件，并转化为逻辑速度向量
            // Movement 存储二维输入向量，供其他系统参考
            // Velocity 存储三维速度方向，供 MovementSystem 执行实际移动
            inputComp.ValueRW.Movement = moveInput;
            moveComp.ValueRW.Velocity = new float3(moveInput.x, 0, moveInput.y);

            // 5. 角色平滑转向：当有有效输入时，让玩家面向移动方向
            // 使用阈值 0.01 过滤微小的输入抖动
            if (math.lengthsq(moveInput) > 0.01f)
            {
                // 将二维输入转换为三维方向向量（Y轴保持水平）
                float3 lookDir = new float3(moveInput.x, 0, moveInput.y);
                
                // 使用 LookRotationSafe 计算目标旋转四元数
                // 第一个参数：前方向量（看向的方向）
                // 第二个参数：上方向量（保持角色直立）
                // Safe 版本能处理非法输入，避免崩溃
                transform.ValueRW.Rotation = quaternion.LookRotationSafe(lookDir, math.up());
            }

            // 6. 射击逻辑处理器（使用 ECB 模式）
            // 检测鼠标左键点击，并确保已加载子弹预制体配置
            if (Input.GetMouseButtonDown(0) && SystemAPI.TryGetSingleton<BulletPrefabConfig>(out var config))
            {
                var ecbSystem = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
                var ecb = ecbSystem.CreateCommandBuffer(World.Unmanaged);

                // 1. 实例化子弹
                Entity bullet = ecb.Instantiate(config.BulletPrefab);

                // 2. 关键点：直接获取预制体本身烘焙好的缩放值
                // config.BulletPrefab 本身就是一个实体，它带有你在编辑器里设置的所有烘焙数据
                var prefabTransform = SystemAPI.GetComponent<LocalTransform>(config.BulletPrefab);
                float prefabScale = prefabTransform.Scale; 

                // 3. 将位置、旋转应用到子弹，同时保留预制体的缩放
                ecb.SetComponent(bullet, new LocalTransform
                {
                    Position = transform.ValueRO.Position,
                    Rotation = transform.ValueRO.Rotation,
                    Scale = prefabScale // 动态读取，不再硬编码 0.2f
                });
            
                // 设置子弹的初速度和移动参数
                // math.forward 根据旋转四元数计算正前方单位向量
                ecb.SetComponent(bullet, new MovementComponent {
                    Velocity = math.forward(transform.ValueRO.Rotation),  // 沿玩家朝向飞行
                    BaseSpeed = 20f,       // 基础速度：20米/秒
                    CurrentSpeed = 20f     // 当前速度：20米/秒（可受游戏逻辑影响）
                });

                // 初始化子弹的特有标记与属性组件
                // BulletTag：用于筛选和识别子弹实体
                ecb.AddComponent(bullet, new BulletTag());
                
                // BulletComponent：存储子弹的伤害和寿命信息
                ecb.AddComponent(bullet, new BulletComponent {
                    Damage = 10f,           // 伤害值：10点
                    MaxLifeTime = 3f,       // 最大存活时间：3秒（超时自动销毁）
                    CurrentLifeTime = 0f    // 当前存活时间：从0开始计时
                });
            }
        }
    }
}
