using Unity.Entities;

namespace Components
{
    /// <summary>
    /// 子弹预制体配置：存储子弹实体的模板引用
    /// </summary>
    public struct BulletPrefabConfig : IComponentData
    {
        public Entity BulletPrefab; // 子弹实体模板句柄
    }

    /// <summary>
    /// 敌人预制体配置：存储敌人实体的模板引用
    /// </summary>
    public struct EnemyPrefabConfig : IComponentData
    {
        public Entity EnemyPrefab; // 敌人实体模板
    }
}