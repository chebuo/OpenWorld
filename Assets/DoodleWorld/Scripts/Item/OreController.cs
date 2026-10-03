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
    public ItemProp GiveItem()
    {
        this.gameObject.SetActive(false);
        return itemParams.itemProp;
    }
}