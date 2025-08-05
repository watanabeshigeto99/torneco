using UnityEngine;
using System;

/// <summary>
/// システム統合専用コンポーネント
/// 責務：各システム間の統合、イベント中継のみ
/// </summary>
[DefaultExecutionOrder(-80)]
public class SystemIntegrationManager : MonoBehaviour
{
    public static SystemIntegrationManager Instance { get; private set; }
    
    [Header("System Integration")]
    public GameStateManager gameStateManager;
    public PlayerDataManager playerDataManager;
    public FloorManager floorManager;
    public SaveSystem.SaveManager saveManager;
    public DeckSystem.DeckManager deckManager;
    public UISystem.UIManager uiManager;
    
    [Header("Event Channels")]
    public SaveSystem.SaveEventChannel saveEventChannel;
    public DeckSystem.DeckEventChannel deckEventChannel;
    public UISystem.UIEventChannel uiEventChannel;
    
    // システム統合イベント
    public static event Action OnSystemsInitialized;
    public static event Action OnSystemDataChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeSystemIntegration();
    }
    
    /// <summary>
    /// システム統合の初期化
    /// </summary>
    private void InitializeSystemIntegration()
    {
        // 各システムの参照を取得
        if (gameStateManager == null)
            gameStateManager = GameStateManager.Instance;
        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance;
        if (floorManager == null)
            floorManager = FloorManager.Instance;
        
        // イベントの購読
        SubscribeToSystemEvents();
        
        Debug.Log("SystemIntegrationManager: システム統合を初期化しました");
        
        OnSystemsInitialized?.Invoke();
    }
    
    /// <summary>
    /// システムイベントの購読
    /// </summary>
    private void SubscribeToSystemEvents()
    {
        // ゲーム状態イベント
        if (gameStateManager != null)
        {
            GameStateManager.OnGameOver += OnGameOver;
            GameStateManager.OnGameClear += OnGameClear;
            GameStateManager.OnScoreChanged += OnScoreChanged;
        }
        
        // プレイヤーデータイベント
        if (playerDataManager != null)
        {
            PlayerDataManager.OnPlayerLevelUp += OnPlayerLevelUp;
            PlayerDataManager.OnPlayerExpGained += OnPlayerExpGained;
            PlayerDataManager.OnPlayerHPChanged += OnPlayerHPChanged;
        }
        
        // 階層システムイベント
        if (floorManager != null)
        {
            FloorManager.OnFloorChanged += OnFloorChanged;
            FloorManager.OnGameClear += OnFloorGameClear;
        }
    }
    
    /// <summary>
    /// システムイベントの購読解除
    /// </summary>
    private void UnsubscribeFromSystemEvents()
    {
        if (gameStateManager != null)
        {
            GameStateManager.OnGameOver -= OnGameOver;
            GameStateManager.OnGameClear -= OnGameClear;
            GameStateManager.OnScoreChanged -= OnScoreChanged;
        }
        
        if (playerDataManager != null)
        {
            PlayerDataManager.OnPlayerLevelUp -= OnPlayerLevelUp;
            PlayerDataManager.OnPlayerExpGained -= OnPlayerExpGained;
            PlayerDataManager.OnPlayerHPChanged -= OnPlayerHPChanged;
        }
        
        if (floorManager != null)
        {
            FloorManager.OnFloorChanged -= OnFloorChanged;
            FloorManager.OnGameClear -= OnFloorGameClear;
        }
    }
    
    // イベントハンドラー
    
    /// <summary>
    /// ゲームオーバー時の処理
    /// </summary>
    private void OnGameOver()
    {
        Debug.Log("SystemIntegrationManager: ゲームオーバーを検知しました");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.AddLog("ゲームオーバー！");
        }
        
        // サウンドシステムに通知
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene("GameOverScene");
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// ゲームクリア時の処理
    /// </summary>
    private void OnGameClear()
    {
        Debug.Log("SystemIntegrationManager: ゲームクリアを検知しました");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.AddLog("ゲームクリア！");
        }
        
        // サウンドシステムに通知
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene("GameClearScene");
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// スコア変更時の処理
    /// </summary>
    /// <param name="newScore">新しいスコア</param>
    private void OnScoreChanged(int newScore)
    {
        Debug.Log($"SystemIntegrationManager: スコア変更を検知しました - {newScore}");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.UpdateScore(newScore);
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// プレイヤーレベルアップ時の処理
    /// </summary>
    /// <param name="newLevel">新しいレベル</param>
    private void OnPlayerLevelUp(int newLevel)
    {
        Debug.Log($"SystemIntegrationManager: プレイヤーレベルアップを検知しました - レベル{newLevel}");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.UpdateLevelDisplay(newLevel, playerDataManager.playerExp, playerDataManager.playerExpToNext);
            uiManager.AddLog($"レベルアップ！レベル {newLevel}");
        }
        
        // サウンドシステムに通知
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("LevelUp");
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// プレイヤー経験値獲得時の処理
    /// </summary>
    /// <param name="expAmount">獲得経験値</param>
    private void OnPlayerExpGained(int expAmount)
    {
        Debug.Log($"SystemIntegrationManager: プレイヤー経験値獲得を検知しました - +{expAmount}");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.AddLog($"経験値獲得！+{expAmount}");
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// プレイヤーHP変更時の処理
    /// </summary>
    /// <param name="currentHP">現在のHP</param>
    /// <param name="maxHP">最大HP</param>
    private void OnPlayerHPChanged(int currentHP, int maxHP)
    {
        Debug.Log($"SystemIntegrationManager: プレイヤーHP変更を検知しました - {currentHP}/{maxHP}");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.UpdateHP(currentHP, maxHP);
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// 階層変更時の処理
    /// </summary>
    /// <param name="newFloor">新しい階層</param>
    private void OnFloorChanged(int newFloor)
    {
        Debug.Log($"SystemIntegrationManager: 階層変更を検知しました - 階層{newFloor}");
        
        // UIシステムに通知
        if (uiManager != null)
        {
            uiManager.UpdateFloor(newFloor);
            uiManager.AddLog($"階層 {newFloor} に進みました");
        }
        
        OnSystemDataChanged?.Invoke();
    }
    
    /// <summary>
    /// 階層システムからのゲームクリア処理
    /// </summary>
    private void OnFloorGameClear()
    {
        Debug.Log("SystemIntegrationManager: 階層システムからのゲームクリアを検知しました");
        
        // ゲーム状態をクリア状態に設定
        if (gameStateManager != null)
        {
            gameStateManager.Victory();
        }
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromSystemEvents();
    }
    
    /// <summary>
    /// システム統合の情報を取得
    /// </summary>
    /// <returns>システム統合の情報文字列</returns>
    public string GetSystemIntegrationInfo()
    {
        return $"SystemIntegration - GameState: {(gameStateManager != null ? "✓" : "✗")}, " +
               $"PlayerData: {(playerDataManager != null ? "✓" : "✗")}, " +
               $"Floor: {(floorManager != null ? "✓" : "✗")}, " +
               $"Save: {(saveManager != null ? "✓" : "✗")}, " +
               $"Deck: {(deckManager != null ? "✓" : "✗")}, " +
               $"UI: {(uiManager != null ? "✓" : "✗")}";
    }
} 