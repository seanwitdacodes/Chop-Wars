using UnityEngine;

/// <summary>One continuous soundtrack across menus and runs, controlled by Settings' master sound toggle.</summary>
[RequireComponent(typeof(AudioSource))]
public sealed class BackgroundMusic : MonoBehaviour
{
    private static BackgroundMusic instance;
    private AudioSource source;
    private bool started;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        if (instance == null)
            new GameObject("Jungle Background Music").AddComponent<BackgroundMusic>();
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = true;
        source.spatialBlend = 0f;
        source.volume = 0.35f;
        source.clip = Resources.Load<AudioClip>("JungleGroove");
        if (source.clip == null)
            Debug.LogWarning("Background music clip is missing.", this);
    }

    private void Start()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        StartMusic();
#endif
    }

    private void Update()
    {
        // Browsers require a player interaction before audio can start.
        // Holding the initial tap also catches the frame after the browser unlocks audio.
        if (!started && (Input.anyKeyDown || Input.GetMouseButton(0) || Input.touchCount > 0))
            StartMusic();
    }

    private void StartMusic()
    {
        if (started || source == null || source.clip == null)
            return;

        source.Play();
        started = true;
    }
}
