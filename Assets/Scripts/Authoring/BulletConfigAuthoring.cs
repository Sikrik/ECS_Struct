using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    /// <summary>
    /// 子弹预制体配置组件
    /// 用于在 ECS 世界中存储经过烘焙后的子弹实体模板
    /// </summary>
    public class BulletConfigAuthoring : MonoBehaviour
    {
        /// <summary>
        /// 子弹预制体引用（在 Inspector 中拖入子弹的 Prefab）
        /// </summary>
        public GameObject BulletPrefab;

        /// <summary>
        /// 烘焙器类：将子弹 GameObject Prefab 引用转换为 ECS 实体引用
        /// </summary>
        public class Baker : Baker<BulletConfigAuthoring>
        {
            /// <summary>
            /// 烘焙方法：将子弹 Prefab 转换为 Entity 引用并存储到组件中
            /// </summary>
            /// <param name="authoring">包含配置的 MonoBehaviour 实例</param>
            public override void Bake(BulletConfigAuthoring authoring)
            {
                // 获取配置实体（无需变换组件，仅作为配置容器）
                var entity = GetEntity(TransformUsageFlags.None);
                
                // 添加子弹预制体配置组件，存储转换后的 Entity Prefab
                AddComponent(entity, new BulletPrefabConfig
                {
                    // 将 GameObject Prefab 转换为 Entity Prefab（支持动态变换）
                    BulletPrefab = GetEntity(authoring.BulletPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}