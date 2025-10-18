using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Abyssal Mail/Item")]
public class Item : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemType itemType;
    public bool isQuestItem = false;

    [Header("Value")]
    public int value = 10;
}

public enum ItemType
{
    Document,    // Cartas, mensajes
    Package,     // Paquetes
    Resource,    // Recursos coleccionables
    KeyItem,     // Items especiales de misión
    Consumable   // Items usables
}