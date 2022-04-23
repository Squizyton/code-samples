using System;
using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

[CreateAssetMenu(fileName = "New Loot Table", menuName = "Scriptable Objects/Loot Table")]
public class LootTable : ScriptableObject
{
   [Header("Include Other Loot Tables")]
   public List<LootTable> includeDropPool = new List<LootTable>();
   
   [Header("Gold")]
   public int minGold;
   public int maxGold;
   
   [Header("Weapons")]
   public float weaponDropChance;
   public int maxWeaponDrops;
   [SerializeField]
   public SpecialParts specialParts;
   
   [Header("Consumables"),Tooltip("Do you want this loot table to spawn consumables")]
   public bool SpawnConsumeables;
   public int consumableChance;
   public List<GameObject> consumables = new List<GameObject>();
   /// <summary>
   /// How many consumables this can generate
   /// </summary>
   public int consumableAmount;
}

[Serializable]
public class SpecialParts : SerializableDictionaryBase<string,PartsList>
{
} 

