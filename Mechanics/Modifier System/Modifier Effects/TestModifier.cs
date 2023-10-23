using Dungeon.Rooms;
using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;
using UnityEngine;

namespace Modifiers.Modifier_Effects
{
    public class TestModifier : BaseModifierRoom
    {
        public override void OnSpawn()
        {
            var x = 0;
            for (; x < 5; x++)
            {
                Object.Instantiate(Data.spawnableObjects[Random.Range(0, Data.spawnableObjects.Count)],
                    new Vector3(Random.Range(0, 100), Random.Range(0, 100), Random.Range(0, 100)),Quaternion.identity);
            }
        }
        public override void OnEnemyDeath()
        {
       
        }

        public override void OnTick()
        {
            throw new System.NotImplementedException();
        }

        public override void OnHit()
        {
            throw new System.NotImplementedException();
        }


        public TestModifier(ModifierData data, Dungeon.Rooms.Room currentRoom) : base(data, currentRoom)
        {
            Data = data;
            base.currentRoom = currentRoom;
        }
    }
}