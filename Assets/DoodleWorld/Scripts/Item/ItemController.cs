using UnityEngine;

public class ItemController : MonoBehaviour
{
    private float power;
    public float multipler;
    private GameObject owner;

    Collider col;
    TerrainGenerator terrainGenerator;

    public void Init(float attackForce,GameObject owner)
    {
        this.power=attackForce*multipler;
        this.owner=owner;
    }

    void Awake()
    {
        terrainGenerator=FindFirstObjectByType<TerrainGenerator>();
        col=this.GetComponent<MeshCollider>();
    }

    void Start()
    {
        Attack();
        Debug.Log(power);
    }

    void Attack()
    {
        PlayerController playerController=owner.GetComponent<PlayerController>();
        if(playerController!=null)
        {
            playerController.AddDigAmount(terrainGenerator.DigCollider(col,power));
        }
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject==owner)return;
        IDamageable damageable=col.GetComponent<IDamageable>();
        if(damageable!=null)damageable.TakeDamage((int)power);
    }
}