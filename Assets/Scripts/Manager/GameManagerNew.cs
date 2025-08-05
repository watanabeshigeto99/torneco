using UnityEngine;
using System;

/// <summary>
/// 新しいGameManager（統合版）
/// 責務：各システムの統合とゲーム全体の制御のみ
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManagerNew : MonoBehaviour
{
    public static GameManagerNew Instance { get; private set; }
    
    [Header("System Managers")]
    public GameStateManager gameStateManager;
    public PlayerDataManager playerDataManager;
    public FloorManager floorManager;
    public SystemIntegrationManager systemIntegrationManager;
    
    [Header("Template Systems")]
    public BattleSystemTemplate.BattleStarter battleStarter;
    public SaveSystem.SaveManager saveManager;
    public DeckSystem.DeckManager deckManager;
    public UISystem.UIManager uiManager;
    
    [Header("Event Channels")]
    public SaveSystem.SaveEventChannel saveEventChannel;
    public DeckSystem.DeckEventChannel deckEventChannel;
    public UISystem.UIEventChannel uiEventChannel;
    public BattleSystemTemplate.BattleEventChannel battleEventChannel;
    
    // ゲーム統合イベント
    public static event Action OnGameInitialized;
    public static event Action OnGameStateChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeGame();
    }
    
    /// <summary>
    /// ゲームの初期化
    /// </summary>
    private void InitializeGame()
    {
        // 各システムの参照を取得
        if (gameStateManager == null)
            gameStateManager = GameStateManager.Instance;
        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance;
        if (floorManager == null)
            floorManager = FloorManager.Instance;
        if (systemIntegrationManager == null)
            systemIntegrationManager = SystemIntegrationManager.Instance;
        
        // イベントの購読
        SubscribeToEvents();
        
        Debug.Log("GameManagerNew: ゲームを初期化しました");
        
        OnGameInitialized?.Invoke();
    }
    
    /// <summary>
    /// イベントの購読
    /// </summary>
    private void SubscribeToEvents()
    {
        // システム統合イベント
        if (systemIntegrationManager != null)
        {
            SystemIntegrationManager.OnSystemsInitialized += OnSystemsInitialized;
            SystemIntegrationManager.OnSystemDataChanged += OnSystemDataChanged;
        }
        
        // ゲーム状態イベント
        if (gameStateManager != null)
        {
            GameStateManager.OnGameOver += OnGameOver;
            GameStateManager.OnGameClear += OnGameClear;
        }
        
        // プレイヤーデータイベント
        if (playerDataManager != null)
        {
            PlayerDataManager.OnPlayerLevelUp += OnPlayerLevelUp;
            PlayerDataManager.OnPlayerExpGained += OnPlayerExpGained;
        }
        
        // 階層システムイベント
        if (floorManager != null)
        {
            FloorManager.OnFloorChanged += OnFloorChanged;
        }
    }
    
    /// <summary>
    /// イベントの購読解除
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (systemIntegrationManager != null)
        {
            SystemIntegrationManager.OnSystemsInitialized -= OnSystemsInitialized;
            SystemIntegrationManager.OnSystemDataChanged -= OnSystemDataChanged;
        }
        
        if (gameStateManager != null)
        {
            GameStateManager.OnGameOver -= OnGameOver;
            GameStateManager.OnGameClear -= OnGameClear;
        }
        
        if (playerDataManager != null)
        {
            PlayerDataManager.OnPlayerLevelUp -= OnPlayerLevelUp;
            PlayerDataManager.OnPlayerExpGained -= OnPlayerExpGained;
        }
        
        if (floorManager != null)
        {
            FloorManager.OnFloorChanged -= OnFloorChanged;
        }
    }
    
    // イベントハンドラー
    
    /// <summary>
    /// システム初期化完了時の処理
    /// </summary>
    private void OnSystemsInitialized()
    {
        Debug.Log("GameManagerNew: 全システムの初期化が完了しました");
        
        // ゲーム開始処理
        StartGame();
    }
    
    /// <summary>
    /// システムデータ変更時の処理
    /// </summary>
    private void OnSystemDataChanged()
    {
        OnGameStateChanged?.Invoke();
        
        Debug.Log("GameManagerNew: システムデータが変更されました");
    }
    
    /// <summary>
    /// ゲームオーバー時の処理
    /// </summary>
    private void OnGameOver()
    {
        Debug.Log("GameManagerNew: ゲームオーバーを検知しました");
        
        // セーブシステムに通知
        if (saveManager != null)
        {
            saveManager.SaveGame();
        }
    }
    
    /// <summary>
    /// ゲームクリア時の処理
    /// </summary>
    private void OnGameClear()
    {
        Debug.Log("GameManagerNew: ゲームクリアを検知しました");
        
        // セーブシステムに通知
        if (saveManager != null)
        {
            saveManager.SaveGame();
        }
    }
    
    /// <summary>
    /// プレイヤーレベルアップ時の処理
    /// </summary>
    /// <param name="newLevel">新しいレベル</param>
    private void OnPlayerLevelUp(int newLevel)
    {
        Debug.Log($"GameManagerNew: プレイヤーレベルアップを検知しました - レベル{newLevel}");
        
        // スコア加算
        if (gameStateManager != null)
        {
            gameStateManager.AddScore(100);
        }
    }
    
    /// <summary>
    /// プレイヤー経験値獲得時の処理
    /// </summary>
    /// <param name="expAmount">獲得経験値</param>
    private void OnPlayerExpGained(int expAmount)
    {
        Debug.Log($"GameManagerNew: プレイヤー経験値獲得を検知しました - +{expAmount}");
        
        // スコア加算
        if (gameStateManager != null)
        {
            gameStateManager.AddScore(10);
        }
    }
    
    /// <summary>
    /// 階層変更時の処理
    /// </summary>
    /// <param name="newFloor">新しい階層</param>
    private void OnFloorChanged(int newFloor)
    {
        Debug.Log($"GameManagerNew: 階層変更を検知しました - 階層{newFloor}");
        
        // セーブシステムに通知
        if (saveManager != null)
        {
            saveManager.AutoSave();
        }
    }
    
    // ゲーム制御メソッド
    
    /// <summary>
    /// ゲームを開始
    /// </summary>
    public void StartGame()
    {
        Debug.Log("GameManagerNew: ゲームを開始しました");
        
        // 初期データの設定
        if (gameStateManager != null)
        {
            gameStateManager.ResetGameState();
        }
        
        if (playerDataManager != null)
        {
            playerDataManager.ResetPlayerData();
        }
        
        if (floorManager != null)
        {
            floorManager.ResetFloor();
        }
    }
    
    /// <summary>
    /// ゲームをリセット
    /// </summary>
    public void ResetGame()
    {
        Debug.Log("GameManagerNew: ゲームをリセットしました");
        
        StartGame();
    }
    
    /// <summary>
    /// 戦闘を開始
    /// </summary>
    public void StartBattle()
    {
        if (battleStarter != null)
        {
            battleStarter.StartBattle();
            Debug.Log("GameManagerNew: 戦闘を開始しました");
        }
        else
        {
            Debug.LogWarning("GameManagerNew: battleStarterが設定されていません");
        }
    }
    
    /// <summary>
    /// セーブを実行
    /// </summary>
    public void SaveGame()
    {
        if (saveManager != null)
        {
            saveManager.SaveGame();
            Debug.Log("GameManagerNew: セーブを実行しました");
        }
        else
        {
            Debug.LogWarning("GameManagerNew: saveManagerが設定されていません");
        }
    }
    
    /// <summary>
    /// ロードを実行
    /// </summary>
    public void LoadGame()
    {
        if (saveManager != null)
        {
            saveManager.LoadGame();
            Debug.Log("GameManagerNew: ロードを実行しました");
        }
        else
        {
            Debug.LogWarning("GameManagerNew: saveManagerが設定されていません");
        }
    }
    
    /// <summary>
    /// 次の階層に進む
    /// </summary>
    public void GoToNextFloor()
    {
        if (floorManager != null)
        {
            floorManager.GoToNextFloor();
            Debug.Log("GameManagerNew: 次の階層に進みました");
        }
        else
        {
            Debug.LogWarning("GameManagerNew: floorManagerが設定されていません");
        }
    }
    
    /// <summary>
    /// プレイヤーに経験値を加算
    /// </summary>
    /// <param name="amount">加算する経験値</param>
    public void AddPlayerExp(int amount)
    {
        if (playerDataManager != null)
        {
            playerDataManager.AddPlayerExp(amount);
        }
        else
        {
            Debug.LogWarning("GameManagerNew: playerDataManagerが設定されていません");
        }
    }
    
    /// <summary>
    /// プレイヤーのHPを設定
    /// </summary>
    /// <param name="currentHP">現在のHP</param>
    /// <param name="maxHP">最大HP</param>
    public void SetPlayerHP(int currentHP, int maxHP = -1)
    {
        if (playerDataManager != null)
        {
            playerDataManager.SetPlayerHP(currentHP, maxHP);
        }
        else
        {
            Debug.LogWarning("GameManagerNew: playerDataManagerが設定されていません");
        }
    }
    
    /// <summary>
    /// スコアを加算
    /// </summary>
    /// <param name="amount">加算するスコア</param>
    public void AddScore(int amount)
    {
        if (gameStateManager != null)
        {
            gameStateManager.AddScore(amount);
        }
        else
        {
            Debug.LogWarning("GameManagerNew: gameStateManagerが設定されていません");
        }
    }
    
    /// <summary>
    /// ゲーム全体の情報を取得
    /// </summary>
    /// <returns>ゲーム全体の情報文字列</returns>
    public string GetGameInfo()
    {
        return $"GameManagerNew - GameState: {(gameStateManager != null ? gameStateManager.GetGameStateInfo() : "✗")}\n" +
               $"PlayerData: {(playerDataManager != null ? playerDataManager.GetPlayerDataInfo() : "✗")}\n" +
               $"Floor: {(floorManager != null ? floorManager.GetFloorInfo() : "✗")}\n" +
               $"SystemIntegration: {(systemIntegrationManager != null ? systemIntegrationManager.GetSystemIntegrationInfo() : "✗")}";
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
} 