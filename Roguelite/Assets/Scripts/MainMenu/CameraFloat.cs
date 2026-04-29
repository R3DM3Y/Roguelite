using UnityEngine;

public class CameraFloat : MonoBehaviour
{
    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        transform.position = startPos +
                             new Vector3(
                                 Mathf.Sin(Time.time * 0.3f) * 0.1f,
                                 Mathf.Cos(Time.time * 0.2f) * 0.05f,
                                 0);
    }
}