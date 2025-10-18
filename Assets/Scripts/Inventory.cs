using UnityEngine;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [System.Serializable]
    public class InventorySlot
    {
        public Item item;
        public int quantity;
    }

    [Header("Inventory Settings")]
    public int maxSlots = 10;
    public List<InventorySlot> items = new List<InventorySlot>();

    public bool AddItem(Item newItem, int quantity = 1)
    {
        // Buscar si ya existe el item
        foreach (var slot in items)
        {
            if (slot.item == newItem)
            {
                slot.quantity += quantity;
                return true;
            }
        }

        // Si no existe y hay espacio, agregar nuevo slot
        if (items.Count < maxSlots)
        {
            items.Add(new InventorySlot { item = newItem, quantity = quantity });
            return true;
        }

        Debug.Log("Inventario lleno!");
        return false;
    }

    public bool RemoveItem(Item itemToRemove, int quantity = 1)
    {
        foreach (var slot in items)
        {
            if (slot.item == itemToRemove)
            {
                slot.quantity -= quantity;
                if (slot.quantity <= 0)
                {
                    items.Remove(slot);
                }
                return true;
            }
        }
        return false;
    }

    public bool HasItem(Item item, int quantity = 1)
    {
        foreach (var slot in items)
        {
            if (slot.item == item && slot.quantity >= quantity)
            {
                return true;
            }
        }
        return false;
    }
}