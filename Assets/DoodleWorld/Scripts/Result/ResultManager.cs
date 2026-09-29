using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using DoodleWorld;

public class ResultManager : MonoBehaviour
{
    [SerializeField]TextMeshProUGUI winnerResult;
    ResultState currentState=ResultState.show;
    public void ToTitle()
    {
        ChangeState(ResultState.end);
    }

    public async UniTask ReslutLoop()
    {
        string winnerIdx=BattleManager.winnerIdx.ToString();
        while (currentState == ResultState.show)
        {
            if(BattleManager.winnerIdx!=-1)winnerResult.text=$"{winnerIdx}P WINS!!";
            else winnerResult.text=$"NO CHANPION";
            await UniTask.WaitUntil(()=>currentState==ResultState.end);
        }
    }

    public async UniTask EndResult()
    {
        await UniTask.WaitUntil(()=>currentState==ResultState.end);
    }

    public void ChangeState(ResultState state)
    {
        if(currentState==state)return;
        currentState=state;
    }
}