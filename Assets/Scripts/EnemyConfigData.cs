using Unity.Entities;
using Unity.Mathematics;

// 存储从 CSV 导入的原始数据
public struct EnemyConfigData : IBufferElementData 
{
    public int EnemyID;
    public float MaxHealth;
    public float MoveSpeed;
    public float AttackPower;
}

// 标识当前正在运行的战斗单例
public struct BattleManagerTag : IComponentData { }