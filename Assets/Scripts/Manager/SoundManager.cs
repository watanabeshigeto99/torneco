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
    
    [Header("Additional Sound Effects")]
    public AudioClip buttonClickSE;
    public AudioClip cardHoverSE;
    public AudioClip cardAddSE;
    public AudioClip cardRemoveSE;
    public AudioClip errorSE;
    
    [Header("BGM")]
    public AudioClip bgmMain;
    public AudioClip bgmDeckBuilder; // デッキビルダー用BGM
    public AudioClip bgmBattle; // バトル用BGM
    public AudioClip bgmGameOver; // ゲームオーバー用BGM
    public AudioClip bgmGameClear; // ゲームクリア用BGM

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
        // まず既存のSEをチェック
        AudioClip clipToPlay = null;
        
        switch (type)
        {
            case "Attack": 
                clipToPlay = attackSE;
                break;
            case "Heal": 
                clipToPlay = healSE;
                break;
            case "Select": 
                clipToPlay = selectSE;
                break;
            case "Death": 
                clipToPlay = enemyDeathSE;
                break;
            case "LevelUp": 
                clipToPlay = levelUpSE;
                break;
            case "Enhance": 
                clipToPlay = enhanceSE;
                break;
            case "ButtonClick": 
                clipToPlay = buttonClickSE;
                break;
            case "card_hover": 
                clipToPlay = cardHoverSE;
                break;
            case "アイテムを入手2": 
                clipToPlay = cardAddSE;
                break;
            case "Error": 
                clipToPlay = errorSE;
                break;
            default:
                // 既存のSEにない場合はResourcesフォルダから読み込み
                clipToPlay = Resources.Load<AudioClip>($"Sounds/SE/{type}");
                break;
        }
        
        if (clipToPlay != null && seSource != null)
        {
            seSource.PlayOneShot(clipToPlay);
            Debug.Log($"SoundManager: SE '{type}' を再生");
        }
        else
        {
            Debug.LogError($"SoundManager: SE '{type}' が見つかりません");
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
    
    /// <summary>
    /// 文字列で指定されたBGMを再生
    /// </summary>
    public void PlayBGM(string bgmName)
    {
        AudioClip bgmClip = null;
        
        // インスペクターで設定されたBGMをチェック
        switch (bgmName)
        {
            case "街の商人":
                bgmClip = bgmDeckBuilder;
                break;
            default:
                // ResourcesフォルダからBGMを読み込み
                bgmClip = Resources.Load<AudioClip>($"Sounds/BGM/{bgmName}");
                break;
        }
        
        if (bgmClip != null)
        {
            PlayBGM(bgmClip);
            Debug.Log($"SoundManager: BGM '{bgmName}' を再生開始");
        }
        else
        {
            Debug.LogError($"SoundManager: BGM '{bgmName}' が見つかりません");
        }
    }

    public void StopBGM()
    {
        if (bgmSource != null)
            bgmSource.Stop();
    }
    
    /// <summary>
    /// シーンに応じたBGMを再生
    /// </summary>
    public void PlayBGMForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "DeckBuilderScene":
                if (bgmDeckBuilder != null)
                {
                    PlayBGM(bgmDeckBuilder);
                    Debug.Log("SoundManager: デッキビルダーBGMを再生");
                }
                break;
                
            case "MainScene":
                if (bgmBattle != null)
                {
                    PlayBGM(bgmBattle);
                    Debug.Log("SoundManager: バトルBGMを再生");
                }
                else if (bgmMain != null)
                {
                    PlayBGM(bgmMain);
                    Debug.Log("SoundManager: メインBGMを再生");
                }
                break;
                
            case "GameOverScene":
                if (bgmGameOver != null)
                {
                    PlayBGM(bgmGameOver);
                    Debug.Log("SoundManager: ゲームオーバーBGMを再生");
                }
                break;
                
            case "GameClearScene":
                if (bgmGameClear != null)
                {
                    PlayBGM(bgmGameClear);
                    Debug.Log("SoundManager: ゲームクリアBGMを再生");
                }
                break;
                
            default:
                if (bgmMain != null)
                {
                    PlayBGM(bgmMain);
                    Debug.Log($"SoundManager: デフォルトBGMを再生 (シーン: {sceneName})");
                }
                break;
        }
    }
    
    /// <summary>
    /// 現在のシーンのBGMを再生
    /// </summary>
    public void PlayBGMForCurrentScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        PlayBGMForScene(currentSceneName);
    }
} 