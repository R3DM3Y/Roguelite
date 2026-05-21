using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Follow")]
    public float smoothSpeed = 8f;

    [Header("Offset")]
    public Vector3 offset;

    private float minX;
    private float maxX;

    private float minY;
    private float maxY;

    private float camHalfWidth;
    private float camHalfHeight;

    private bool instantMove;

    private Camera cam;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 targetPos =
            target.position + offset;

        targetPos.z = transform.position.z;

        targetPos.x =
            Mathf.Clamp(
                targetPos.x,
                minX,
                maxX
            );

        targetPos.y =
            Mathf.Clamp(
                targetPos.y,
                minY,
                maxY
            );

        if (instantMove)
        {
            transform.position = targetPos;
            instantMove = false;
            return;
        }

        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPos,
                smoothSpeed * Time.deltaTime
            );
    }

    public void SetBounds(BoxCollider2D bounds)
    {
        if (cam == null)
        {
            cam = Camera.main;
        }

        camHalfHeight =
            cam.orthographicSize;

        camHalfWidth =
            cam.aspect * camHalfHeight;

        Bounds b = bounds.bounds;

        minX =
            b.min.x + camHalfWidth;

        maxX =
            b.max.x - camHalfWidth;

        minY =
            b.min.y + camHalfHeight;

        maxY =
            b.max.y - camHalfHeight;
    }

    public void InstantSnap()
    {
        instantMove = true;
    }
}