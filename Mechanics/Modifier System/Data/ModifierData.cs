using System.Collections.Generic;
using Dungeon.Rooms;
using Modifiers.ModifierTypes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Modifiers.Data
{
    [CreateAssetMenu(fileName = "New Modifier", menuName = "Modifiers/New Modifier")]
    public class ModifierData : SerializedScriptableObject
    {
        [Title("Name")] public string modifierName;

        [Title("Description")] public string description;

        [Title("Modifier Effects What?"),
         InfoBox("If choosing Room, please have a reference of a box collider in the Room Script")]
        public ModifierType type;
        [ShowIf("@type == ModifierType.Enemy || type == ModifierType.Player"),Tooltip("Used for sorting into certain Actions")]
        public ModifierAction typeOfAction;
        [Title("Modifier")] public BaseModifier Modifier;
        [Title("Main Purpose of Modifier")] public ModifierEffect effect;
        [ShowIf("@effect == ModifierEffect.SpawnObjects")]
        public List<GameObject> spawnableObjects;
        [Title("Room you want to appear more"), ShowIf("@effect == ModifierEffect.SwayRoom")]
        public RoomTemplate roomYouWantToAppearMoreOften;
        [ShowIf("@effect == ModifierEffect.SwayRoom")]
        public float weightAmount;
        [Title("Ground Physics Material Replacement"),ShowIf("@effect == ModifierEffect.ReplaceGroundMaterial")]
        public PhysicMaterial newMaterial;
    }

    public enum ModifierType
    {
        Room,
        Enemy,
        Player,
        Manager
    }

    public enum ModifierAction
    {
        OnTick,
        OnDeath,
        OnHit,
        OnPickup,
        OnHitSomething
    }


    public enum ModifierEffect
    {
        SpawnObjects,
        Test,
        SwayRoom,
        ReplaceGroundMaterial,
        ChangeStats
    }
}