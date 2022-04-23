using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WeaponGenerator : MonoBehaviour
{
    public static readonly string RESOURCE_PATH = "WeaponParts";

    //Weight values for rank
    public float[] table =
    {
        //Todo Pass monster values to bump up
        //D
        .48f,
        //C
        .21f,
        //B
        .19f,
        //A
        .11f,
        //S
        .01f
    };

    /// <summary>
    /// Generates a random sword
    /// </summary>
    /// <returns>The random weapon object</returns>
    public Weapon GenerateSword(ItemPool itemPool)
    {
        Weapon weapon = GenerateWeapon(itemPool, "hilt", "guard", "blade");

        weapon.skill = "Slash"; //Random.Range(0, 10) < 5 ? "Slash" : "Tornado"; //typeof(BasicMeleeSkill).FullName;

        //What does this do?
        weapon.mountInformation = new MountInformation
        {
            bone = "Right_Hand",
            position =  new Vector3(.15f,0,0),
            rotation = new Vector3(270, 0f, 0)
        };

        return weapon;
    }

    /// <summary>
    /// Loads the weapon part from the resources
    /// </summary>
    /// <param name="partName"></param>
    /// <returns></returns>
    public static WeaponPartTemplate LoadWeaponPart(string partName)
    {
        return Resources.Load<WeaponPartTemplate>(RESOURCE_PATH + "/" + partName);
    }

    private Weapon GenerateWeapon(ItemPool itemPool, params string[] partTypes)
    {
        var weapon = new Weapon();


        //Calculate rank through weight 
        float total = 0;
        foreach (var weight in table)
        {
            total += weight;
        }

        var randomNumber = UnityEngine.Random.Range(0, (float) total);

        for (int i = 0; i < table.Length; i++)
        {
            if (randomNumber <= table[i])
            {
                //Assign rank here?
                switch (i)
                {
                    case 0:
                        weapon.rank = Weapon.Rank.D;
                        break;
                    case 1:
                        weapon.rank = Weapon.Rank.C;
                        break;
                    case 2:
                        weapon.rank = Weapon.Rank.B;
                        break;
                    case 3:
                        weapon.rank = Weapon.Rank.A;
                        break;
                    case 4:
                        weapon.rank = Weapon.Rank.S;
                        break;
                }
                break;
            }

            {
                randomNumber -= table[i];
            }
        }
        weapon.partNames = new string[partTypes.Length];
        List<Modifier> lModifiers = new List<Modifier>();
        weapon.stats = new Stats();
        //Debug.Log(weapon.stats);

        //Stat Generation-------------------------
        for (var x = 0; x < partTypes.Length; x++)
        {
            var template = itemPool.GetTemplate(partTypes[x]);
            foreach (var statName in template.weaponStats.Keys)
            {
                if (weapon.stats != null)
                {
                    //Debug.Log("Adding key: " + statName);
                    if (!weapon.stats.ContainsKey(statName))
                    {
                        if (template.weaponStats.TryGetValue(statName, out var statRange))
                            weapon.stats.Add(statName, Random.Range(statRange.min, statRange.max));
                    }
                    else
                    {
                        //Debug.Log("Already found key: " + statName +  "Adding to: " + weapon.stats[statName]);
                        weapon.stats[statName] += Random.Range(template.weaponStats[statName].min,
                            template.weaponStats[statName].max);
                    }
                }
            }

            weapon.partNames[x] = template.name;
            //--------------------------------------     


            //Mod generation-------------------------
            lModifiers.AddRange(template.possibleMods);
            //-----------------
        }

        var amountMod = 0;
        switch (weapon.rank)
        {
            case Weapon.Rank.D:
                amountMod = 2;
                break;
            case Weapon.Rank.C:
                amountMod = 3;
                break;
            case Weapon.Rank.B:
                amountMod = 4;
                break;
            case Weapon.Rank.A:
                amountMod = 5;
                break;
            case Weapon.Rank.S:
                amountMod = 6;
                break;
        }

        
        
        var iModifier = new List<IModifier>();
            
        for (var x = 0; x < amountMod; x++)
        {
            Debug.Log("----");
            Debug.Log(lModifiers);
            Debug.Log(lModifiers.Count);
            lModifiers.ForEach(modifier => Debug.Log(modifier));
            Debug.Log("----");
            var index = GetRandomWeightedIndex(lModifiers.Select(modifier => modifier.weight).ToArray());
            var selectedMod = lModifiers[index];
           
            
            iModifier.Add(selectedMod.Generate());
        }
        weapon.modifiers = iModifier.ToArray();
        //itemPool.ResetList();


        return weapon;
    }
    
    public int GetRandomWeightedIndex(float[] weights)
    {
        if (weights == null || weights.Length == 0) return -1;
 
        float w;
        float t = 0;
        int i;
        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
 
            if (float.IsPositiveInfinity(w))
            {
                return i;
            }
            else if (w >= 0f && !float.IsNaN(w))
            {
                t += weights[i];
            }
        }
 
        float r = Random.value;
        float s = 0f;
 
        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];
            if (float.IsNaN(w) || w <= 0f) continue;
 
            s += w / t;
            if (s >= r) return i;
        }
 
        return -1;
    }
    
}