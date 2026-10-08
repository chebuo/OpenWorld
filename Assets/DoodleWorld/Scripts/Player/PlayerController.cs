using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

public class PlayerController : MonoBehaviour
{
    [SerializeField] Transform cameraTransform;
    Collider[] hitColliders = new Collider[10];
    [SerializeField] LayerMask hitLayer;
    public List<ItemProp> itemList = new List<ItemProp>();
    private readonly ReactiveProperty<ItemProp> currentItem=new(null);
    public ReadOnlyReactiveProperty<ItemProp> CurrentItem => currentItem;

    Rigidbody rb;
    [SerializeField] TerrainGenerator terrainGenerator;
    PlayerManager playerManager;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerManager = this.GetComponent<PlayerManager>();
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

        //rb.linearVelocity = new Vector3(moveDir.x*speed,rb.linearVelocity.y,moveDir.z*speed);
        rb.linearVelocity=moveDir*speed;
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
        Vector3 digPos = transform.position + transform.forward * 0.1f+transform.up*(radius/2);
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
            OreController oreController=col.GetComponent<OreController>();
            if(oreController.isAlive)return;
            ItemParams itemParams=oreController.GiveItem();
            if (itemParams.itemProp.shape == null)
            {
                playerManager.UseStatusEffect(itemParams.statusEffects[0]);
                return;
            }
            else
            {
                itemList.Add(itemParams.itemProp);
                currentItem.Value=itemList[0];
                Debug.Log($"Item Collected: {itemParams.itemProp.shape.name}");
                Debug.Log($"Item List Count: {currentItem.Value}");
                return;
            }
            
        }
    }

    public void UseItem()
    {
        if(itemList.Count==0)return;
        ItemProp item=itemList[0];
        
        item.remaining--;
        GameObject weapon=Instantiate(item.shape,transform.position+transform.forward*item.distance,transform.rotation);
        ItemController itemController=weapon.GetComponent<ItemController>();
        if (itemController != null)
        {
            itemController.Init(item.power,this.gameObject);
        }
        Destroy(weapon,1f);

        if(item.remaining<=0)
        {
            itemList.RemoveAt(0);
            currentItem.Value=itemList.Count > 0 ? itemList[0] : null;
        }
    }

    private float xRot=0f;
    private float yRot=0f;
    public void ChangeDir(Vector2 lookValue)
    {  
        if (lookValue != Vector2.zero)
        {
            xRot-=lookValue.y;
            xRot=Mathf.Clamp(xRot,-90,90);

            transform.Rotate(0, lookValue.x, 0);

            yRot+=lookValue.x;
            transform.rotation=Quaternion.Euler(xRot,yRot,0);
        }
    }
}
