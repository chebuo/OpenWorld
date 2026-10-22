using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

public class GrapeBombController:MonoBehaviour,IWeapon
{
    private readonly ReactiveProperty<bool> isAttack=new(true);
    public ReadOnlyReactiveProperty<bool> IsAttack=>isAttack;

    float power;
    Collider col;
    PlayerController playerController;
    TerrainGenerator terrainGenerator;

    void Awake()
    {
        col=this.GetComponent<Collider>();
        terrainGenerator=FindFirstObjectByType<TerrainGenerator>();
    }

    public async UniTask Attack(GameObject owner,float power)
    {
        playerController=owner.GetComponent<PlayerController>();
        this.power=power;

        await WaitBomb();
        await BombGrape();
    }

    private async UniTask WaitBomb()
    {
        Debug.Log("waiting");
        await UniTask.Delay(2000);
    }

    private async UniTask BombGrape()
    {
        Debug.Log("tata");
        playerController.AddDigAmount(terrainGenerator.DigCollider(col,power));
        await UniTask.Yield();
        isAttack.Value=false;
    }
}