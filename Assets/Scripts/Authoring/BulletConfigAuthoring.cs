using Unity.Entities;
using UnityEngine;

using Unity.Entities;

/// <summary>
/// 子弹预制体配置组件
/// 用于在 ECS 世界中存储经过烘焙后的子弹实体模板
/// </summary>
// 烘焙器 (挂在场景里的一个管理类上)
public class BulletConfigAuthoring : MonoBehaviour
{
    public GameObject BulletPrefab;

    public class Baker : Baker<BulletConfigAuthoring>
    {
        public override void Bake(BulletConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new BulletPrefabConfig
            {
                // 将 GameObject Prefab 转换为 Entity Prefab
                BulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}