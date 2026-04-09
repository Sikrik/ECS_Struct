using Unity.Entities;
using UnityEngine;

public class EnemyConfigAuthoring : MonoBehaviour
{
    public GameObject EnemyPrefab; // 在 Inspector 中拖入敌人的 Prefab

    public class Baker : Baker<EnemyConfigAuthoring>
    {
        public override void Bake(EnemyConfigAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new EnemyPrefabConfig
            {
                // 将 GameObject 转换为 Entity Prefab
                EnemyPrefab = GetEntity(authoring.EnemyPrefab, TransformUsageFlags.Dynamic)
            });
        }
    }
}