using UnityEngine;

[System.Serializable]
public class DropableItem
{
    public GameObject itemPrefab;
    public float dropChance;

    // Custom constructor
    public DropableItem(GameObject prefab, float chance)
    {
        itemPrefab = prefab;
        dropChance = chance;
    }
}