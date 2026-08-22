using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using DoodleWorld;

namespace DoodleWorld
{
    public class GameManager : MonoBehaviour
    {
        TitleManager titleManager;
        BattleManager battleManager;

        public static GameManager Instance { get; private set; }

        public GameState currentState { get; private set; } = GameState.Idle;
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
            await titleManager.TitleLoop();
            currentState=GameState.Playing;
        }

        public async UniTask StartGame()
        {
            await MoveScene("Field");
            battleManager=FindFirstObjectByType<BattleManager>();
            await battleManager.Init();
            await battleManager.BattleLoop();
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
            Debug.Log("Game Ended");
            await UniTask.WaitUntil(()=>currentState==GameState.Title);
        }

        public void UpdateStateFromScene()
        {
            switch (SceneManager.GetActiveScene().name)
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
            await SceneManager.LoadSceneAsync(sceneName);
        }
    }
}