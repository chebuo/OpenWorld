using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

public class PlayerManager : MonoBehaviour, IDamageable
{
    [SerializeField] int HP = 20;
    [SerializeField] float moveSpeed = 5f;
    [SerializeField] int attackForce = 5;
    [SerializeField] float digRadius = 3f;

    [SerializeField] private InputActionAsset inputActionAsset;

    public InputActionAsset playerInputActions;
    private int playerIndex = 0;
    public InputDevice device;

    private InputAction moveAction;
    private InputAction itemAttackAction;
    private InputAction digAction;
    private InputAction lookAction;

    [SerializeField] public PlayerState currentState = PlayerState.idle;
    public bool isWaitInput = false;
    PlayerController playerController;
    [SerializeField] PlayerData playerData;

    void Awake()
    {
        playerController = GetComponent<PlayerController>();

        if (playerData != null)
        {
            HP = playerData.HP;
            moveSpeed = playerData.moveSpeed;
            attackForce = playerData.attackForce;
        }
    }

    void Start()
    {
        _ = StateLoop();
        _ = InputLoop();
    }

    public async UniTask Init(int playerIndex, InputDevice device)
    {
        this.playerIndex = playerIndex;
        this.device = device;
        if (playerController != null)
        {
            await playerController.Init();
        }
        SetupInput();
        ChangeState(PlayerState.idle);
    }

