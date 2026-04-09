using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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

    public class EntityBaker : Baker<EntityAuthoring>
    {
        public override void Bake(EntityAuthoring authoring)
        {
            // 获取实体的动态变换标记
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            // 初始化位移组件
            AddComponent(entity, new MovementComponent
            {
                BaseSpeed = authoring.Speed,
                CurrentSpeed = authoring.Speed,
                Velocity = float3.zero
            });

            // 初始化生命值
            AddComponent(entity, new HealthComponent
            {
                CurrentHealth = authoring.MaxHealth,
                MaxHealth = authoring.MaxHealth
            });

            // 身份分配与特有组件注入
            if (authoring.IsPlayer)
            {
                AddComponent<PlayerTag>(entity);
                AddComponent(entity, new PlayerInputComponent { Movement = float2.zero });
            }
            else
            {
                AddComponent<EnemyTag>(entity);
            }
        }
    }
}