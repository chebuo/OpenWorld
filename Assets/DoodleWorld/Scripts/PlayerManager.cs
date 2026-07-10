using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

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
                case PlayerState.jumping:
                    await JumpingLoop();
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

    async UniTask IdleLoop()
    {
        Debug.Log("Idle Loop");
        //while (true)
        {
            OnEnterIdle();
            while (currentState == PlayerState.idle)
            {
                OnIdle();
                await UniTask.Yield();
            }
            OnExitIdle();
        }
    }

    void OnEnterIdle()
    {
        Debug.Log("Enter Idle");
    }

    void OnIdle()
    {
        var moveValue = move.ReadValue<Vector2>();
        Debug.Log($"Move: {moveValue}");
        if (moveValue != Vector2.zero)
        {
            ChangeState(PlayerState.walking);
        }
        else if (jump.triggered)
        {
            ChangeState(PlayerState.jumping);
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

    async UniTask WalkingLoop()
    {
        Debug.Log("Walking Loop");
        //while (true)
        {
            OnEnterWalking();
            while (currentState == PlayerState.walking)
            {
                OnWalking();
                await UniTask.Yield();
            }
            OnExitWalking();
        }
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
        if (jump.triggered)
        {
            ChangeState(PlayerState.jumping);
            return;
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

    async UniTask JumpingLoop()
    {
        Debug.Log("Jumping Loop");
        //while (true)
        {
            OnEnterJumping();
            while (currentState == PlayerState.jumping)
            {
                OnJumping();
                await UniTask.Yield();
            }
            OnExitJumping();
        }
    }

    void OnEnterJumping()
    {
        Debug.Log("Enter Jumping");
    }

    void OnJumping()
    {
        playerController.Jump();
    }

    void OnExitJumping()
    {
        Debug.Log("Exit Jumping");
    }

    async UniTask AttackingLoop()
    {
        Debug.Log("Attacking Loop");
        //while (true)
        {
            OnEnterAttacking();
            while (currentState == PlayerState.attacking)
            {
                OnAttacking();
                await UniTask.Yield();
            }
            OnExitAttacking();
        }
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