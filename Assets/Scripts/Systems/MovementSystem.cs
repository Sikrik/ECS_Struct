using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Systems
{
    /// <summary>
    /// 基础位移系统：这是逻辑层与物理表现层的最后同步环节。
    /// 职责：根据结算后的 Velocity 更新实体的渲染坐标。
    /// 
    /// 该系统是 ECS 架构中的核心移动执行者，负责将所有具有 MovementComponent 的实体
    /// 按照其速度向量进行实际位置更新。它与追踪系统（EnemyTrackingSystem）配合工作：
    /// - 追踪系统：计算移动方向（Velocity）
    /// - 移动系统：执行实际位移（Position 更新）
    /// 
    /// 使用 Burst 编译和并行处理，可高效更新成千上万个实体
    /// </summary>
    [BurstCompile]
    public partial struct MovementSystem : ISystem
    {
        /// <summary>
        /// 系统更新方法，每帧调用一次
        /// 捕获时间增量并调度并行 Job 执行所有实体的位置更新
        /// </summary>
        /// <param name="state">系统状态引用，用于访问时间和依赖管理</param>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // 1. 捕获当前帧的时间增量（Delta Time）
            // DeltaTime 表示上一帧到当前帧的时间间隔（秒）
            // 使用 DeltaTime 确保移动速度与帧率无关，在不同性能设备上保持一致
            float deltaTime = SystemAPI.Time.DeltaTime;

            // 2. 调度并行作业：计算所有带 MovementComponent 实体的坐标更新
            // ScheduleParallel 自动将实体分配到多个线程并行处理
            // 通过 state.Dependency 维护任务依赖链，确保在之前的系统完成后执行
            state.Dependency = new MoveJob 
            { 
                DeltaTime = deltaTime 
            }.ScheduleParallel(state.Dependency);
        }
    }

    /// <summary>
    /// 高性能并行移动作业：完全脱离主线程运行。
    /// 使用 Burst 编译优化，针对 SIMD 指令集进行加速。
    /// 该 Job 会自动筛选所有同时拥有 LocalTransform 和 MovementComponent 的实体
    /// </summary>
    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        /// <summary>
        /// 时间增量，用于计算本帧的移动距离
        /// 由主系统在 OnUpdate 中传入
        /// </summary>
        public float DeltaTime;

        /// <summary>
        /// 对每个可移动实体执行位置更新
        /// 使用经典的运动学公式：新位置 = 旧位置 + (方向 × 速度 × 时间)
        /// 
        /// 计算流程：
        /// 1. movement.Velocity：归一化的移动方向向量（由追踪系统等设置）
        /// 2. movement.CurrentSpeed：当前移动速率（可受 buff/debuff 影响）
        /// 3. DeltaTime：帧时间间隔
        /// 4. 三者相乘得到本帧的实际位移量
        /// </summary>
        /// <param name="transform">实体变换组件引用，用于读写位置信息</param>
        /// <param name="movement">实体移动组件，提供方向和速度数据（只读）</param>
        public void Execute(ref LocalTransform transform, in MovementComponent movement)
        {
            // 核心位移计算公式：位置 = 位置 + (方向向量 × 实时速率 × 时间)
            // 
            // 分解说明：
            // - movement.Velocity：归一化方向向量（长度为1）
            // - movement.CurrentSpeed：标量速度值（单位：米/秒）
            // - DeltaTime：时间增量（单位：秒）
            // - 最终结果：本帧沿指定方向移动的距离（单位：米）
            //
            // 示例：如果 CurrentSpeed=5 m/s，DeltaTime=0.016s（60fps）
            // 则本帧移动距离 = 1 × 5 × 0.016 = 0.08 米
            transform.Position += movement.Velocity * movement.CurrentSpeed * DeltaTime;
        }
    }
}
