using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using DoodleWorld;

namespace DoodleWorld
{
    public class GameManager : MonoBehaviour
    {
        TitleManager titleManager;
        ResultManager resultManager;
        DeviceManager deviceManager;
        BattleManager battleManager;
        [SerializeField]BattleSettings battleSettings = new BattleSettings();
        private Dictionary<int, PlayerJoinData> playerDevices = new();

        public static GameManager Instance { get; private set; }

        public GameState currentState { get; private set; } = GameState.Idle;

        SceneHandler sceneHandler=new SceneHandler();
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
            UpdateStateFromScene();
        }

        private async void Start()
        {
            await GameLoop();
            Debug.Log(currentState);
        }

        public async UniTask GameLoop()
        {
            while (true)
            {
                switch (currentState)
                {
                    case GameState.Title:
                        await TitleScreen();
                        break;

                    case GameState.Playing:
                        await StartGame();
                        break;

                    case GameState.GameOver:
                        await EndGame();
                        break;
                }
            }
        }

        public async UniTask TitleScreen()
        {
            await MoveScene("Title");
            titleManager=FindFirstObjectByType<TitleManager>();
            deviceManager=FindFirstObjectByType<DeviceManager>();
            await titleManager.TitleLoop();
            battleSettings=titleManager.battleSettings;
            playerDevices=deviceManager.GetPlayerDevices();
            Debug.Log($"Game Mode: {battleSettings.gameMode}");
            currentState=GameState.Playing;
        }

        public async UniTask StartGame()
        {
            Debug.Log("Game Started");
            await MoveScene("Field");
            battleManager=FindFirstObjectByType<BattleManager>();
            await battleManager.Init(battleSettings, playerDevices);
            await battleManager.BattleLoop(battleSettings.gameMode);
            await battleManager.EndBattle();
            currentState=GameState.GameOver;
        }

        public void PauseGame()
        {
            if(currentState!=GameState.Playing)return;
            ChangeState(GameState.Paused);
            Debug.Log("Game Paused");
        }

        public async UniTask EndGame()
        {
            await MoveScene("Result");
            resultManager=FindFirstObjectByType<ResultManager>();
            await resultManager.ReslutLoop();
            await resultManager.EndResult();
            Debug.Log("Game Ended");
            currentState=GameState.Title;
        }

        public void UpdateStateFromScene()
        {
            switch (sceneHandler.GetActiveScene())
            {
                case "Title":
                    ChangeState(GameState.Title);
                    break;
                case "Field":
                    ChangeState(GameState.Playing);
                    break;
                case "Result":
                    ChangeState(GameState.GameOver);
                    break;
            }
        }

        public void ChangeState(GameState state)
        {
            if(currentState==state)return;
            currentState = state;
        }

        public async UniTask MoveScene(string sceneName)
        {
            await sceneHandler.LoadSceneAsync(sceneName);
        }
    }
}