using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DoodleWorld;

public class VersusBattleController : MonoBehaviour
{
    [SerializeField] PlayerManager[] playerManagers;

    public async UniTask Init(BattleSettings battleSettings, Dictionary<int, PlayerJoinData> playerDevices)
    {
        int playerCount = Mathf.Clamp(playerDevices.Count, 2, 4);

        for (int i = 0; i < playerCount; i++)
        {
            if(!playerDevices.TryGetValue(i,out PlayerJoinData joinData))
            {
                playerManagers[i].gameObject.SetActive(false);
                continue;
            }

            playerManagers[i].gameObject.SetActive(true);

            await playerManagers[i].Init(i,playerDevices[i].Device);
            playerManagers[i].StopInput();
        }
    }

    public void StartBattle()
    {
        foreach (var playerManager in playerManagers)
        {
            if (!playerManager.gameObject.activeSelf)
                continue;

            playerManager.StartInput();
        }
    }

    public void EndBattle()
    {
        foreach (var playerManager in playerManagers)
        {
            if (!playerManager.gameObject.activeSelf)
                continue;

            playerManager.StopInput();
        }
    }
}