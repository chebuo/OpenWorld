using UnityEngine;

public class SunDrawer : MonoBehaviour
{
    public Light sun;
    public float distance = 1000f;

    Camera mainCamera;
    void Start()
    {
        mainCamera = Camera.main;
    }
    void Update()
    {
        if(sun == null) return;
        Vector3 dir = -sun.transform.forward;
        transform.position = mainCamera.transform.position + dir * distance;

        transform.LookAt(mainCamera.transform);
    }
}