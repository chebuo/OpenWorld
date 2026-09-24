using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using DoodleWorld;

namespace DoodleWorld
{
    public class BattleManager : MonoBehaviour
    {
        [SerializeField] GameObject singleControllerObj;
        [SerializeField] GameObject versusControllerObj;
        [SerializeField] GameObject onlineControllerObj;

        SingleBattleController singleBattleController;
        VersusBattleController versusBattleController;
        OnlineBattleController onlineBattleController;
        BattleSettings battleSettings;
        Dictionary<int, PlayerJoinData> playerDevices;
        PlayerManager playerManager;
        ItemGenerator itemGenerator;

        int countNumber = 3;
        bool isBattle = false;

        public async UniTask Init(BattleSettings battleSettings, Dictionary<int, PlayerJoinData> playerDevices)
        {
            this.battleSettings = battleSettings;
            this.playerDevices = playerDevices;
            playerManager = FindFirstObjectByType<PlayerManager>();
            itemGenerator = FindFirstObjectByType<ItemGenerator>();

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

            if (itemGenerator != null)
            {
                itemGenerator.GenerateItem();
            }

            await CountDown();
            _ = Timer();
            _=BattleLoop(battleSettings.gameMode);
            StartPlayerInputs();
        }

        private void StartPlayerInputs()
        {
            if (battleSettings.gameMode == GameMode.versus && versusBattleController != null)
            {
                versusBattleController.StartBattle();
            }
            else if (playerManager != null)
            {
                playerManager.StartInput();
            }
        }

        public async UniTask InitSingleMode()
        {
            Debug.Log("Single Mode");
            singleBattleController = singleControllerObj.GetComponent<SingleBattleController>();
            await UniTask.Yield();
        }

        public async UniTask InitVersusMode()
        {
            Debug.Log("Versus Mode");
            versusBattleController = versusControllerObj.GetComponent<VersusBattleController>();
            await versusBattleController.Init(battleSettings, playerDevices);
            await UniTask.Yield();
        }

        public async UniTask InitOnlineMode()
        {
            Debug.Log("Online Mode");
            onlineBattleController = onlineControllerObj.GetComponent<OnlineBattleController>();
            await UniTask.Yield();
        }

        public async UniTask CountDown()
        {
            for (int i = 0; i < countNumber; i++)
            {
                await UniTask.Delay(1000);
            }
            Debug.Log("START!!");
            isBattle = true;
        }

        public async UniTask Timer()
        {
            for (int i = battleSettings.battleTime; i > 0; i--)
            {
                await UniTask.Delay(1000);
            }
            Debug.Log("Time Up!!");
            isBattle = false;
        }

        public async UniTask BattleLoop(GameMode mode)
        {
            await UniTask.WaitUntil(() => isBattle);
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
                await versusBattleController.BattleLoop(isBattle);
                await UniTask.Yield();
            }
        }

        public async UniTask EndBattle()
        {
            if (battleSettings.gameMode == GameMode.versus && versusBattleController != null)
            {
                versusBattleController.EndBattle();
            }
            else if (playerManager != null)
            {
                playerManager.StopInput();
            }
            Debug.Log("Battle Finished");
            await UniTask.Delay(3000);
        }
    }
}