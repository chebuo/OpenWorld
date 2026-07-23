using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEditor.Callbacks;

public class PlayerManager : MonoBehaviour
{
    InputAction move;
    InputAction jump;
    InputAction attack;
    PlayerState currentState = PlayerState.idle;
    PlayerController playerController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        move=InputSystem.actions.FindAction("Move");
        jump=InputSystem.actions.FindAction("Jump");
        attack=InputSystem.actions.FindAction("Attack");
        move.Enable();
        jump.Enable();
        attack.Enable();
        playerController = this.GetComponent<PlayerController>();
        await StateLoop();
    }

    async UniTask StateLoop()
    {
        while (true)
        {
            var state = currentState;
            switch (state)
            {
                case PlayerState.idle:
                    await IdleLoop();
                    break;
                case PlayerState.walking:
                    await WalkingLoop();
                    break;
                case PlayerState.attacking:
                    await AttackingLoop();
                    break;
                default:
                    break;
            }
            await UniTask.WaitUntil(()=>currentState!=state);
        }
    }

    //Idle

    async UniTask IdleLoop()
    {
        Debug.Log("Idle Loop");
        OnEnterIdle();
        while (currentState == PlayerState.idle)
        {
            OnIdle();
            await UniTask.Yield();
        }
        OnExitIdle();
    }

    void OnEnterIdle()
    {
        Debug.Log("Enter Idle");
    }

    void OnIdle()
    {
        var moveValue = move.ReadValue<Vector2>();
        if (moveValue != Vector2.zero)
        {
            ChangeState(PlayerState.walking);
        }
        else if (jump.triggered)
        {
            playerController.Jump();
        }
        else if (attack.triggered)
        {
            ChangeState(PlayerState.attacking);
        }
    }

    void OnExitIdle()
    {
        Debug.Log("Exit Idle");
    }

    //Walking

    async UniTask WalkingLoop()
    {
        Debug.Log("Walking Loop");
        OnEnterWalking();
        while (currentState == PlayerState.walking)
        {
            OnWalking();
            await UniTask.Yield();
        }
        OnExitWalking();
    }

    void OnEnterWalking()
    {
        Debug.Log("Enter Walking");
    }

    void OnWalking()
    {
        var moveValue=move.ReadValue<Vector2>();
        if (moveValue == Vector2.zero)
        {
            ChangeState(PlayerState.idle);
        }
        if (attack.triggered)
        {
            ChangeState(PlayerState.attacking);
            return;
        }
        playerController.Move();
        Debug.Log("Walking");
    }

    void OnExitWalking()
    {
        Debug.Log("Exit Walking");
    }

    //Attacking

    async UniTask AttackingLoop()
    {
        Debug.Log("Attacking Loop");
        OnEnterAttacking();
        while (currentState == PlayerState.attacking)
        {
            OnAttacking();
            await UniTask.Yield();
        }
        OnExitAttacking();
    }

    void OnEnterAttacking()
    {
        Debug.Log("Enter Attacking");
    }

    void OnAttacking()
    {
        playerController.Attack();
    }

    void OnExitAttacking()
    {
        Debug.Log("Exit Attacking");
    }

    public void ChangeState(PlayerState state)
    {
        if(currentState==state)return;
        currentState=state;
    }
}