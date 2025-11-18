using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    void Update()
    {
        if (transform.position.y < -10f) // ejemplo de "caer del mapa"
        {
            Respawn();
        }
    }

    public void Respawn()
    {
        string scene = GameMaster.instance.playerData.respawnScene;

        if (scene != "" && SceneManager.GetActiveScene().name != scene)
        {
            SceneManager.LoadScene(scene);
        }
        else
        {
            transform.position = GameMaster.instance.playerData.respawnPos;
        }
    }
}
