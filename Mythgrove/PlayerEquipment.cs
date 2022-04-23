using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Console.Scripts;
using Mirror;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Version = Mirror.Version;

public class PlayerEquipment : NetworkBehaviour {
    //Todo: Might convert this to a list that isnt gameObjects. Still debating
    public IConsumable[] consumables = new IConsumable[4];

    public GameControls _controls;
    public WeaponController wController;
    [SyncVar] public int currentWeapon = 0;

    public Weapon EquippedWeapon {
        get {
            try {
                return (playerInventory.playerEquipment.Slots[currentWeapon] as WeaponItem)?.weapon;
            } catch(IndexOutOfRangeException e) {
                //Todo: Fix IndexOutOfRangeException
                Debug.LogError(e.StackTrace);
                return null;
            }
        }
    }
    
    public int gold = 0;
    
    public Player local;
    public PlayerInventory playerInventory;
    
    public SkillTemplate defaultSkill;
    
    public override void OnStartLocalPlayer()
    {
        if (isLocalPlayer)
        {
            _controls = new GameControls();
            _controls.Enable();
            _controls.Gameplay.SwitchWeapon.performed += context => { handleWeaponSwitch(); };
            _controls.Gameplay.UseConsumable1.performed += context => { UseConsumable(3); };
            _controls.Gameplay.UseConsumable2.performed += context => { UseConsumable(4); };
            _controls.Gameplay.UseConsumable3.performed += context => { UseConsumable(5); };
            _controls.Gameplay.UseConsumable4.performed += context => { UseConsumable(6); };


            local = this.gameObject.GetComponent<Player>();
            var player = FindObjectsOfType<PlayerController>().FirstOrDefault(controller => controller.isLocalPlayer);
            
            defaultSkill.AddToPlayer(local.GetComponent<PlayerController>(), null);
            
            wController = player.gameObject.GetComponent<WeaponController>();
            playerInventory = this.gameObject.GetComponent<PlayerInventory>();
            var firstWeapon = playerInventory.playerEquipment.Slots[0];
            if (firstWeapon is WeaponItem)
            {
                currentWeapon = 0;
                StartCoroutine(CycleEquippedWeapon());
                OnWeaponSwitch?.Invoke(currentWeapon);
            }
        }
    }

    [ServerCallback]
    private void Update()
    {
        if (EquippedWeapon != null && EquippedWeapon.modifiers.Length > 0)
        {
            var spawningModifiers = EquippedWeapon.modifiers.Where(modifier => modifier is IPassiveModifier)
                .Cast<IPassiveModifier>();
            foreach (var spawningModifier in spawningModifiers)
            {
                spawningModifier.WeaponUpdate(local.GetComponent<WeaponController>(), EquippedWeapon);
            }
        }
    }
    
    public void SetupHud()
    {
   
    }
    
    void UseConsumable(int slot)
    {
        var consumable = playerInventory.playerEquipment.Slots[slot];
        if (consumable is ConsumableItem citem && citem.consumable != null)
        {
            citem.consumable.SetPlayer(local);
            citem.consumable.Effect();
            playerInventory.playerEquipment.RemoveItem(slot);
            OnConsumableUsed?.Invoke(slot);
        }
        else
        {
            return;
        }
    }
    
    #region Interact Methods

    /// <summary>
    /// Drops a weapon at a location
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="weapon"></param>
    void DropWeaponAtLocation(Transform transform, Weapon weapon)
    {
        LootGenerator.Instance.DropWeaponAtLocation(weapon, transform);
    }
    void handleWeaponSwitch()
    {
        if (currentWeapon == 2)
        {
            currentWeapon = 0;
        }
        else
        {
            currentWeapon++;
        }
        wController.Clear();
        OnWeaponSwitch?.Invoke(currentWeapon);
        StartCoroutine(CycleEquippedWeapon());
    }
    
    public void InsertGoldAmount(int gold)
    {
        this.gold += gold;
        ResetGold?.Invoke(this.gold);
    }

