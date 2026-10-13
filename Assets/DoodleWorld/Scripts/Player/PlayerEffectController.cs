using UnityEngine;
using Cysharp.Threading.Tasks;
using R3;

public class PlayerEffectController : MonoBehaviour
{
    Material material;

    void Awake()
    {
        Renderer renderer=GetComponent<Renderer>();
        material=renderer.material;
    }

    public async UniTask HitEffect(int damage)
    {
        material.EnableKeyword("_EMISSION");
        await HitStop(damage);
        await UniTask.Delay(500);
        material.DisableKeyword("_EMISSION");
        Debug.Log("hitEffect");
    }

    public async UniTask HitStop(int damage)
    {
        int hitStop=damage*10;
        int minHitStop=100;
        int maxHitStop=500;
        if(hitStop<=minHitStop)hitStop=minHitStop;
        if(hitStop>=maxHitStop)hitStop=maxHitStop;

        Time.timeScale=0;
        await UniTask.Delay(hitStop,ignoreTimeScale:true);
        Time.timeScale=1;
    }
}