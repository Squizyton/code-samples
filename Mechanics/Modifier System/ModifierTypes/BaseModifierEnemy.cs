using Enemies;
using Modifiers.Data;

namespace Modifiers.ModifierTypes
{
    [System.Serializable]
    public abstract class BaseModifierEnemy : BaseModifier
    {
        private BaseEnemy _enemy;
        protected BaseModifierEnemy(ModifierData data, BaseEnemy enemy) : base(data)
        {
            _enemy = enemy;
        }

        public override void OnSpawn()
        {
            
        }
    }
}