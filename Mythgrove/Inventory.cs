using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

public class Inventory
{
    /// <summary>
    /// Called when the inventory might have been updated. Changes to the inventory are not guaranteed.
    /// </summary>
    public event Action onInventoryUpdated; 
    
    private IItem[] slots;

    public IItem[] Slots => slots;

    private IItemFilter[] slotFilters;

    
    public Inventory() : this(0) { }

    public Inventory(int slotCount)
    {
        slots = new IItem[slotCount];
        this.slotFilters = Enumerable.Repeat(new GenericItemSlot(), slotCount).ToArray();
    }

    public Inventory(int slotCount, IItemFilter[] slotFilters)
    {
        slots = new IItem[slotCount];
        this.slotFilters = slotFilters;

    }

    /// <summary>
    /// Adds an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add to the inventory</param>
    /// <returns>Returns true if item was added successfully</returns>
    public bool AddItem(IItem item)
    {
        for (var i = 0; i < slots.Length; i++)
        {
            if (CanSlotHoldItem(i, item))
            {
                if (slots[i] == null)
                {
                    slots[i] = item;
                    onInventoryUpdated?.Invoke();
                    return true;
                }
                else if (slots[i].CanStack(item))
                {
                    slots[i].Quantity++;
                    onInventoryUpdated?.Invoke();
                    return true;
                }
            }
        }
        onInventoryUpdated?.Invoke();
        return false;
    }
    
    /// <summary>
    /// Removes an item from a slot.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns>Returns the removed item</returns>
    public IItem RemoveItem(int slot)
    {
        var item = slots[slot];
        if (item != null)
        {
            slots[slot] = null;
        }
        onInventoryUpdated?.Invoke();
        return item;
    }

    public bool CanAddItem(IItem item)
    {
        for (var i = 0; i < slots.Length; i++)
        {
            if (CanSlotHoldItem(i,item))
            {
                return true;
            }
        }
        return false;
    }

    public bool CanSlotHoldItem(int slot,IItem item)
    {
        return (slots[slot] == null || slots[slot].CanStack(item)) && slotFilters[slot].CanAcceptItem(item);
    }

    public bool SetSlot(int slot,IItem item)
    {
        if (CanSlotHoldItem(slot, item))
        {
            return true;
        }
        else
        {
            return false; 
        }
    }
}
