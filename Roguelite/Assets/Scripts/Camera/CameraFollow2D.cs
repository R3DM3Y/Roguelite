using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform target;           // Игрок

    [Header("Camera Settings")]
    public float smoothSpeed = 0.3f; // Чем меньше, тем плавнее движение
    public Vector2 offset;             // Смещение камеры относительно игрока

    [Header("Clamp (optional)")]
    public Vector2 minPosition;        // Минимальные X и Y камеры
    public Vector2 maxPosition;        // Максимальные X и Y камеры
    public bool useClamp = false;      // Включить/выключить ограничение движения камеры

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            target.position.y + offset.y,
            transform.position.z
        );

        if (useClamp)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minPosition.x, maxPosition.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minPosition.y, maxPosition.y);
        }

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }
}