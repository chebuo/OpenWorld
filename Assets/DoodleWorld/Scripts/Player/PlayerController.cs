using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    Collider[] hitColliders = new Collider[10];
    [SerializeField] LayerMask hitLayer;
    public List<ItemProp> itemList = new List<ItemProp>();

    Rigidbody rb;
    [SerializeField] TerrainGenerator terrainGenerator;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    public async UniTask Init()
    {
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        }
        if (terrainGenerator != null)
        {
            await UniTask.WaitUntil(() => terrainGenerator.isInit);
            terrainGenerator.Dig(transform.position, 3f);
        }
    }

    public void Move(Vector2 moveValue, float speed)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (rb == null) return;

        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveValue.y + right * moveValue.x;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            moveDir.Normalize();
        }

        rb.linearVelocity = moveDir*speed;
    }

    public void Attack(int force)
    {
        DamageArea(transform.position, 1f)?.TakeDamage(force);
    }

    public void MoveDig(float radius, int damage)
    {
        if (terrainGenerator == null)
        {
            terrainGenerator = FindFirstObjectByType<TerrainGenerator>();
        }
        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator null");
            return;
        }
        Vector3 digPos = transform.position + transform.forward * 0.1f;
        terrainGenerator.Dig(digPos, radius);
        DamageArea(digPos, radius)?.TakeDamage(damage);
    }

    public IDamageable DamageArea(Vector3 pos, float radius)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(pos, radius, hitColliders, hitLayer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hitColliders[i];
            if (hit.gameObject == gameObject) continue;
            IDamageable damageable = hit.GetComponent<IDamageable>();
            GetItem(hit);
            if (damageable == null) continue;
            return damageable;
        }
        return null;
    }

    private void GetItem(Collider col)
    {
        if (col.gameObject.CompareTag("Item"))
        {
            ItemController itemController=col.GetComponent<ItemController>();
            if(itemController.isAlive)return;
            itemList.Add(itemController.GiveItem());
        }
    }

    public void ChangeDir(Vector2 lookValue)
    {  
        if (lookValue != Vector2.zero)
        {
            transform.Rotate(lookValue.y, lookValue.x, 0);
        }
    }
}
