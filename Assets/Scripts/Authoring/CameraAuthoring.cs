using Components;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    /// <summary>
    /// 相机烘焙器：为相机 GameObject 添加 ECS 标签，用于相机跟随系统识别目标
    /// </summary>
    public class CameraAuthoring : MonoBehaviour
    {
        /// <summary>
        /// 烘焙器类：将相机 GameObject 转换为带有相机标签的 ECS 实体
        /// </summary>
        public class Baker : Baker<CameraAuthoring>
        {
            /// <summary>
            /// 烘焙方法：为相机实体添加相机目标标签
            /// </summary>
            /// <param name="authoring">包含配置的 MonoBehaviour 实例</param>
            public override void Bake(CameraAuthoring authoring)
            {
                // 获取实体（无需变换组件，仅作为标记实体）
                var entity = GetEntity(TransformUsageFlags.None);
                
                // 添加相机目标标签，使该实体可被相机跟随系统查询到
                AddComponent<CameraTargetTag>(entity);
            }
        }
    }
}