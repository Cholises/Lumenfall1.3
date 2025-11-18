using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    [Header("LevelTransition")]
    public Animator anim;
    public float transitionTime = 1f;
    public static LevelLoader instance;
    public string spawnPosName = "";

    private bool doRespawn;
    public string respawnScene = "";
    public Vector3 respawnPos = Vector3.zero;

    private Samurai player;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            player = FindFirstObjectByType<Samurai>();
            SpawnPointInit();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += SceneChange;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneChange;
    }

    public void SceneChange(Scene scene, LoadSceneMode mode)
    {
        player = FindFirstObjectByType<Samurai>();

        if (player == null)
        {
            Debug.LogWarning("Samurai no encontrado después de cargar la escena.");
            return;
        }

        UpdatePlayerPosition();
    }

    private void SpawnPointInit()
    {
        player = FindFirstObjectByType<Samurai>();

        if (player == null)
        {
            Debug.LogWarning("Samurai no encontrado en SpawnPointInit");
            return;
        }

        if (respawnScene == "")
        {
            respawnScene = SceneManager.GetActiveScene().name;
            respawnPos = player.transform.position;
            Debug.Log("Initialized respawn: " + respawnScene + " at " + respawnPos);
        }
        else
        {
            doRespawn = true;

            if (Application.isEditor)
                respawnPos = player.transform.position;

            UpdatePlayerPosition();
        }
    }

    private void UpdatePlayerPosition()
    {
        player = FindFirstObjectByType<Samurai>();

        if (player == null)
        {
            Debug.LogWarning("Samurai no encontrado en UpdatePlayerPosition");
            return;
        }

        if (doRespawn)
        {
            player.transform.position = respawnPos;

            if (player.rb != null)
                player.rb.bodyType = RigidbodyType2D.Dynamic;

            doRespawn = false;
        }
        else if (!string.IsNullOrEmpty(spawnPosName))
        {
            GameObject spawnObj = GameObject.Find(spawnPosName);
            if (spawnObj != null)
                player.transform.position = spawnObj.transform.position;

            spawnPosName = "";

            if (player.rb != null)
                player.rb.gravityScale = player.originalGravityScale;
        }

        player.disableControlCounter = Mathf.Max(0, player.disableControlCounter - 1);
    }

    public void LoadLevel(string sceneName, string spawnPosName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("❌ LoadLevel llamado con un nombre de escena vacío.");
            return;
        }

        this.spawnPosName = spawnPosName;

        if (player != null && player.rb != null)
        {
            player.disableControlCounter += 1;
            player.rb.gravityScale = 0f;
        }

        StartCoroutine(LoadingScreen(sceneName));
    }

    private IEnumerator LoadingScreen(string sceneName)
    {
        if(anim != null)
            anim.SetBool("changeScene", true);

        yield return new WaitForSeconds(transitionTime);

        SceneManager.LoadScene(sceneName);

        if(anim != null)
            anim.SetBool("changeScene", false);
    }
}
