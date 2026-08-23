using UnityEngine;
using Cysharp.Threading.Tasks;

public class TitleManager : MonoBehaviour
{
    [SerializeField]GameObject titleScreen;
    [SerializeField]GameObject settingScreen;
    [SerializeField]GameObject modeScreen;
    [SerializeField]GameObject playerScreen;
    [SerializeField]GameObject startScreen;

    bool isTitle=true;

    public TitleState currentState{get; private set;}=TitleState.title;
    public GameMode gameMode{get;private set;}=GameMode.versus;
    
    public async UniTask TitleLoop()
    {
        while (isTitle)
        {
            var state=currentState;
            switch (state)
            {
                case TitleState.title:
                    TitleScreen();
                    break;
                case TitleState.settings:
                    SettingScreen();
                    break;
                case TitleState.modeSelect:
                    ModeScreen();
                    break;
                case TitleState.playerSelect:
                    PlayerScreen();
                    break;
                case TitleState.characterSelect:
                    CharacterScreen();
                    break;
                case TitleState.start:
                    StartScreen();
                    break;
                case TitleState.end:
                    GameStart();
                    return;
            }
            await UniTask.WaitUntil(()=>currentState!=state);
        }
    }
    
    void TitleScreen()
    {
        SetActiveScreen(titleScreen);
    }

    void SettingScreen()
    {
        SetActiveScreen(settingScreen);
    }

    void ModeScreen()
    {
        SetActiveScreen(modeScreen);
    }

    void PlayerScreen()
    {
        SetActiveScreen(playerScreen);
    }

    void CharacterScreen()
    {
        //SetActiveScreen(characterScreen);
    }

    void StartScreen()
    {
        SetActiveScreen(startScreen);
    }

    void GameStart()
    {
        isTitle=false;
    }

    void SetActiveScreen(GameObject activeScreen)
    {
        titleScreen.SetActive(activeScreen == titleScreen);
        settingScreen.SetActive(activeScreen == settingScreen);
        modeScreen.SetActive(activeScreen == modeScreen);
        playerScreen.SetActive(activeScreen == playerScreen);
        startScreen.SetActive(activeScreen==startScreen);
    }

    public void SelectMode(GameMode mode)
    {
        gameMode=mode;
    }

    public void ChangeState(TitleState state)
    {
        if(currentState==state)return;
        currentState=state;
    }
}