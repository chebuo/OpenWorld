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

    public void Move()
    {
        var moveValue = move.ReadValue<Vector2>();
        Vector3 moveDir =
        transform.forward * moveValue.y +
        transform.right * moveValue.x;

        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed,rb.linearVelocity.y,moveDir.z * moveSpeed);
    }



    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Attack()
    {
        // Implementation for attack logic
    }

    public void Dig(float radius)
    {
        Debug.Log("Controller: Dig 呼ばれた");

        if (terrainGenerator == null)
        {
            Debug.LogError("TerrainGenerator null");
            return;
        }

        // とりあえず前方に掘る（デバッグ用）
        Vector3 digPos = transform.position + Vector3.down * 2f;

        Debug.Log($"Dig位置: {digPos}");
        Debug.DrawLine(transform.position, digPos, Color.red, 2f);
        Debug.DrawRay(digPos, Vector3.up * 2f, Color.green, 2f);

        terrainGenerator.Dig(digPos, radius);
    }

    public void ChangeDir()
    {
        var lookValue=look.ReadValue<Vector2>();
        transform.Rotate(0,lookValue.x,0);
    }
}
