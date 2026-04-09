using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    /// <summary>
    /// 敌人配置烘焙器：用于存储敌人 Prefab 引用，供生成系统使用
    /// </summary>
    public class EnemyConfigAuthoring : MonoBehaviour
    {
        /// <summary>
        /// 敌人预制体引用（在 Inspector 中拖入敌人的 Prefab）
        /// </summary>
        public GameObject EnemyPrefab;

        /// <summary>
        /// 烘焙器类：将 GameObject Prefab 引用转换为 ECS 实体引用
        /// </summary>
        public class Baker : Baker<EnemyConfigAuthoring>
        {
            /// <summary>
            /// 烘焙方法：将敌人 Prefab 转换为 Entity 引用并存储到组件中
            /// </summary>
            /// <param name="authoring">包含配置的 MonoBehaviour 实例</param>
            public override void Bake(EnemyConfigAuthoring authoring)
            {
                // 获取配置实体（无需变换组件）
                var entity = GetEntity(TransformUsageFlags.None);
                
                // 添加敌人预制体配置组件
                AddComponent(entity, new EnemyPrefabConfig
                {
                    // 将 GameObject Prefab 转换为 Entity Prefab（支持动态变换）
                    EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}