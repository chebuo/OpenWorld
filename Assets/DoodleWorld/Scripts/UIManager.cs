using UnityEngine;
using UnityEngine.UI;
using R3;

public class UIManager : MonoBehaviour
{
    [SerializeField]private Image[] playerImages;
    [SerializeField]private Sprite noJoinedSprites;
    [SerializeField]private Sprite joinedSprites;
    [SerializeField]private Sprite readySprites;

    [SerializeField]GameObject readyButton;

    DeviceManager deviceManager;

    void Start()
    {
        deviceManager=FindFirstObjectByType<DeviceManager>();
        readyButton.SetActive(false);

        for(int i=0;i<playerImages.Length;i++)
        {
            UpdatePlayerUI(i);
        }
        deviceManager.OnPlayerStateChanged.Subscribe(playerIndex =>
        {
            UpdatePlayerUI(playerIndex);
            UpdateReadyUI();
            Debug.Log($"Player {playerIndex} state changed. Updated UI.");
        }).AddTo(this);

        deviceManager.OnAllReady.Subscribe(_ =>
        {
            AllReady();
        }).AddTo(this);
    }
    private void UpdatePlayerUI(int playerIndex)
    {
        PlayerJoinState state=deviceManager.GetPlayerState(playerIndex);
        switch (state)
        {
            case PlayerJoinState.NoJoined:
                playerImages[playerIndex].sprite=noJoinedSprites;
                break;
            case PlayerJoinState.Joined:
                playerImages[playerIndex].sprite=joinedSprites;
                break;
            case PlayerJoinState.Ready:
                playerImages[playerIndex].sprite=readySprites;
                break;
        }
        Debug.Log($"Player {playerIndex}P:{state}");
    }

    private void UpdateReadyUI()
    {
        bool isAllReady=deviceManager.IsAllReady();

        readyButton.SetActive(isAllReady);
    }

    private void AllReady()
    {
        readyButton.SetActive(true);
    }
}