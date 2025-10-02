using UnityEditor;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public GameObject prefab; // prefab to spawn when dropped
    public GameObject heldPrefab;
}
