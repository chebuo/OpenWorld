using UnityEngine;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class SceneHandler
{
    public void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public string GetActiveScene()
    {
        return SceneManager.GetActiveScene().name;
    }

    public async UniTask LoadSceneAsync(string name)
    {
        await SceneManager.LoadSceneAsync(name);
    }
}