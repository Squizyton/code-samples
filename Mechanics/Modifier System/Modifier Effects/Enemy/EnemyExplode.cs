using System.Collections;
using System.Collections.Generic;
using Enemies;
using Modifiers.Data;
using Modifiers.ModifierTypes;
using UnityEngine;

public class EnemyExplode : BaseModifierEnemy
{
    public EnemyExplode(ModifierData data, BaseEnemy enemy) : base(data, enemy)
    {
    }

    public override void OnEnemyDeath()
    {
        //Explosion Effect
        Debug.Log("Boom");
    }

    public override void OnTick()
    {
        throw new System.NotImplementedException();
    }

    public override void OnHit()
    {
        throw new System.NotImplementedException();
    }
}
