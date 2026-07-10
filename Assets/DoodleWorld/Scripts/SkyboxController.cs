using UnityEngine;

public class SkyColorController : MonoBehaviour
{
    public Light sun;
    public Material skyboxMat;

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
        }
        else if (t < 0.5f){
            col = dayColor;
        }
        else if (t < 0.7f){
            col = morningColor;
        }
        else {
            col = nightColor;
        }

        RenderSettings.skybox.SetColor("_Tint", col);
    }
}