using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMLoader : MonoBehaviour
{
    public AudioSource bgm;
    public static BGMLoader instance;

    private void Awake()
    {
        string escenaActual = SceneManager.GetActiveScene().name;
        string clipNombre = bgm.clip != null ? bgm.clip.name : "NULL";
        
        Debug.Log($"=== BGMLoader Awake ===");
        Debug.Log($"Escena: {escenaActual}");
        Debug.Log($"Clip asignado: {clipNombre}");
        
        if (instance == null)
        {
            Debug.Log("✓ Primera instancia - Creando singleton");
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            
            if (bgm.clip != null)
            {
                bgm.Play();
                Debug.Log($"♪ Reproduciendo: {bgm.clip.name}");
            }
        }
        else
        {
            string clipActual = instance.bgm.clip != null ? instance.bgm.clip.name : "NULL";
            Debug.Log($"✗ Instancia duplicada detectada");
            Debug.Log($"Clip en singleton: {clipActual}");
            Debug.Log($"Clip en duplicado: {clipNombre}");
            
            if (this.bgm.clip != null && instance.bgm.clip != this.bgm.clip)
            {
                Debug.Log($"♪ CAMBIANDO MÚSICA: {clipActual} → {clipNombre}");
                instance.bgm.Stop();
                instance.bgm.clip = this.bgm.clip;
                instance.bgm.Play();
            }
            else
            {
                Debug.Log("→ Mismo clip, no se cambia la música");
            }
            
            Debug.Log("× Destruyendo instancia duplicada");
            Destroy(this.gameObject);
        }
        
        Debug.Log("=======================\n");
    }
}