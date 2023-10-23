using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;
using Modifiers.ModifierTypes;

namespace Modifiers.Modifier_Effects.Player
{
    public class AdrenalineIsADrug : BaseModifierPlayer
    {
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public AdrenalineIsADrug(ModifierData data) : base(data)
        {
            Data = data;
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

        public override void OnHitSomething()
        {
            throw new System.NotImplementedException();
        }

        public override void OnPickup()
        {
            var mHealth =PlayerContainer.Instance.stats.maxhealth /= 2;
            var cHealth = PlayerContainer.Instance.stats.currentHealth;

            if (cHealth > mHealth)
            {
                PlayerContainer.Instance.stats.currentHealth = mHealth;
            }
            //Add some of that speed;
            PlayerContainer.Instance.stats.extraSpeed += 10;
        }

        public override void OnDrop()
        {
            throw new System.NotImplementedException();
        }
    }
}
