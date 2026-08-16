using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float attackForce = 5f;

    InputAction move;
    InputAction jump;
    InputAction attack;
    InputAction dig;
    InputAction look;

    [SerializeField] Transform cameraTransform;

    Rigidbody rb;
    [SerializeField]TerrainGenerator terrainGenerator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        move=InputSystem.actions.FindAction("Move");
        jump=InputSystem.actions.FindAction("Jump");
        attack=InputSystem.actions.FindAction("Attack");
        dig=InputSystem.actions.FindAction("Dig");
        look=InputSystem.actions.FindAction("Look");
        move.Enable();
        jump.Enable();
        attack.Enable();
        look.Enable();
        rb=this.GetComponent<Rigidbody>();
    }

    void Update()
    {
        ChangeDir();
    }

    public void Init()
    {
        terrainGenerator.Dig(transform.position , 3f);
    }

    public void Move()
    {
        var moveValue = move.ReadValue<Vector2>();
        Vector3 moveDir =
        cameraTransform.forward * moveValue.y +
        cameraTransform.right * moveValue.x;

        rb.linearVelocity =moveDir * moveSpeed;
    }



    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Attack()
    {
        // Implementation for attack logic
    }

    public void MoveDig(float radius)
    {
        Debug.Log("Controller: Dig 呼ばれた");

        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator null");
            return;
        }
        Vector3 digPos = transform.position + transform.forward*0.1f;
        terrainGenerator.Dig(digPos, radius);
    }

    public void ChangeDir()
    {
        var lookValue=look.ReadValue<Vector2>();
        transform.Rotate(lookValue.y,lookValue.x,0);
    }
}
