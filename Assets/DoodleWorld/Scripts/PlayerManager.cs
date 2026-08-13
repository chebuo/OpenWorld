using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using UnityEditor.Callbacks;
using System.Globalization;
using DoodleWorld;
using Cysharp.Threading.Tasks.Triggers;

public class PlayerManager : MonoBehaviour
{
    InputAction move;
    InputAction jump;
    InputAction attack;
    InputAction dig;
    PlayerState currentState = PlayerState.start;
    PlayerController playerController;
    [SerializeField] TerrainGenerator terrainGenerator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async UniTaskVoid Start()
    {
        move=InputSystem.actions.FindAction("Move");
        jump=InputSystem.actions.FindAction("Jump");
        attack=InputSystem.actions.FindAction("Attack");
        dig=InputSystem.actions.FindAction("Dig");
        move.Enable();
        jump.Enable();
        attack.Enable();
        dig.Enable();
        playerController = this.GetComponent<PlayerController>();

        _ = StateLoop();
        _ = InputLoop();
    }

    async UniTask StateLoop()
    {
        while (true)
        {
            var state = currentState;
            switch (state)
            {
                case PlayerState.start:
                    await Init();
                    break;
                case PlayerState.idle:
                    await IdleLoop();
                    break;
                case PlayerState.walking:
                    await WalkingLoop();
                    break;
                case PlayerState.attacking:
                    await AttackingLoop();
                    break;
                case PlayerState.dead:
                    await DeadLoop();
                    break;
                default:
                    break;
            }
            await UniTask.WaitUntil(()=>currentState!=state);
        }
    }

    public async UniTask Init()
    {
        await UniTask.WaitUntil(()=>terrainGenerator.isInit);
        playerController.Init();
        ChangeState(PlayerState.idle);
    }

    async UniTask InputLoop()
    {
        while (true)
        {
            ParallelInput();
            await UniTask.Yield();
        }
    }

    void ParallelInput()
    {
        if (jump.WasPressedThisFrame())
        {
            playerController.Jump();
        }
        if (dig.WasPressedThisFrame())
        {
            playerController.MoveDig(3f);
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
        if (jump.triggered)
        {
            playerController.Jump();
        }
        if (dig.triggered)
        {
            playerController.MoveDig(3f);
        }
        if (attack.triggered)
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

    async UniTask DeadLoop()
    {
        Debug.Log("You Dead Loop");
        OnEnterDead();
        while (currentState == PlayerState.dead)
        {
            OnDead();
            await UniTask.Yield();
        }
        OnExitDead();
    }

    void OnEnterDead()
    {
        Debug.Log("Enter Dead");
    }

    void OnDead()
    {
        Debug.Log("You Dead");
        ChangeState(PlayerState.idle);
    }

    void OnExitDead()
    {
        Debug.Log("Exit Dead");
        
    }

    public void ChangeState(PlayerState state)
    {
        if(currentState==state)return;
        currentState=state;
    }
}