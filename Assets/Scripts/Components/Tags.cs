using Unity.Entities;

namespace Components
{
    /// <summary>
    /// 相机目标标签：标记相机应该跟随的实体
    /// 注意：在 IComponentData 中不能直接放 Transform (它是 Class)
    /// 但在 SystemBase 中我们可以通过 Managed 方式获取它
    /// </summary>
    public struct CameraTargetTag : IComponentData 
    {
    }

    /// <summary>
    /// 玩家标签：标记唯一玩家实体
    /// </summary>
    public struct PlayerTag : IComponentData { }

    /// <summary>
    /// 敌人标签：标记敌人单位实体
    /// </summary>
    public struct EnemyTag : IComponentData { }

    /// <summary>
    /// 子弹标签：标记子弹实体
    /// </summary>
    public struct BulletTag : IComponentData { }

    /// <summary>
    /// 战斗管理器标签：标识当前正在运行的战斗单例实体
    /// </summary>
    public struct BattleManagerTag : IComponentData { }
}