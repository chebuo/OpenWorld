using UnityEngine;
using Cysharp.Threading.Tasks;

namespace DoodleWorld
{
    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance{get; private set;}
        PlayerManager playerManager;
        ItemGenerator itemGenerator;

        int countNumber=3;
        public int battleTime=120;
        bool isBattle=false;
        void Awake()
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
        public async UniTask Init()
        {
            playerManager=FindFirstObjectByType<PlayerManager>();
            itemGenerator=FindFirstObjectByType<ItemGenerator>();

            await playerManager.Init();
            itemGenerator.GenerateItem();
            playerManager.isWaitInput=false;
            await CountDown();
            _=Timer();
            playerManager.isWaitInput=true;
        }

        public async UniTask CountDown()
        {
            for(int i = 0; i < countNumber; i++)
            {
                Debug.Log($"{countNumber-i}");
                await UniTask.Delay(1000);
            }
            Debug.Log("START!!");
            isBattle=true;
        }

        public async UniTask Timer()
        {
            for(int i = battleTime; i >= 0; i--)
            {
                await UniTask.Delay(1000);
                Debug.Log($"Time Left: {i}");
            }
            Debug.Log("Time Up!!");
            isBattle=false;
        }

        public async UniTask BattleLoop()
        {
            await UniTask.WaitUntil(()=>isBattle);
            while (isBattle)
            {


                await UniTask.WaitUntil(()=>!isBattle);
            }
        }

        public async UniTask EndBattle()
        {
            playerManager.isWaitInput=false;
            Debug.Log("Battle Finished");
            await UniTask.Delay(3000);
        }
    }
}