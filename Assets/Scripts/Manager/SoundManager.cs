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
    public AudioClip levelUpSE;
    public AudioClip enhanceSE; // カード強化効果音
    
    [Header("BGM")]
    public AudioClip bgmMain;

    private void Awake()
    {
        Debug.Log("SoundManager: Awake開始");
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // AudioSourceコンポーネントの取得とキャッシュ
            seSource = GetComponent<AudioSource>();
            if (seSource == null)
            {
                seSource = gameObject.AddComponent<AudioSource>();
            }
            
            // BGM用のAudioSourceを追加
            GameObject bgmObject = new GameObject("BGM Source");
            bgmObject.transform.SetParent(transform);
            bgmSource = bgmObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            
            Debug.Log("SoundManager: AudioSource設定完了");
        }
        else 
        {
            Debug.LogWarning("SoundManager: 重複するSoundManagerインスタンスを破棄");
            Destroy(gameObject);
        }
        
        Debug.Log("SoundManager: Awake完了");
    }

    private void Start()
    {
        Debug.Log("SoundManager: Start開始");
        
        // BGM再生開始
        if (bgmMain != null)
        {
            PlayBGM(bgmMain);
            Debug.Log("SoundManager: BGM再生開始");
        }
        else
        {
            Debug.LogWarning("SoundManager: bgmMainが設定されていません");
        }
        
        Debug.Log("SoundManager: Start完了");
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
            case "LevelUp": 
                if (seSource != null && levelUpSE != null)
                    seSource.PlayOneShot(levelUpSE); 
                break;
            case "Enhance": 
                if (seSource != null && enhanceSE != null)
                    seSource.PlayOneShot(enhanceSE); 
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