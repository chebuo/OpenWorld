using UnityEngine;

public class TitleController : MonoBehaviour
{
    [SerializeField]TitleManager titleManager;

    TitleState lastState;
    public void OnClickTitleButton()
    {
        lastState=TitleState.title;
        titleManager.ChangeState(TitleState.modeSelect);
        Debug.Log(titleManager.currentState);
    }

    public void OnClickSettingsButton()
    {
        lastState=titleManager.currentState;
        titleManager.ChangeState(TitleState.settings);
    }

    public void OnClickSingleButton()
    {
        lastState=TitleState.modeSelect;
        titleManager.SelectMode(GameMode.single);
        titleManager.ChangeState(TitleState.start);
    }

    public void OnClickVersusButton()
    {
        lastState=TitleState.modeSelect;
        titleManager.SelectMode(GameMode.versus);
        titleManager.ChangeState(TitleState.playerSelect);
    }

    public void OnClickOnlineButton()
    {
        lastState=TitleState.modeSelect;
        titleManager.SelectMode(GameMode.online);
        titleManager.ChangeState(TitleState.start);
    }
    
    public void OnClickStartButton()
    {
        titleManager.ChangeState(TitleState.end);
    }

    public void OnClickReturnButton()
    {
        titleManager.ChangeState(lastState);
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