    public void EquipItem(int slot)
    {
        var playerSack = playerInventory.playerSack;
        var playerEquipment = playerInventory.playerEquipment;
        var item = playerSack.Slots[slot];
        if (item is WeaponItem weapon)
        {
            if(playerEquipment.AddItem(item));
            {
                playerSack.RemoveItem(slot);
                OnWeaponGet?.Invoke();
                if (Array.IndexOf(playerEquipment.Slots, item) == 0)
                {
                    currentWeapon = 0;
                    StartCoroutine(CycleEquippedWeapon());
                    OnWeaponSwitch?.Invoke(currentWeapon);
                }
                OnWeaponGet?.Invoke();
            }
        }
        if (item is ConsumableItem consumable)
        {
            if (playerEquipment.AddItem(item))
            {
                playerSack.RemoveItem(slot);
                OnConsumableGet?.Invoke();
            }
        }
    }
    public void UnEquipItem(int slot)
    {
        
        var playerSack = playerInventory.playerSack;
        var playerEquipment = playerInventory.playerEquipment;
        var item = playerEquipment.Slots[slot];
        playerSack.AddItem(playerEquipment.Slots[slot]);
        
        //If Current weapon is equipped
        if (item is WeaponItem weapon)
        {
            //wController.weapon.Equals(weapon.weapon) Does not work due to different Hash Codes. Idk it thinks its two different items
            if(currentWeapon == slot)
            {
                Debug.Log("Thrown Weapon");
                wController.Clear();
            }
        }

        if (item is ConsumableItem consumable)
        {
            OnConsumableUsed?.Invoke(slot);
        }

        playerEquipment.RemoveItem(slot);
    }

    public void UpdateHud()
    {
        OnWeaponGet?.Invoke();
    }

    public void StoreItem(int slot)
    {
        var playerSack = playerInventory.playerSack;
        var playerStorage = playerInventory.playerStorage;
        var item = playerSack.Slots[slot];
        playerStorage.AddItem(playerSack.Slots[slot]);
        playerSack.RemoveItem(slot); 
    }
    public void TakeStoredItem(int slot)
    {
        var playerSack = playerInventory.playerSack;
        var playerStorage = playerInventory.playerStorage;
        var item = playerSack.Slots[slot];
        playerSack.AddItem(playerStorage.Slots[slot]);
        playerStorage.RemoveItem(slot);
    }

    #endregion

    #region HelperMethod
    public void SwitchToActiveWeapon(int slot)
    {
        OnWeaponSwitch?.Invoke(slot);
    }

    public IEnumerator CycleEquippedWeapon()
    {
        yield return null;
        var item = playerInventory.playerEquipment.Slots[currentWeapon];
        if (item is WeaponItem weapon)
        {
            wController.CmdBuildWeapon(weapon.weapon);

            var skillName = weapon.weapon.skill;
            var skillModifiers = weapon.weapon.modifiers.Where(modifier => modifier is ISkillModifier).Cast<ISkillModifier>();
            foreach (var skillModifier in skillModifiers)
            {
                skillName = skillModifier.GetSkill();
            }
            
            var skillObj = Resources.Load($"Skills/{skillName}") as GameObject;
            if(!skillObj)
            {
                GameConsole.Error($"Could not find the skill \"{skillName}\"");
                yield break;
            }

            var skill = skillObj.GetComponent<SkillTemplate>();
            if (!skill)
            {
                GameConsole.Error($"Skill \"{skillName}\" is invalid");
                yield break;
            }
    
            skill.AddToPlayer(local.GetComponent<PlayerController>(), weapon.weapon);
		
            var spawningModifiers = weapon.weapon.modifiers.Where(modifier => modifier is IWeaponBuildModifier).Cast<IWeaponBuildModifier>();
            foreach (var spawningModifier in spawningModifiers)
            {
                spawningModifier.OnWeaponSpawn(local.GetComponent<WeaponController>(), weapon.weapon);
            }
        }
        else
        {
            defaultSkill.AddToPlayer(local.GetComponent<PlayerController>(), null);
        }
    }
    
    private void OnEnable()
    {
      //  _controls.Enable();
    }

    private void OnDisable()
    {
        //_controls.Disable();
    }

    #endregion

    #region Events

    public event Action<int> OnWeaponSwitch;
    public event Action OnWeaponGet;
    public event Action<int> OnWeaponDrop;
    public event Action<int> OnConsumableUse;
    public event Action OnConsumableGet;

    public event Action<int> OnConsumableUsed;
    public event Action ResetHud;
    public event Action<int> ResetGold;

    #endregion
}