using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;
using UnityEngine;

namespace Modifiers.Modifier_Effects.Room
{
    public class ReplaceGroundMaterial : BaseModifierRoom
    {
        // Start is called before the first frame update
    
        public ReplaceGroundMaterial(ModifierData data, Dungeon.Rooms.Room currentRoom) : base(data, currentRoom)
        {
            Data = data;
            base.currentRoom = currentRoom;
        }

        public override void OnSpawn()
        {
            currentRoom.groundGO.TryGetComponent(out Collider collider);

            collider.material = Data.newMaterial;
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
