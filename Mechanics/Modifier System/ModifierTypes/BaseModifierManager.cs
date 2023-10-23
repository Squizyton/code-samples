using Modifiers.Data;
using UnityEngine;

namespace Modifiers.ModifierTypes
{
    public class BaseModifierManager : BaseModifier
    {
        
        public BaseModifierManager(ModifierData data) : base(data)
        {
        }
        public override void OnSpawn()
        {
            throw new System.NotImplementedException();
        }

        public override void OnEnemyDeath()
        {
            throw new System.NotImplementedException();
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
}
