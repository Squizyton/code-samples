using Managers;
using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;
using Modifiers.ModifierTypes;

namespace Modifiers.Modifier_Effects.Player
{
    public class LongHook : BaseModifierPlayer
    {
        public LongHook(ModifierData data) : base(data)
        {
            Data = data;
        }

        public override void OnSpawn() { }
        public override void OnEnemyDeath() { }
        public override void OnTick()
        {
        }

        public override void OnHit()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHitSomething()
        {
            throw new System.NotImplementedException();
        }

        public override void OnPickup()
        {
            GameManager.instance.inventory.hookLengthDoubler++;
        }

        public override void OnDrop()
        {
            GameManager.instance.inventory.hookLengthDoubler--;
        }

    }
}