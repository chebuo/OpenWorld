using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using DoodleWorld;

namespace DoodleWorld
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField]BattleManager battleManager;

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
        }

        private async void Start()
        {
            UpdateStateFromScene();
            await GameLoop();
        }

        public async UniTask GameLoop()
        {
            while (true)
            {
                await TitleLoop();
                await StartGame();
                await EndGame();
            }
        }

        public async UniTask TitleLoop()
        {
            Debug.Log("Title Screen");

            await UniTask.WaitUntil(()=>currentState==GameState.Playing);
        }

        public async UniTask StartGame()
        {
            await battleManager.Init();
            //await battleManager.BattleLoop();
            Debug.Log("Game Started");

            await UniTask.WaitUntil(()=>currentState==GameState.GameOver);
        }

        public void PauseGame()
        {
            if(currentState!=GameState.Playing)return;
            ChangeState(GameState.Paused);
            Debug.Log("Game Paused");
        }

        public async UniTask EndGame()
        {
            MoveScene("Title");
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

        public void MoveScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}