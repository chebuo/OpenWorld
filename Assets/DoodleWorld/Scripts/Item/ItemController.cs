using UnityEngine;

public class ItemController : MonoBehaviour
{
    private float power;
    public float multipler;
    private GameObject owner;

    Collider col;
    TerrainGenerator terrainGenerator;

    public void Init(float power,GameObject owner)
    {
        this.power=power*multipler;
        this.owner=owner;
    }

    void Awake()
    {
        terrainGenerator=FindFirstObjectByType<TerrainGenerator>();
        col=this.GetComponent<SphereCollider>();
        terrainGenerator.DigCollider(col);
    }

    private void OnTriggerEnter(Collider col)
    {
        if(col.gameObject==owner)return;
        IDamageable damageable=col.GetComponent<IDamageable>();
        if(damageable!=null)damageable.TakeDamage((int)power);
    }
}