using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField]int HP=20;
    [SerializeField]float moveSpeed=5f;
    [SerializeField]int attackForce=5;
    [SerializeField]float digRadius=3f;

    InputAction move;
    InputAction attack;
    InputAction dig;
    PlayerState currentState = PlayerState.idle;
    public bool isWaitInput=false;
    PlayerController playerController;
    [SerializeField] PlayerData playerData;
    void Awake()
    {
        playerController = this.GetComponent<PlayerController>();
        move=InputSystem.actions.FindAction("Move");
        attack=InputSystem.actions.FindAction("Attack");
        dig=InputSystem.actions.FindAction("Dig");
        move.Enable();
        attack.Enable();
        dig.Enable();
        HP=playerData.HP;
        moveSpeed=playerData.moveSpeed;
        attackForce=playerData.attackForce;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _ = StateLoop();
        _ = InputLoop();
    }

    async UniTask StateLoop()
    {
        await UniTask.WaitUntil(()=>isWaitInput);
        while (isWaitInput)
        {
            await UniTask.WaitUntil(()=>isWaitInput);
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
        await playerController.Init();
        ChangeState(PlayerState.idle);
    }

    async UniTask InputLoop()
    {
        await UniTask.WaitUntil(()=>isWaitInput);
        while (isWaitInput)
        {
            await UniTask.WaitUntil(()=>isWaitInput);
            ParallelInput();
            await UniTask.Yield();
        }
    }

    void ParallelInput()
    {
        if (dig.WasPressedThisFrame())
        {
            playerController.MoveDig(digRadius,attackForce);
        }
    }

    //Idle

    async UniTask IdleLoop()
    {
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
        //Debug.Log("Enter Idle");
    }

    void OnIdle()
    {
        var moveValue = move.ReadValue<Vector2>();
        if (moveValue != Vector2.zero)
        {
            ChangeState(PlayerState.walking);
        }
        if (attack.triggered)
        {
            ChangeState(PlayerState.attacking);
        }
    }

    void OnExitIdle()
    {
        //Debug.Log("Exit Idle");
    }

    //Walking

    async UniTask WalkingLoop()
    {
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
        //Debug.Log("Enter Walking");
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
        playerController.Move(moveValue, moveSpeed);
    }

    void OnExitWalking()
    {
        //Debug.Log("Exit Walking");
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
        //playerController.Attack(attackForce);
    }

    void OnExitAttacking()
    {
        Debug.Log("Exit Attacking");
    }

    async UniTask DeadLoop()
    {
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
        ChangeState(PlayerState.idle);
    }

    void OnExitDead()
    {
        Debug.Log("Exit Dead");
        
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        Debug.Log($"Player took {damage} damage. Current HP: {HP}");
        if (HP <= 0)
        {
            ChangeState(PlayerState.dead);
        }
    }

    public void ChangeState(PlayerState state)
    {
        if(currentState==state)return;
        currentState=state;
    }
}