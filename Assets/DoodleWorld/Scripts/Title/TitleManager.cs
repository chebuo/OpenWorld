using UnityEngine;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;
using DoodleWorld;

public class TitleManager : MonoBehaviour
{
    [SerializeField]GameObject titleScreen;
    [SerializeField]GameObject settingScreen;
    [SerializeField]GameObject modeScreen;
    [SerializeField]GameObject playerScreen;
    [SerializeField]GameObject startScreen;
    [SerializeField]GameObject returnButton;

    bool isTitle=true;

    public TitleState currentState{get; private set;}=TitleState.title;
    public BattleSettings battleSettings=new BattleSettings();

    [SerializeField]DeviceManager deviceManager;
    
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
        returnButton.SetActive(!titleScreen.activeSelf);
    }

    public void SelectMode(GameMode mode)
    {
        battleSettings.gameMode=mode;
    }

    public void ChangeState(TitleState state)
    {
        if(currentState==state)return;
        currentState=state;
    }

    public void SetPlayerCount(int count)
    {
        battleSettings.playerCount=count;
    }

    public InputDevice[] GetDevice()
    {
        InputDevice[] devices = new InputDevice[battleSettings.playerCount];
        for(int i=0;i<battleSettings.playerCount;i++)
        {
            devices[i] = deviceManager.GetDevice(i);
        }
        return devices;
    }

    public void SetPlayerCount()
    {
        battleSettings.playerCount=deviceManager.GetPlayerCount();
    }
}