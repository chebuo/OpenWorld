using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;

public class BattleManager : MonoBehaviour
{
    [SerializeField]PlayerManager playerManager;
    [SerializeField]ItemGenerator itemGenerator;
    public async Task Init()
    {
        Debug.Log("Init!!");
        await playerManager.Init();
        itemGenerator.GenerateItem();
    }

    public async UniTask BattleLoop()
    {
        while (true)
        {

        }
    }
}