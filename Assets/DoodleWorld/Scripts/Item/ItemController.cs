using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

public class ItemController : MonoBehaviour
{
    private float power;
    public float multipler;
    private GameObject owner;

    public bool isAttack=true;

    IWeapon weapon;

    public void Init(float attackForce,GameObject owner)
    {
        this.power=attackForce*multipler;
        this.owner=owner;

        Attack().Forget();
    }

    void Awake()
    {
        weapon=this.GetComponent<IWeapon>();

        weapon.IsAttack.Where(isAttack=>!isAttack).Take(1).Subscribe(_=>{
            Destroy(this.gameObject);
        }).AddTo(this);
    }
    
    async UniTask Attack()
    {
        await UniTask.WaitUntil(()=>owner!=null);
        await weapon.Attack(owner,power);
        Debug.Log(power);
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject==owner)return;
        IDamageable damageable=col.GetComponent<IDamageable>();
        if(damageable!=null)damageable.TakeDamage((int)power);
    }
}