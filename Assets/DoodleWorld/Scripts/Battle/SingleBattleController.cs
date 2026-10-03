using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using DoodleWorld;

public class SingleBattleController : MonoBehaviour
{
    [Header("Test Settings")]
    [Tooltip("タイトルシーンを経由せず直接起動した場合にテストモードで初期化するかどうか")]
    [SerializeField] private bool isTestMode = true;

    [Tooltip("テスト時のバトル時間（秒）")]
    [SerializeField] private int testBattleTime = 60;

    [Tooltip("テスト実行時にGameManagerの干渉（フリーズやシーン自動遷移）を防ぐために無効化/破棄するか")]
    [SerializeField] private bool disableGameManagerInTest = true;

    [Tooltip("シーン内に他のPlayerManagerが存在する場合、1P以外を非アクティブにするか")]
    [SerializeField] private bool disableOtherPlayers = true;

    [Header("References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private ItemGenerator itemGenerator;

    private bool isInitialized = false;

    private void Awake()
    {
        // GameManagerがシーン内に存在する場合、タイトルシーン以外で起動すると
        // GameManager.GameLoop()の無限ループ（フリーズ）や予期せぬシーン遷移(Field/Result)が発生するため、
        // テストモード時はGameManagerの動作を停止・破棄する
        if (isTestMode && disableGameManagerInTest)
        {
            GameManager gm = FindFirstObjectByType<GameManager>();
            if (gm != null)
            {
                Debug.Log("[SingleBattleController] テストモードのためGameManagerを停止・破棄し、自動シーン遷移やフリーズを防止します。");
                gm.enabled = false;
                Destroy(gm.gameObject);
            }
        }
    }

    private async void Start()
    {
        // 外部から既にInit()が呼ばれていない、かつテストモードの場合はテスト用初期化を実行
        if (isTestMode && !isInitialized)
        {
            await StartTestBattle();
        }
    }

    /// <summary>
    /// 通常プレイ時、BattleManager等から呼び出される初期化処理
    /// </summary>
    public async UniTask Init()
    {
        if (isInitialized) return;
        isInitialized = true;

        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        if (playerManager != null)
        {
            InputDevice device = Keyboard.current != null ? (InputDevice)Keyboard.current : Gamepad.current;
            await playerManager.Init(0, device);
            playerManager.isWaitInput = false;
        }
    }

    /// <summary>
    /// テストシーン直接起動用のバトル初期化処理
    /// </summary>
    private async UniTask StartTestBattle()
    {
        isInitialized = true;
        Debug.Log("[SingleBattleController] テストバトルを開始します。");

        // 1. PlayerManagerの取得と初期化
        if (playerManager == null)
        {
            playerManager = FindFirstObjectByType<PlayerManager>();
        }

        // シーン内の他プレイヤーを非アクティブ化（Fieldシーン等で2P〜4Pが配置されている場合の対策）
        if (disableOtherPlayers && playerManager != null)
        {
            PlayerManager[] allPlayers = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
            foreach (var p in allPlayers)
            {
                if (p != playerManager)
                {
                    p.gameObject.SetActive(false);
                }
            }
        }

        InputDevice device = Keyboard.current != null ? (InputDevice)Keyboard.current : Gamepad.current;
        if (playerManager != null)
        {
            await playerManager.Init(0, device);
        }
        else
        {
            Debug.LogWarning("[SingleBattleController] シーン内にPlayerManagerが見つかりません。");
        }

        // 2. BattleManagerの取得と初期化
        if (battleManager == null)
        {
            battleManager = FindFirstObjectByType<BattleManager>();
        }

        if (battleManager != null)
        {
            var battleSettings = new BattleSettings
            {
                gameMode = GameMode.single,
                battleTime = testBattleTime,
                playerCount = 1
            };

            var playerDevices = new Dictionary<int, PlayerJoinData>();
            if (device != null)
            {
                playerDevices[0] = new PlayerJoinData(device);
            }

            // BattleManagerのInitを実行（カウントダウン、タイマー、アイテム生成、プレイヤー入力開始が実行される）
            await battleManager.Init(battleSettings, playerDevices);
        }
        else
        {
            // BattleManagerが無い場合でもプレイヤー単体でテストできるようにするフォールバック
            Debug.LogWarning("[SingleBattleController] シーン内にBattleManagerが見つかりません。Playerの入力を直接開始します。");
            if (playerManager != null)
            {
                playerManager.StartInput();
            }
        }
        if (itemGenerator == null)
        {
            itemGenerator=FindFirstObjectByType<ItemGenerator>();
        }
        itemGenerator.GenerateItem();
    }
}