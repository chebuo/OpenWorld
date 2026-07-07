using UnityEngine;
using DoodleWorld;

public class TitleController : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = GameManager.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClickStartButton()
    {
        Instance.ChangeState(GameState.Playing);
    }

    public void OnClickQuitButton()
    {
        Application.Quit();
    }
}
