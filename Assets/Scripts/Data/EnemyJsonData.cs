using System;
using System.Collections.Generic;

[Serializable]
public class EnemyJsonData
{
    public int EnemyID;
    public float MaxHealth;
    public float MoveSpeed;
    public float AttackPower;
}

[Serializable]
public class EnemyListWrapper
{
    public List<EnemyJsonData> Enemies;
}