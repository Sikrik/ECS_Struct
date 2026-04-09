using System.IO;
using Components;
using Data;
using Unity.Entities;
using UnityEngine;

namespace Authoring
{
    /// <summary>
    /// 敌人数据加载器：从 JSON 文件读取敌人配置并转换为 ECS 数据
    /// </summary>
    public class EnemyDataLoaderAuthoring : MonoBehaviour
    {
        /// <summary>
        /// JSON 配置文件名称（位于 StreamingAssets 目录下）
        /// </summary>
        public string JsonFileName = "EnemyData.json";

        /// <summary>
        /// 烘焙器类：在编辑器时将 JSON 配置数据烘焙到 ECS 实体中
        /// </summary>
        public class Baker : Baker<EnemyDataLoaderAuthoring>
        {
            /// <summary>
            /// 烘焙方法：读取 JSON 文件并将敌人配置数据存储到 DynamicBuffer
            /// </summary>
            /// <param name="authoring">包含配置的 MonoBehaviour 实例</param>
            public override void Bake(EnemyDataLoaderAuthoring authoring)
            {
                // 1. 获取（或创建）一个配置单例实体（无需变换组件）
                var entity = GetEntity(TransformUsageFlags.None);
            
                // 2. 构建 JSON 文件完整路径并读取内容
                // 文件应放置在 Assets/StreamingAssets 目录下
                string filePath = Path.Combine(Application.streamingAssetsPath, authoring.JsonFileName);
            
                // 检查文件是否存在，避免运行时错误
                if (!File.Exists(filePath))
                {
                    Debug.LogError($"找不到 JSON 配置文件: {filePath}");
                    return;
                }

                // 读取文件全部内容并反序列化为敌人数据列表
                string jsonContent = File.ReadAllText(filePath);
                EnemyListWrapper dataWrapper = JsonUtility.FromJson<EnemyListWrapper>(jsonContent);

                // 3. 将解析后的敌人配置数据存入 DynamicBuffer（动态缓冲区）
                var buffer = AddBuffer<EnemyConfigData>(entity);
                foreach (var item in dataWrapper.Enemies)
                {
                    buffer.Add(new EnemyConfigData
                    {
                        EnemyID = item.EnemyID,       // 敌人唯一标识
                        MaxHealth = item.MaxHealth,     // 最大生命值
                        MoveSpeed = item.MoveSpeed,     // 移动速度
                        AttackPower = item.AttackPower  // 攻击力
                    });
                }
            
                // 4. 添加管理器标签，方便系统通过 WithAll<BattleManagerTag>() 查询到此配置实体
                AddComponent<BattleManagerTag>(entity);
            }
        }
    }
}
