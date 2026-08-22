using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    InputAction look;

    [SerializeField] Transform cameraTransform;
    Collider[] hitColliders= new Collider[10];
    [SerializeField]LayerMask playerLayer;
    public List<GameObject> itemList=new List<GameObject>();

    Rigidbody rb;
    [SerializeField]TerrainGenerator terrainGenerator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        look=InputSystem.actions.FindAction("Look");
        look.Enable();
        rb=this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        ChangeDir();
    }

    public async UniTask Init()
    {
        await UniTask.WaitUntil(()=>terrainGenerator.isInit);
        terrainGenerator.Dig(transform.position , 3f);
    }

    public void Move(Vector2 moveValue,float speed)
    {
        Vector3 moveDir =
        cameraTransform.forward * moveValue.y +
        cameraTransform.right * moveValue.x;

        rb.linearVelocity =moveDir * speed;
    }

    public void Attack(int force)
    {
        DamageArea(transform.position, 1f)?.TakeDamage(force);
    }

    public void MoveDig(float radius,int damage)
    {

        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator null");
            return;
        }
        Vector3 digPos = transform.position + transform.forward*0.1f;
        terrainGenerator.Dig(digPos, radius);
        DamageArea(digPos, radius)?.TakeDamage(damage);
    }
    public IDamageable DamageArea(Vector3 pos,float radius)
    {
        int hitCount=Physics.OverlapSphereNonAlloc(pos,radius,hitColliders,playerLayer);
        for(int i = 0; i < hitCount; i++)
        {
            Collider hit=hitColliders[i];
            IDamageable damageable=hit.GetComponent<IDamageable>();
            if(damageable==null)continue;
            return damageable;
        }
        return null;
    }

    public void ChangeDir()
    {
        var lookValue=look.ReadValue<Vector2>();
        transform.Rotate(lookValue.y,lookValue.x,0);
    }
}
