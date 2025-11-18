using UnityEngine;

public class RespawnZone : MonoBehaviour
{
    public Vector3 respawnPosition;
    public string sceneName;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameMaster.instance.playerData.respawnPos = respawnPosition;
            GameMaster.instance.playerData.respawnScene = sceneName;
        }
    }
}
