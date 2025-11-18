using UnityEngine;

public class ChangeLevelTrigger : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre exacto de la escena a cargar (debe estar en Build Settings)")]
    public string targetSceneName = "DirtCave1";
    
    [Tooltip("Nombre del GameObject donde aparecerá el jugador (ejemplo: Entry0, Entry1)")]
    public string spawnPointName = "Entry0";
    
    [Header("Configuración de Trigger")]
    [Tooltip("Tag del jugador (normalmente 'Player')")]
    public string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Verificar si quien entró es el jugador
        if (other.CompareTag(playerTag))
        {
            Debug.Log($"🚪 Cambiando a escena: {targetSceneName}, spawn: {spawnPointName}");
            
            // Verificar que LevelLoader existe
            if (LevelLoader.instance != null)
            {
                LevelLoader.instance.LoadLevel(targetSceneName, spawnPointName);
            }
            else
            {
                Debug.LogError("❌ LevelLoader.instance es null. Asegúrate de que LevelLoader esté en la escena.");
            }
        }
    }

    // Para visualizar el trigger en el editor
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.DrawWireCube(transform.position + (Vector3)box.offset, box.size);
        }
    }
}