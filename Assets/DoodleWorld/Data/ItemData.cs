using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public ItemParam[] playerParams;
    [System.Serializable]
    public class ItemParam
    {
        public float hardness;
        public float power;
        public float rarity;
    }
}
