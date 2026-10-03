using UnityEngine;

public class ItemController : MonoBehaviour
{
    private float power;
    public float multipler;

    public void Init(float power)
    {
        this.power=power*multipler;
    }

    private void OnTriggerEnter(Collider col)
    {
        IDamageable damageable=col.GetComponent<IDamageable>();
        if(damageable!=null)damageable.TakeDamage((int)power);
    }
}