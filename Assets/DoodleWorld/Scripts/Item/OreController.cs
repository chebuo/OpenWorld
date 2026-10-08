using UnityEngine;
using Cysharp.Threading.Tasks;

public class OreController : MonoBehaviour,IDamageable
{
    private int HP;
    public bool isAlive=true;
    private ItemParams itemParams;

    public void Init(ItemParams itemData)
    {
        this.itemParams=itemData;
        HP=itemData.maxHP;
        isAlive=true;
    }

    public async UniTask TakeDamage(int damage)
    {
        HP-=damage;
        if (HP <= 0)
        {
            isAlive=false;
        }
        await UniTask.Yield();
    }
    public ItemParams GiveItem()
    {
        this.gameObject.SetActive(false);
        return new ItemParams
        {
            itemProp = new ItemProp
            {
                shape = itemParams.itemProp.shape,
                distance = itemParams.itemProp.distance,
                power = itemParams.itemProp.power,
                remaining = itemParams.itemProp.remaining,
                icon = itemParams.itemProp.icon
            },
            statusEffects = itemParams.statusEffects,
        };
    }
}