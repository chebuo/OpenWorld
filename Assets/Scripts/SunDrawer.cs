using UnityEngine;

public class SunDrawer : MonoBehaviour
{
    public Light sun;
    public float distance = 1000f;

    void Update()
    {
        Vector3 dir = -sun.transform.forward;
        transform.position = Camera.main.transform.position + dir * distance;

        transform.LookAt(Camera.main.transform);
    }
}