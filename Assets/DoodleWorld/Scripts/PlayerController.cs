using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PlayerController : MonoBehaviour
{
    InputAction look;

    [SerializeField] Transform cameraTransform;
    Collider[] hitColliders = new Collider[10];
    [SerializeField] LayerMask playerLayer;
    public List<GameObject> itemList = new List<GameObject>();

    Rigidbody rb;
    [SerializeField] TerrainGenerator terrainGenerator;

    void Start()
    {
        if (InputSystem.actions != null)
        {
            look = InputSystem.actions.FindAction("Look");
            look?.Enable();
        }
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        ChangeDir();
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

    Transform CamTransform
    {
        get
        {
            if (cameraTransform != null) return cameraTransform;
            if (Camera.main != null) return Camera.main.transform;
            return transform;
        }
    }

    public void Move(Vector2 moveValue, float speed)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }
        if (rb == null) return;

        Transform cam = CamTransform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = forward * moveValue.y + right * moveValue.x;
        if (moveDir.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDir);
        }

        rb.linearVelocity = new Vector3(moveDir.x * speed, rb.linearVelocity.y, moveDir.z * speed);
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
        int hitCount = Physics.OverlapSphereNonAlloc(pos, radius, hitColliders, playerLayer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hitColliders[i];
            if (hit.gameObject == gameObject) continue;
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable == null) continue;
            return damageable;
        }
        return null;
    }

    public void ChangeDir()
    {
        if (look != null && look.enabled)
        {
            var lookValue = look.ReadValue<Vector2>();
            if (lookValue != Vector2.zero)
            {
                transform.Rotate(lookValue.y, lookValue.x, 0);
            }
        }
    }
}