    private void SetupInput()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
            Destroy(playerInputActions);
        }

        if (inputActionAsset == null)
        {
            Debug.LogWarning($"[P{playerIndex + 1}] inputActionAsset is null.");
            return;
        }

        playerInputActions = Instantiate(inputActionAsset);

        // Filter devices
        if (device != null)
        {
            System.Collections.Generic.List<InputDevice> targetDevices = new System.Collections.Generic.List<InputDevice> { device };
            if (device is Keyboard && Mouse.current != null)
            {
                targetDevices.Add(Mouse.current);
            }

            foreach (InputActionMap map in playerInputActions.actionMaps)
            {
                map.devices = targetDevices.ToArray();
            }
        }

        InputActionMap playerMap = playerInputActions.FindActionMap("Player");
        if (playerMap != null)
        {
            moveAction = playerMap.FindAction("Move");
            itemAttackAction = playerMap.FindAction("Attack") ?? playerMap.FindAction("ItemAttack");
            digAction = playerMap.FindAction("Dig");
            lookAction=playerMap.FindAction("Look");
        }

        Debug.Log($"[P{playerIndex + 1}] Input setup complete. Device: {(device != null ? device.displayName : "None")}");
    }

    public void StartInput()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Enable();
        }
        isWaitInput = true;
        ChangeState(PlayerState.idle);
        Debug.Log($"[P{playerIndex + 1}] Input started.");
    }

    public void StopInput()
    {
        if (playerInputActions != null)
        {
            playerInputActions.Disable();
        }
        isWaitInput = false;
        ChangeState(PlayerState.idle);
        Debug.Log($"[P{playerIndex + 1}] Input stopped.");
    }

    private async UniTask StateLoop()
    {
        while (this != null && gameObject != null)
        {
            await UniTask.WaitUntil(() => isWaitInput);
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
                    await UniTask.Yield();
                    break;
            }
            await UniTask.Yield();
        }
    }

    private async UniTask InputLoop()
    {
        while (this != null && gameObject != null)
        {
            await UniTask.WaitUntil(() => isWaitInput);
            ParallelInput();
            await UniTask.Yield();
        }
    }

    private void ParallelInput()
    {
        if (IsDigTriggered())
        {
            if (playerController != null)
            {
                playerController.MoveDig(digRadius, attackForce);
            }
        }
        Debug.Log("ChangeDir");
        ChangeDir();
    }

    private bool IsDigTriggered()
    {
        if (digAction != null && digAction.enabled && digAction.WasPressedThisFrame())
        {
            return true;
        }

        // Keyboard fallback
        if (device is Keyboard || device == null)
        {
            if (Keyboard.current != null)
            {
                if (playerIndex == 0 && Keyboard.current.spaceKey.wasPressedThisFrame)
                    return true;
                if (playerIndex == 1 && (Keyboard.current.numpad0Key.wasPressedThisFrame || Keyboard.current.enterKey.wasPressedThisFrame))
                    return true;
            }
        }

        return false;
    }

    public void ChangeDir()
    {
        var lookValue=lookAction.ReadValue<Vector2>();
        playerController.ChangeDir(lookValue);
    }

    private Vector2 GetMoveVector()
    {
        Vector2 value = Vector2.zero;
        if (moveAction != null && moveAction.enabled)
        {
            value = moveAction.ReadValue<Vector2>();
        }

        // Keyboard fallback / 1P 2P keyboard separation
        if ((device is Keyboard || device == null) && value == Vector2.zero)
        {
            if (Keyboard.current != null)
            {
                Vector2 kbValue = Vector2.zero;
                if (playerIndex == 0)
                {
                    if (Keyboard.current.wKey.isPressed) kbValue.y += 1f;
                    if (Keyboard.current.sKey.isPressed) kbValue.y -= 1f;
                    if (Keyboard.current.aKey.isPressed) kbValue.x -= 1f;
                    if (Keyboard.current.dKey.isPressed) kbValue.x += 1f;
                }
                else if (playerIndex == 1)
                {
                    if (Keyboard.current.upArrowKey.isPressed) kbValue.y += 1f;
                    if (Keyboard.current.downArrowKey.isPressed) kbValue.y -= 1f;
                    if (Keyboard.current.leftArrowKey.isPressed) kbValue.x -= 1f;
                    if (Keyboard.current.rightArrowKey.isPressed) kbValue.x += 1f;
                }
                if (kbValue != Vector2.zero)
                {
                    value = kbValue.normalized;
                }
            }
        }

        return value;
    }

    // --- Idle State ---

    private async UniTask IdleLoop()
    {
        OnEnterIdle();
        while (isWaitInput && currentState == PlayerState.idle)
        {
            OnIdle();
            await UniTask.Yield();
        }
        OnExitIdle();
    }

    private void OnEnterIdle()
    {
    }

    private void OnIdle()
    {
        var moveValue = GetMoveVector();
        if (moveValue != Vector2.zero)
        {
            ChangeState(PlayerState.walking);
            return;
        }

        if (itemAttackAction != null && itemAttackAction.enabled && itemAttackAction.triggered)
        {
            ChangeState(PlayerState.attacking);
        }
    }

    private void OnExitIdle()
    {
    }

    // --- Walking State ---

    private async UniTask WalkingLoop()
    {
        OnEnterWalking();
        while (isWaitInput && currentState == PlayerState.walking)
        {
            OnWalking();
            await UniTask.Yield();
        }
        OnExitWalking();
    }

    private void OnEnterWalking()
    {
    }

    private void OnWalking()
    {
        var moveValue = GetMoveVector();
        if (moveValue == Vector2.zero)
        {
            if (playerController != null)
            {
                playerController.Move(Vector2.zero, moveSpeed);
            }
            ChangeState(PlayerState.idle);
            return;
        }

        if (itemAttackAction != null && itemAttackAction.enabled && itemAttackAction.triggered)
        {
            ChangeState(PlayerState.attacking);
            return;
        }

        if (playerController != null)
        {
            playerController.Move(moveValue, moveSpeed);
        }
    }

    private void OnExitWalking()
    {
    }

    // --- Attacking State ---

    private async UniTask AttackingLoop()
    {
        OnEnterAttacking();
        while (isWaitInput && currentState == PlayerState.attacking)
        {
            OnAttacking();
            await UniTask.Yield();
        }
        OnExitAttacking();
    }

    private void OnEnterAttacking()
    {
        if (playerController != null)
        {
            playerController.Attack(attackForce);
        }
    }

    private void OnAttacking()
    {
        // Finish attack and return to idle
        ChangeState(PlayerState.idle);
    }

    private void OnExitAttacking()
    {
    }

    // --- Dead State ---

    private async UniTask DeadLoop()
    {
        OnEnterDead();
        while (isWaitInput && currentState == PlayerState.dead)
        {
            OnDead();
            await UniTask.Yield();
        }
        OnExitDead();
    }

    private void OnEnterDead()
    {
    }

    private void OnDead()
    {
        
    }

    private void OnExitDead()
    {
        this.gameObject.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        HP -= damage;
        Debug.Log($"Player {playerIndex + 1} took {damage} damage. Current HP: {HP}");
        if (HP <= 0)
        {
            ChangeState(PlayerState.dead);
        }
    }

    public void ChangeState(PlayerState state)
    {
        if (currentState == state) return;
        currentState = state;
    }
}