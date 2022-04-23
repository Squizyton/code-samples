using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MamoShop : NetworkBehaviour
{
    /// <summary>
    /// This is the set LOOT TABLE that mamo can use to generate his "for sale swords"
    /// </summary>
    /// <returns></returns>
    public List<LootTable> momoLootTables = new List<LootTable>();

    public Weapon[] weapons = new Weapon[3];

    public GameObject[] WeaponPriceTexts = new GameObject[3];

    //Would it be better to use a dictionary here? Or a Array to store the weapon Prices?
    public int[] weaponPrices = new int[3];

    public WeaponGenerator WeaponGenerator;

    [ServerCallback]
    void Start()
    {
        GenerateSellableWeapons();
    }

    public void BuyPotion(string typePotion)
    {
        CmdbuyPotion(typePotion);
    }


    public void BuyWeapon(int weaponNumber)
    {
        CmdBuyWeapon(weaponNumber);
    }


    [Command(ignoreAuthority = true)]
    public void CmdbuyPotion(string typePotion, NetworkConnectionToClient sender = null)
    {
        switch (typePotion)
        {
            case "Health":

                var playerSack = sender.identity.GetComponent<PlayerInventory>().playerSack;
                var player = sender.identity.GetComponent<Player>();
                var healthPot = new HealthPotion();
                var item = new ConsumableItem();
                item.consumable = healthPot;
                if (player.Gold > 500 && playerSack.AddItem(item))
                {
                    player.AddGold(-500);
                }

                break;
        }
    }

    [Command(ignoreAuthority = true)]
    public void CmdBuyWeapon(int slot, NetworkConnectionToClient sender = null)
    {
        var playerGold = sender.identity.GetComponent<Player>().Gold;
        //Fill in when inventory is done
        if (playerGold > weaponPrices[slot])
        {
            sender.identity.GetComponent<Player>().AddGold(-weaponPrices[slot]);
            var invWeapon = new WeaponItem(weapons[slot]);
            sender.identity.GetComponent<PlayerInventory>().playerSack.AddItem(invWeapon);
        }
        else
        {
            //Mamo saying you don't have enough money?
        }
    }

    void GenerateWeaponPrices(int slot, Weapon weapon)
    {
        var priceAmount = weapon.modifiers.Length * UnityEngine.Random.Range(1000, 1500);
        weaponPrices[slot] = priceAmount;
    }


    [Server]
    public void GenerateSellableWeapons()
    {
        ItemPool itemPool = new ItemPool();
        itemPool.FeedTemplates(momoLootTables);

        for (int x = 0; x < weapons.Length; x++)
        {
            weapons[x] = WeaponGenerator.GenerateSword(itemPool);
            GenerateWeaponPrices(x, weapons[x]);
        }
        FindObjectOfType<ShopReference>().GetSwordPrices(weaponPrices);
    }
}