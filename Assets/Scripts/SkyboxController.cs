using UnityEngine;

public class SkyColorController : MonoBehaviour
{
    public Light sun;
    public Material skyboxMat;

    // 👇 追加（Inspectorで編集できる）
    public Color nightColor;
    public Color morningColor;
    public Color dayColor;
    public Color eveningColor;

    void Update()
    {
        float t = sun.transform.forward.y;
        // -1〜1 → 0〜1
        t = t * 0.5f + 0.5f;
        Color col;

        if (t < 0.25f){
            col = dayColor;
            Debug.Log("eveningColor selected");
        }
        else if (t < 0.5f){
            col = dayColor;
            Debug.Log("dayColor selected");
        }
        else if (t < 0.7f){
            col = morningColor;
            Debug.Log("morningColor selected");
        }
        else {
            col = nightColor;
            Debug.Log("nightColor selected");
        }

        RenderSettings.skybox.SetColor("_Tint", col);
    }
}