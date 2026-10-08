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
    public StatusEffect[] statusEffects;
    public GameObject prefab;

    public int maxHP;
    public float hardness;
    public float rarity;
}

[System.Serializable]
public class ItemProp
{
    public GameObject shape;
    public float distance;
    public float power;
    public int remaining;
    public Sprite icon;
}

[System.Serializable]
public class StatusEffect
{
    public StatusEffectType type;
    public float value;
}

public enum StatusEffectType
{
    WalkSpeed,
    AttackForce,
    DigRadius,
    RecoverHP
}
