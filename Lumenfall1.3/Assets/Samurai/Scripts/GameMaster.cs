using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public string respawnScene = "";
    public Vector3 respawnPos = Vector3.zero;
}

[System.Serializable]
public class GameWorldData
{
    public List<string> visitedRooms = new List<string>();
}

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;

    public PlayerData playerData;
    public GameWorldData worldData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

            playerData = new PlayerData();
            worldData = new GameWorldData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
