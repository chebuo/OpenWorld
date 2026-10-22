using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

public class SoftcreamController : MonoBehaviour,IWeapon
{
    [SerializeField]float speed=2;
    [SerializeField]float speedMagnification=1.2f;
    float power;
    GameObject cream;
    private readonly ReactiveProperty<bool> isAttack=new(true);
    public ReadOnlyReactiveProperty<bool> IsAttack=>isAttack;

    Collider col;
    Rigidbody rb;
    PlayerController playerController;
    TerrainGenerator terrainGenerator; 
    void Awake()
    {
        cream=transform.GetChild(1).gameObject;
        col=cream.GetComponent<Collider>();
        rb=cream.GetComponent<Rigidbody>();
        terrainGenerator=FindFirstObjectByType<TerrainGenerator>();
    }

    public UniTask Attack(GameObject owner,float power)
    {
        playerController=owner.GetComponent<PlayerController>();
        this.power=power;

        CreamMove().Forget();
        StopAttack().Forget();

        return UniTask.CompletedTask;
    }

    public async UniTask CreamMove()
    {
        isAttack.Value=true;
        await UniTask.Delay(0);
        while (isAttack.Value)
        {
            Debug.Log("CreamMove");
            
            if(rb.linearVelocity.magnitude<=20)rb.AddForce(transform.up*speed);
            speed=speed*speedMagnification;
            if(playerController!=null)playerController.AddDigAmount(terrainGenerator.DigCollider(col,power));
            await UniTask.Delay(100,cancellationToken:destroyCancellationToken);
        }
    }

    private async UniTask StopAttack()
    {
        Debug.Log(isAttack);
        await UniTask.Delay(1500);
        isAttack.Value=false;
        Debug.Log(isAttack);
    }

    void Onestroy()
    {
        isAttack.Dispose();
    }
}