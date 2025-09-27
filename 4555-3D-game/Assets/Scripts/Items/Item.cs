using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject pickupPrefab; // prefab to spawn when dropped
    public GameObject heldPrefab;
}
