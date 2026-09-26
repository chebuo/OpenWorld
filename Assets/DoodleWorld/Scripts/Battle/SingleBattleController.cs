using UnityEngine;
using Cysharp.Threading.Tasks;

public class SingleBattleController : MonoBehaviour
{
    PlayerManager playerManager;

    public void Init()
    {
        playerManager=FindFirstObjectByType<PlayerManager>();
        //await playerManager.Init();
        playerManager.isWaitInput=false;
    }
}