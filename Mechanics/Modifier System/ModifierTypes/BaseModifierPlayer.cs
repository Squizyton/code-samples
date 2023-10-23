using Modifiers.Data;

namespace Modifiers.ModifierTypes
{
    [System.Serializable]
    public abstract class BaseModifierPlayer : BaseModifier
    {
        protected BaseModifierPlayer(ModifierData data) : base(data)
        {
        }

        public abstract void OnHitSomething();
        
        public abstract void OnPickup(); //Called when this modifier is gained
        public abstract void OnDrop(); //Called when this modifier is lost
    }
}