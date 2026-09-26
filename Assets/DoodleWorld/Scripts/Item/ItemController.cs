using UnityEngine;

public class ItemController : MonoBehaviour,IDamageable
{
    private int HP;
    public bool isAlive=true;
    private ItemParams itemParams;

    public void Init(ItemParams itemData)
    {
        this.itemParams=itemData;
        HP=itemData.maxHP;
    }

    public void TakeDamage(int damage)
    {
        HP-=damage;
        if (HP <= 0)
        {
            isAlive=false;
        }
    }
    public ItemProp GiveItem()
    {
        this.gameObject.SetActive(false);
        return itemParams.itemProp;
    }
}