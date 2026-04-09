using Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Authoring
{
    /// <summary>
    /// 实体烘焙器：挂载于 GameObject Prefab，用于生成 ECS 实体。
    /// </summary>
    public class EntityAuthoring : MonoBehaviour
    {
        [Header("移动属性")]
        public float Speed = 5f; 
    
        [Header("战斗属性")]
        public float MaxHealth = 100f; 
    
        [Header("角色身份")]
        public bool IsPlayer;
        
        [Header("碰撞属性")]
        public float CollisionRadius = 0.5f;

        /// <summary>
        /// 烘焙器类：在编辑器中将 MonoBehaviour 转换为 ECS 实体和组件
        /// </summary>
        public class EntityBaker : Baker<EntityAuthoring>
        {
            /// <summary>
            /// 烘焙方法：将 Authoring 配置转换为 ECS 组件
            /// </summary>
            /// <param name="authoring">包含配置的 MonoBehaviour 实例</param>
            public override void Bake(EntityAuthoring authoring)
            {
                // 获取实体的动态变换标记（支持运行时位置/旋转/缩放变化）
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                // 初始化位移组件：设置基础速度和当前速度
                AddComponent(entity, new MovementComponent
                {
                    BaseSpeed = authoring.Speed,
                    CurrentSpeed = authoring.Speed,
                    Velocity = float3.zero
                });

                // 初始化生命值组件：当前生命值等于最大生命值
                AddComponent(entity, new HealthComponent
                {
                    CurrentHealth = authoring.MaxHealth,
                    MaxHealth = authoring.MaxHealth
                });

                // 修改身份分配逻辑
                if (authoring.IsPlayer)
                {
                    AddComponent<PlayerTag>(entity);
                    AddComponent(entity, new PlayerInputComponent { Movement = float2.zero });
                }
                // 这里不要直接用 else，而是可以根据物体名、层级或其他标记判断
                // 方案 A：如果该物体名字包含 "Enemy"，才加 EnemyTag
                else if (authoring.gameObject.name.Contains("Enemy"))
                {
                    AddComponent<EnemyTag>(entity);
                }
            }
        }
    }
}
