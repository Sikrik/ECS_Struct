using System;
using System.Collections.Generic;

namespace Data
{
    /// <summary>
    /// 敌人JSON数据结构
    /// 用于序列化和反序列化JSON格式的敌人配置数据
    /// 通常用于从 StreamingAssets 文件夹加载敌人配置
    /// </summary>
    [Serializable]
    public class EnemyJsonData
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

    /// <summary>
    /// 敌人列表包装类
    /// 用于处理包含多个敌人配置的JSON数组
    /// 作为JSON反序列化的根对象使用
    /// </summary>
    [Serializable]
    public class EnemyListWrapper
    {
        /// <summary>
        /// 敌人配置数据列表
        /// 包含所有从JSON文件加载的敌人配置信息
        /// </summary>
        public List<EnemyJsonData> Enemies;
    }
}