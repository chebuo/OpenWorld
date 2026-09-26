using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public ItemParams[] items;
}

[System.Serializable]
public class ItemParams
{
    public ItemProp itemProp;
    public GameObject prefab;

    public int maxHP;
    public float hardness;
    public float rarity;
}

[System.Serializable]
public class ItemProp
{
    public GameObject shape;
    public float power;
    public float size;
}
