using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using DoodleWorld;

namespace DoodleWorld
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField]GameObject singleBattleControllerPrefab;
        [SerializeField]GameObject versusBattleControllerPrefab;
        [SerializeField]GameObject onlineBattleControllerPrefab;

        SingleBattleController singleBattleController;
        VersusBattleController versusBattleController;
        OnlineBattleController onlineBattleController;
        BattleSettings battleSettings;
        Dictionary<int, PlayerJoinData> playerDevices;
        PlayerManager playerManager;
        ItemGenerator itemGenerator;

        int countNumber=3;
        bool isBattle=false;

        public async UniTask Init(BattleSettings battleSettings, Dictionary<int, PlayerJoinData> playerDevices)
        {
            this.battleSettings = battleSettings;
            this.playerDevices = playerDevices;
            playerManager=FindFirstObjectByType<PlayerManager>();
            itemGenerator=FindFirstObjectByType<ItemGenerator>();

            switch (battleSettings.gameMode)
            {
                case GameMode.single:
                    await InitSingleMode();
                    break;
                case GameMode.versus:
                    await InitVersusMode();
                    break;
                case GameMode.online:
                    await InitOnlineMode();
                    break;
            }

            itemGenerator.GenerateItem();
            playerManager.isWaitInput=false;
            await CountDown();
            _=Timer();
            playerManager.isWaitInput=true;
        }

        public async UniTask InitSingleMode()
        {
            Debug.Log("Single Mode");
            GameObject singleControllerObj=Instantiate(singleBattleControllerPrefab);
            singleBattleController=singleControllerObj.GetComponent<SingleBattleController>();
            // Additional initialization for single mode can be added here
            await UniTask.Yield();
        }

        public async UniTask InitVersusMode()
        {
            Debug.Log("Versus Mode");
            GameObject versusControllerObj=Instantiate(versusBattleControllerPrefab);
            versusBattleController=versusControllerObj.GetComponent<VersusBattleController>();
            await versusBattleController.Init(battleSettings, playerDevices);
            await UniTask.Yield();
        }

        public async UniTask InitOnlineMode()
        {
            Debug.Log("Online Mode");
            GameObject onlineControllerObj=Instantiate(onlineBattleControllerPrefab);
            onlineBattleController=onlineControllerObj.GetComponent<OnlineBattleController>();
            // Additional initialization for online mode can be added here
            await UniTask.Yield();
        }

        public async UniTask CountDown()
        {
            for(int i = 0; i < countNumber; i++)
            {
                await UniTask.Delay(1000);
            }
            Debug.Log("START!!");
            isBattle=true;
        }

        public async UniTask Timer()
        {
            for(int i = battleSettings.battleTime; i > 0; i--)
            {
                await UniTask.Delay(1000);
            }
            Debug.Log("Time Up!!");
            isBattle=false;
        }

        public async UniTask BattleLoop(GameMode mode)
        {
            await UniTask.WaitUntil(()=>isBattle);
            switch (mode)
            {
                case GameMode.single:
                    await SingleBattleLoop();
                    break;
                case GameMode.versus:
                    await VersusBattleLoop();
                    break;
            }
        }
        async UniTask SingleBattleLoop()
        {
            while (isBattle)
            {
                await UniTask.Yield();
            }
        }

        async UniTask VersusBattleLoop()
        {
            while (isBattle)
            {
                await UniTask.Yield();
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