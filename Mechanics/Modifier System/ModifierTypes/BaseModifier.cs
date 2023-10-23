using Modifiers.Data;

namespace Modifiers.ModifierTypes
{
    [System.Serializable]
    public abstract class BaseModifier
    {
        protected ModifierData Data;


        public BaseModifier(ModifierData data)
        {
            Data = data;
        }

        /// <summary>
        /// Should Be called when something spawns
        /// </summary>
        public abstract void OnSpawn();
        /// <summary>
        /// Called when something dies
        /// </summary>
        public abstract void OnEnemyDeath();
        /// <summary>
        /// Something that happens every update
        /// </summary>
        public abstract void OnTick();
        public abstract void OnHit();
    }
}