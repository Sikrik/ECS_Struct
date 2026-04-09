using Unity.Entities;

public struct BulletPrefabConfig : IComponentData
{
    public Entity BulletPrefab; // 子弹实体模板句柄
}
public struct EnemyPrefabConfig : IComponentData
{
    public Entity EnemyPrefab; // 敌人实体模板
}