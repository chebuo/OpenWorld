using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField]int HP=20;
    [SerializeField]float moveSpeed=5f;
    [SerializeField]int attackForce=5;
    [SerializeField]float digRadius=3f;

    [SerializeField] private InputActionAsset inputActionAsset;

    public InputActionAsset playerInputActions;
    private int playerIndex=0;
    public InputDevice device;

    private InputAction moveAction;
    private InputAction itemAttackAction;
    private InputAction digAction;

    [SerializeField]PlayerState currentState = PlayerState.idle;
    public bool isWaitInput=false;
    PlayerController playerController;
    [SerializeField] PlayerData playerData;
    void Awake()
    {
        playerController = this.GetComponent<PlayerController>();
        
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

    void Update()
    {
        var moveValue = moveAction.ReadValue<Vector2>();
        Debug.Log("moveAction"+moveAction);
        Debug.Log(
            $"P{playerIndex + 1} Move: {moveValue} / " +
            $"Device: {device.displayName}"
        );
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

    public async UniTask Init(int playerIndex,InputDevice device)
    {
        this.playerIndex=playerIndex;
        this.device=device;
        await playerController.Init();
        ChangeState(PlayerState.idle);
        SetupInput();
        
    }
    private void SetupInput()
{
    if (playerInputActions != null)
    {
        playerInputActions.Disable();
        Destroy(playerInputActions);
    }

    playerInputActions = Instantiate(inputActionAsset);

    // 先にDeviceを限定する
    foreach (InputActionMap actionMap in playerInputActions.actionMaps)
    {
        actionMap.devices = new InputDevice[] { device };
        actionMap.Disable();
    }

    InputActionMap playerMap = playerInputActions.FindActionMap("Player");

    // Action取得
    moveAction = playerInputActions.FindAction("Move");
    itemAttackAction = playerInputActions.FindAction("ItemAttack");
    digAction = playerInputActions.FindAction("Dig");

    // 必要なActionだけEnable
    playerMap.Enable();
    moveAction.Enable();
    digAction.Enable();

    Debug.Log(
        $"P{playerIndex + 1} " +
        $"Device={device.displayName} " +
        $"ID={device.deviceId} " +
        $"MoveEnabled={moveAction.enabled}"
    );

    foreach (var control in moveAction.controls)
    {
        Debug.Log(
            $"P{playerIndex + 1} Move Control: " +
            $"{control.path} / " +
            $"Device={control.device.displayName} / " +
            $"ID={control.device.deviceId}"
        );
    }
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
        if (digAction.WasPressedThisFrame())
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
        Debug.Log("Enter Idle");
    }

    void OnIdle()
    {
        var moveValue = moveAction.ReadValue<Vector2>();
        Debug.Log(
            $"P{playerIndex + 1} Move: {moveValue} / " +
            $"Device: {device.displayName}"
        );
        if (moveValue != Vector2.zero)
        {
            ChangeState(PlayerState.walking);
        }
        if (itemAttackAction.triggered)
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
            Debug.Log("Walking Loop");
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
        var moveValue=moveAction.ReadValue<Vector2>();
        if (moveValue == Vector2.zero)
        {
            ChangeState(PlayerState.idle);
        }
        if (itemAttackAction.triggered)
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

    public void StartInput()
    {
        if(playerInputActions==null)return;
        playerInputActions.Enable();
        isWaitInput=true;
        ChangeState(PlayerState.idle);
    }

    public void StopInput()
    {
        playerInputActions?.Disable();
        isWaitInput=false;
        ChangeState(PlayerState.idle);
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