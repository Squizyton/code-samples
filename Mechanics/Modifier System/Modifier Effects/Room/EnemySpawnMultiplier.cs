using Managers;
using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;

namespace Modifiers.Modifier_Effects.Room
{
    public class EnemySpawnMultiplier : BaseModifierRoom
    {
        // Start is called before the first frame update

        public EnemySpawnMultiplier(ModifierData data, Dungeon.Rooms.Room currentRoom) : base(data, currentRoom)
        {
            Data = data;
            base.currentRoom = currentRoom;
        }

        public override void OnSpawn()
        {
            currentRoom.baseSpawnAmount += (int)(GameManager.instance.playerStats.currentHealth * .5f);
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
