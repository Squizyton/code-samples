using Modifiers.Data;
using Modifiers.Modifier_Effects.ModifierType;
using UnityEngine;

namespace Modifiers.Modifier_Effects.Room
{
    public class SpawnGrapplePoints : BaseModifierRoom
    {
        private float spawnChance = .25f;
        private const float SpawnAmountTimes = 10f;
        private const float SpawnRadius = 20f;

        // Start is called before the first frame update
        public SpawnGrapplePoints(ModifierData data, Dungeon.Rooms.Room currentRoom) : base(data, currentRoom)
        {
            base.currentRoom = currentRoom;
        }

    
        public override void OnSpawn()
        {
            var spawnCenter = currentRoom.modelCenterPoint.position + new Vector3(0, 10, 0);

            for (var i = 0; i < SpawnAmountTimes; i++)
            {
                var percentage = Random.Range(0f, 1f);

                if (percentage < spawnChance)
                {
                    var xPos = Mathf.Cos(Mathf.Deg2Rad * Random.Range(0, 360)) * Random.Range(0, SpawnRadius) +
                               spawnCenter.x;
                    var zPos = Mathf.Sin(Mathf.Deg2Rad * Random.Range(0, 360)) * Random.Range(0, SpawnRadius) +
                               spawnCenter.z;

                    Object.Instantiate(Data.spawnableObjects[Random.Range(0,Data.spawnableObjects.Count)].gameObject, new Vector3(xPos, spawnCenter.y + Random.Range(-5,20), zPos),
                        Quaternion.identity);
                }
            }
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
