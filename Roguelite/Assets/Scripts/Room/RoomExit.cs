using UnityEngine;

public enum ExitDirection
{
    Left,
    Right,
    Up,
    Down
}

public class RoomExit : MonoBehaviour
{
    public ExitDirection direction;

    private bool canTeleport = true;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canTeleport)
            return;

        if (!other.CompareTag("Player"))
            return;

        RoomManager.Instance.ChangeRoom(direction);
    }

    public void DisableTemporarily()
    {
        StartCoroutine(DisableRoutine());
    }

    private System.Collections.IEnumerator DisableRoutine()
    {
        canTeleport = false;

        yield return new WaitForSeconds(0.3f);

        canTeleport = true;
    }
}