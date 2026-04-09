using UnityEngine;
using Unity.Entities;
using System.IO;

public class EnemyDataLoaderAuthoring : MonoBehaviour
{
    public string JsonFileName = "EnemyData.json";

    public class Baker : Baker<EnemyDataLoaderAuthoring>
    {
        public override void Bake(EnemyDataLoaderAuthoring authoring)
        {
            // 1. 获取（或创建）一个配置单例实体
            var entity = GetEntity(TransformUsageFlags.None);
            
            // 2. 读取 JSON 文件内容
            // 路径通常在 StreamingAssets 或 Resources
            string filePath = Path.Combine(Application.streamingAssetsPath, authoring.JsonFileName);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"找不到 JSON 配置文件: {filePath}");
                return;
            }

            string jsonContent = File.ReadAllText(filePath);
            EnemyListWrapper dataWrapper = JsonUtility.FromJson<EnemyListWrapper>(jsonContent);

            // 3. 将数据存入 DynamicBuffer
            var buffer = AddBuffer<EnemyConfigData>(entity);
            foreach (var item in dataWrapper.Enemies)
            {
                buffer.Add(new EnemyConfigData
                {
                    EnemyID = item.EnemyID,
                    MaxHealth = item.MaxHealth,
                    MoveSpeed = item.MoveSpeed,
                    AttackPower = item.AttackPower
                });
            }
            
            // 4. 给该实体添加标记，方便系统查询
            AddComponent<BattleManagerTag>(entity);
        }
    }
}