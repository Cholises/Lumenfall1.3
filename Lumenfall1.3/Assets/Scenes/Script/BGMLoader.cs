using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BGMLoader : MonoBehaviour
{
    public static BGMLoader instance;

    [Header("Audio")]
    public AudioSource bgm;

    [Header("Fade")]
    public float fadeTime = 1f;

    [Header("Música por tipo de escena")]
    public AudioClip musicaDirt;
    public AudioClip musicaJefe;      // DirtCave8
    public AudioClip musicaMossy;

    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            // Reproducir música de la escena inicial si aplica
            PlayMusicForScene(SceneManager.GetActiveScene().name);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip targetClip = null;

        // Escena sin música: MainMenu
        if (sceneName == "MainMenu")
        {
            if (bgm.isPlaying)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeOut(bgm, fadeTime));
            }
            return;
        }

        // DirtCave escenas
        if (sceneName.StartsWith("DirtCave"))
        {
            if (sceneName == "DirtCave8" && musicaJefe != null)
            {
                targetClip = musicaJefe;
            }
            else
            {
                targetClip = musicaDirt;
            }
        }
        // MossyCavern escenas
        else if (sceneName.StartsWith("MossyCavern"))
        {
            targetClip = musicaMossy;
        }

        if (targetClip != null)
        {
            if (bgm.clip != targetClip)
            {
                if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
                fadeCoroutine = StartCoroutine(FadeChange(bgm, targetClip, fadeTime));
            }
            else if (!bgm.isPlaying)
            {
                // Si ya es el clip pero estaba detenido → reproducir
                bgm.volume = 1f;
                bgm.Play();
            }
        }
    }

    private IEnumerator FadeChange(AudioSource audioSource, AudioClip newClip, float time)
    {
        yield return FadeOut(audioSource, time / 2);

        audioSource.clip = newClip;
        audioSource.loop = true;
        audioSource.Play();

        yield return FadeIn(audioSource, time / 2);
    }

    private IEnumerator FadeOut(AudioSource audioSource, float time)
    {
        float startVolume = audioSource.volume;
        float t = 0;

        while (t < time)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0, t / time);
            yield return null;
        }

        audioSource.volume = 0;
        audioSource.Stop();
    }

    private IEnumerator FadeIn(AudioSource audioSource, float time)
    {
        float t = 0;
        audioSource.volume = 0;
        audioSource.Play();

        while (t < time)
        {
            t += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0, 1f, t / time);
            yield return null;
        }

        audioSource.volume = 1f;
    }

    private void OnDestroy()
    {
        if (instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
