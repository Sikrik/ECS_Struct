using Unity.Entities;

// 存储从 CSV 导入的原始数据
namespace Data
{
    /// <summary>
    /// 敌人配置数据结构
    /// 用于存储从 CSV 文件导入的敌人基础配置信息
    /// 该结构体实现了 IBufferElementData 接口，可以存储在 DynamicBuffer 中
    /// 适用于需要管理多个敌人配置数据的场景
    /// </summary>
    public struct EnemyConfigData : IBufferElementData 
    {
        /// <summary>
        /// 敌人唯一标识ID
        /// 用于区分不同类型的敌人
        /// </summary>
        public int EnemyID;
    
        /// <summary>
        /// 敌人最大生命值
        /// 决定敌人的生存能力
        /// </summary>
        public float MaxHealth;
    
        /// <summary>
        /// 敌人移动速度
        /// 控制敌人在场景中的移动快慢
        /// </summary>
        public float MoveSpeed;
    
        /// <summary>
        /// 敌人攻击力
        /// 决定敌人对玩家造成的伤害值
        /// </summary>
        public float AttackPower;
    }
}