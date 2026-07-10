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

    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        move=InputSystem.actions.FindAction("Move");
        jump=InputSystem.actions.FindAction("Jump");
        attack=InputSystem.actions.FindAction("Attack");
        rb=this.GetComponent<Rigidbody>();
    }

    public void Move()
    {
        var moveValue = move.ReadValue<Vector2>();
        rb.AddForce(new Vector3(moveValue.x * moveSpeed, 0, moveValue.y * moveSpeed));
        Debug.Log(rb.linearVelocity);
    }

    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void Attack()
    {
        // Implementation for attack logic
    }
}
