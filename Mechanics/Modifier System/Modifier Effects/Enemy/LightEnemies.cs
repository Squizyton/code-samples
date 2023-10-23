using System.Collections;
using System.Collections.Generic;
using Enemies;
using Managers;
using Modifiers.Data;
using Modifiers.ModifierTypes;
using UnityEngine;

public class LightEnemies : BaseModifierEnemy
{
    private BaseEnemy enemy;
    private ModifierData data;
    
    public LightEnemies(ModifierData data, BaseEnemy enemy) : base(data, enemy)
    {
        this.enemy = enemy;
        this.data = data;
    }

    public override void OnEnemyDeath()
    {
    }

    public override void OnTick()
    {
        
    }

    public override void OnHit()
    {
        Debug.Log("LAUNCHING ENEMY");
        // Launch Enemy
        enemy.TryGetComponent(out Rigidbody rb);
        Vector3 direction =  GameManager.instance.player.transform.position - enemy.transform.position;
        direction.y = 3f;
        rb.AddForce(direction, ForceMode.Impulse);
    }
}
