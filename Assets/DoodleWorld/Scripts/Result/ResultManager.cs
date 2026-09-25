using UnityEngine;
using Cysharp.Threading.Tasks;

public class ResultManager : MonoBehaviour
{
    ResultState currentState=ResultState.show;
    public void ToTitle()
    {
        ChangeState(ResultState.end);
    }

    public async UniTask ReslutLoop()
    {
        while (currentState == ResultState.show)
        {
            
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