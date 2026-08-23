using UnityEngine;
using Cysharp.Threading.Tasks;
using DoodleWorld;

public class VersusBattleController : MonoBehaviour
{
    [SerializeField] PlayerManager[] playerManagers;

    public async UniTask Init(BattleSettings battleSettings)
    {
        int playerCount = Mathf.Clamp(battleSettings.playerCount, 2, 4);
        Debug.Log($"Initializing Versus Battle with {playerCount} players");

        for (int i = 0; i < playerManagers.Length; i++)
        {
            bool isParticipating = i < playerCount;

            playerManagers[i].gameObject.SetActive(isParticipating);

            if (!isParticipating)continue;
            Debug.Log($"Initializing Player {i}");
            await playerManagers[i].Init();
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