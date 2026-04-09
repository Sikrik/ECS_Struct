using Unity.Entities;

public struct CameraTargetTag : IComponentData 
{
    // 注意：在 IComponentData 中不能直接放 Transform (它是 Class)
    // 但在 SystemBase 中我们可以通过 Managed 方式获取它
}
public struct PlayerTag : IComponentData { } // 标记唯一玩家
public struct EnemyTag  : IComponentData { } // 标记敌人单位
public struct BulletTag : IComponentData { } // 标记子弹实体