using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
public class PlayerData : ScriptableObject
{
    public PlayerParam[] playerParams;
    [System.Serializable]
    public class PlayerParam
    {
        public int HP;
        public float power;
        public float moveSpeed;
    }
}
