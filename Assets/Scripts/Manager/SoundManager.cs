using UnityEngine;

[DefaultExecutionOrder(-40)]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    
    public AudioSource seSource;
    public AudioSource bgmSource;

    [Header("Sound Effects")]
    public AudioClip attackSE;
    public AudioClip healSE;
    public AudioClip selectSE;
    public AudioClip enemyDeathSE;
    
    [Header("BGM")]
    public AudioClip bgmMain;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // BGM再生開始
        PlayBGM(bgmMain);
    }

    public void PlaySound(string type)
    {
        switch (type)
        {
            case "Attack": 
                if (seSource != null && attackSE != null)
                    seSource.PlayOneShot(attackSE); 
                break;
            case "Heal": 
                if (seSource != null && healSE != null)
                    seSource.PlayOneShot(healSE); 
                break;
            case "Select": 
                if (seSource != null && selectSE != null)
                    seSource.PlayOneShot(selectSE); 
                break;
            case "Death": 
                if (seSource != null && enemyDeathSE != null)
                    seSource.PlayOneShot(enemyDeathSE); 
                break;
        }
    }

    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource == null || clip == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.volume = 0.5f;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }
} 