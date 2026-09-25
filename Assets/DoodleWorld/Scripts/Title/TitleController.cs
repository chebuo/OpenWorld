using System.Collections.Generic;
using UnityEngine;

public class TitleController : MonoBehaviour
{
    [SerializeField] TitleManager titleManager;

    private Stack<TitleState> stateHistory = new Stack<TitleState>();

    private void ChangeState(TitleState nextState)
    {
        stateHistory.Push(titleManager.currentState);
        titleManager.ChangeState(nextState);
    }

    public void OnClickTitleButton()
    {
        ChangeState(TitleState.modeSelect);
    }

    public void OnClickSettingsButton()
    {
        ChangeState(TitleState.settings);
    }

    public void OnClickSingleButton()
    {
        titleManager.SelectMode(GameMode.single);
        ChangeState(TitleState.start);
    }

    public void OnClickVersusButton()
    {
        titleManager.SelectMode(GameMode.versus);
        ChangeState(TitleState.playerSelect);
    }

    public void OnClickOnlineButton()
    {
        titleManager.SelectMode(GameMode.online);
        ChangeState(TitleState.start);
    }

    public void OnClickStartButton()
    {
        ChangeState(TitleState.end);
    }

    public void OnClickReturnButton()
    {
        if (stateHistory.Count == 0)return;

        TitleState previousState = stateHistory.Pop();
        titleManager.ChangeState(previousState);
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}