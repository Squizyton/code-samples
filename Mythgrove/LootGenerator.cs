using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class LootGenerator : NetworkBehaviour
{
    public static LootGenerator Instance { get; private set; }
    public WeaponGenerator wGen;
    public GameObject pickUpWeapon, juansSack;

    public void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GenerateLoot(List<LootTable> dropTables)
    {
        var itemPool = new ItemPool();

        //Generate Weapon
        var weaponDropPercentage = 0f;
        foreach (LootTable table in dropTables)
        {
            if (table.weaponDropChance > 0)
            {
                weaponDropPercentage += table.weaponDropChance;
            }
        }

        var wDropChance = Random.Range(0, 100);
        if (wDropChance < weaponDropPercentage)
        {
            itemPool.FeedTemplates(dropTables);
        }
    }

    public void GenerateLootAtLocation(List<LootTable> dropTables, Transform transform)
    {
        GenerateGold(dropTables, transform);
        GenerateConsumables(dropTables, transform);

        var itemPool = new ItemPool();
        //Generate Weapon
        var weaponDropPercentage = 0f;
        //Debug.Log(dropTables.Count);

        foreach (var table in dropTables)
        {
            //Debug.Log(table);
            weaponDropPercentage += table.weaponDropChance;
        }

        var wDropChance = Random.Range(0, 100);
        if (wDropChance < weaponDropPercentage)
        {
            itemPool.FeedTemplates(dropTables);
            spawnWeaponAtLocation(itemPool, transform);
        }
    }

    [Server]
    void spawnWeaponAtLocation(ItemPool itemPool, Transform transform)
    {
        var gWeapon = Instantiate(pickUpWeapon, transform.position, transform.rotation);
        var weapon = wGen.GenerateSword(itemPool);
        gWeapon.GetComponent<PickupWeapon>().weapon = weapon;
        var rb = gWeapon.GetComponent<Rigidbody>();
        //TODO: use variable, don't hardcode
        rb.AddForce(new Vector3(Random.Range(100, 150), Random.Range(100, 150), Random.Range(100, 150)));
        NetworkServer.Spawn(gWeapon);
    }

    [Server]
    public void DropWeaponAtLocation(Weapon weapon, Transform transform)
    {
        var gWeapon = Instantiate(pickUpWeapon, transform.position, transform.rotation);
        gWeapon.GetComponent<PickupWeapon>().weapon = weapon;
        var rb = gWeapon.GetComponent<Rigidbody>();
        //TODO: use variable, don't hardcode
        rb.AddForce(new Vector3(Random.Range(100, 150), Random.Range(100, 150), Random.Range(100, 150)));
        NetworkServer.Spawn(gWeapon);
    }

    [Server]
    public void GenerateGold(List<LootTable> dropTables, Transform transform)
    {
        int totalMinGoldAmount = 0;
        int totalMaxGoldAmount = 0;

        foreach (var table in dropTables)
        {
            totalMinGoldAmount += table.minGold;
            totalMaxGoldAmount += table.maxGold;
        }

        var sack = Instantiate(juansSack,
            new Vector3(transform.position.x, transform.position.y + 1, transform.position.z), transform.rotation);
        sack.GetComponent<MoneySack>().SetMoney(UnityEngine.Random.Range(totalMinGoldAmount, totalMaxGoldAmount));
        sack.GetComponent<Rigidbody>()
            .AddForce(new Vector3(Random.Range(100, 150), Random.Range(200, 250), Random.Range(100, 150)));
        NetworkServer.Spawn(sack);
    }

    [Server]
    public void GenerateConsumables(List<LootTable> lootTables, Transform transform)
    {
        foreach (LootTable lootTable in lootTables)
        {
            if (lootTable.SpawnConsumeables)
            {
                for (int x = 0; x < lootTable.consumableAmount; x++)
                {
                    var random = UnityEngine.Random.Range(0, 100) + 1;
                    if (random < lootTable.consumableChance)
                    {
                        var consumable =
                            Instantiate(lootTable.consumables[UnityEngine.Random.Range(0, lootTable.consumables.Count)],
                                transform.position, transform.rotation);
                        consumable.GetComponent<Rigidbody>()
                            .AddForce(new Vector3(Random.Range(100, 150), Random.Range(200, 250), Random.Range(100, 150)));
                        NetworkServer.Spawn(consumable);
                    }
                }
            }
        }
    }
}