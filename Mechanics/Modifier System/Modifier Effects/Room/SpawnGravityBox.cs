using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;
using UnityEngine;

namespace Modifiers.Modifier_Effects.Room
{
    public class SpawnGravityBox :BaseModifierRoom
    {
        
        public SpawnGravityBox(ModifierData data, Dungeon.Rooms.Room currentRoom) : base(data, currentRoom)
        {
            Data = data;
            base.currentRoom = currentRoom;
        }

        public override void OnSpawn()
        {
            var box = Object.Instantiate(Data.spawnableObjects[0], currentRoom.modelCenterPoint.position, Quaternion.identity);
            box.TryGetComponent(out BoxCollider collider);
            var componentBounds = collider.bounds;
            var bounds = currentRoom.bounds.bounds;
            box.transform.localScale += new Vector3(bounds.size.x,bounds.size.y,bounds.size.z);
            Physics.SyncTransforms();
        }

        public override void OnEnemyDeath()
        {
        }

        public override void OnTick()
        {
        }

        public override void OnHit()
        {
            throw new System.NotImplementedException();
        }
    }
}
