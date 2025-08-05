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
    
    [Header("New Improvement Managers")]
    public AsyncOperationManager asyncOperationManager;
    public StateChangeManager stateChangeManager;
    public CompatibilityManager compatibilityManager;
    
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
        
        // 新しい改善Managerの参照を取得
        if (asyncOperationManager == null)
            asyncOperationManager = AsyncOperationManager.Instance;
        if (stateChangeManager == null)
            stateChangeManager = StateChangeManager.Instance;
        if (compatibilityManager == null)
            compatibilityManager = CompatibilityManager.Instance;
        
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
            FloorManager.OnGameClear += OnFloorGameClear;
        }
        
        // 新しい改善Managerのイベント
        if (asyncOperationManager != null)
        {
            AsyncOperationManager.OnAllOperationsCompleted += OnAllAsyncOperationsCompleted;
        }
        
        if (stateChangeManager != null)
        {
            StateChangeManager.OnStateChanged += OnStateChanged;
        }
        
        if (compatibilityManager != null)
        {
            CompatibilityManager.OnAllCompatibilityChecksCompleted += OnAllCompatibilityChecksCompleted;
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
            FloorManager.OnGameClear -= OnFloorGameClear;
        }
        
        if (asyncOperationManager != null)
        {
            AsyncOperationManager.OnAllOperationsCompleted -= OnAllAsyncOperationsCompleted;
        }
        
        if (stateChangeManager != null)
        {
            StateChangeManager.OnStateChanged -= OnStateChanged;
        }
        
        if (compatibilityManager != null)
        {
            CompatibilityManager.OnAllCompatibilityChecksCompleted -= OnAllCompatibilityChecksCompleted;
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
    
    /// <summary>
    /// 階層ゲームクリア時の処理
    /// </summary>
    private void OnFloorGameClear()
    {
        Debug.Log("GameManagerNew: 階層ゲームクリアを検知しました");
        
        // セーブシステムに通知
        if (saveManager != null)
        {
            saveManager.SaveGame();
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
    /// 全ての非同期処理完了時の処理
    /// </summary>
    private void OnAllAsyncOperationsCompleted()
    {
        Debug.Log("GameManagerNew: 全ての非同期処理が完了しました");
        
        // ゲーム状態の更新を通知
        OnGameStateChanged?.Invoke();
    }
    
    /// <summary>
    /// 状態変化時の処理
    /// </summary>
    private void OnStateChanged(string stateId, object oldValue, object newValue)
    {
        Debug.Log($"GameManagerNew: 状態変化を検知しました - {stateId}: {oldValue} -> {newValue}");
        
        // 重要な状態変化の場合はゲーム状態を更新
        if (IsImportantStateChange(stateId))
        {
            OnGameStateChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// 全ての互換性チェック完了時の処理
    /// </summary>
    private void OnAllCompatibilityChecksCompleted()
    {
        Debug.Log("GameManagerNew: 全ての互換性チェックが完了しました");
        
        // 互換性レポートを生成
        if (compatibilityManager != null)
        {
            string report = compatibilityManager.GenerateCompatibilityReport();
            Debug.Log($"Compatibility Report:\n{report}");
        }
    }
    
    /// <summary>
    /// 重要な状態変化かどうかを判定
    /// </summary>
    /// <param name="stateId">状態ID</param>
    /// <returns>重要な状態変化かどうか</returns>
    private bool IsImportantStateChange(string stateId)
    {
        string[] importantStates = {
            "PlayerHP", "PlayerLevel", "PlayerExp", "Score", "Floor", "GameOver", "GameClear"
        };
        
        foreach (var importantState in importantStates)
        {
            if (stateId.Contains(importantState))
                return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 新機能追加前の互換性チェック
    /// </summary>
    /// <param name="newFeatureId">新機能ID</param>
    /// <param name="dependencies">依存関係</param>
    /// <returns>互換性チェック結果</returns>
    public CompatibilityManager.CompatibilityCheckResult CheckNewFeatureCompatibility(string newFeatureId, string[] dependencies)
    {
        if (compatibilityManager != null)
        {
            return compatibilityManager.CheckNewFeatureCompatibility(newFeatureId, dependencies);
        }
        
        Debug.LogWarning("GameManagerNew: CompatibilityManagerが見つかりません");
        return null;
    }
    
    /// <summary>
    /// 非同期処理をキューに追加
    /// </summary>
    /// <param name="operationId">処理ID</param>
    /// <param name="operation">実行するコルーチン</param>
    /// <param name="timeout">タイムアウト時間</param>
    /// <param name="onSuccess">成功時のコールバック</param>
    /// <param name="onFailure">失敗時のコールバック</param>
    /// <param name="isPriority">優先度</param>
    public void QueueAsyncOperation(string operationId, System.Func<System.Collections.IEnumerator> operation, float timeout = -1, System.Action onSuccess = null, System.Action<string> onFailure = null, bool isPriority = false)
    {
        if (asyncOperationManager != null)
        {
            asyncOperationManager.QueueOperation(operationId, operation, timeout, onSuccess, onFailure, isPriority);
        }
        else
        {
            Debug.LogWarning("GameManagerNew: AsyncOperationManagerが見つかりません");
        }
    }
    
    /// <summary>
    /// 状態変化を記録
    /// </summary>
    /// <param name="stateId">状態ID</param>
    /// <param name="oldValue">古い値</param>
    /// <param name="newValue">新しい値</param>
    /// <param name="source">変化の原因</param>
    /// <param name="description">説明</param>
    public void RecordStateChange(string stateId, object oldValue, object newValue, string source, string description = "")
    {
        if (stateChangeManager != null)
        {
            stateChangeManager.RecordStateChange(stateId, oldValue, newValue, source, description);
        }
        else
        {
            Debug.LogWarning("GameManagerNew: StateChangeManagerが見つかりません");
        }
    }
    
    /// <summary>
    /// ゲーム情報を取得（改善版）
    /// </summary>
    public string GetGameInfo()
    {
        var info = "=== GameManagerNew Info ===\n";
        
        // 基本システム情報
        info += $"GameStateManager: {(gameStateManager != null ? "Active" : "Inactive")}\n";
        info += $"PlayerDataManager: {(playerDataManager != null ? "Active" : "Inactive")}\n";
        info += $"FloorManager: {(floorManager != null ? "Active" : "Inactive")}\n";
        info += $"SystemIntegrationManager: {(systemIntegrationManager != null ? "Active" : "Inactive")}\n";
        
        // 新しい改善Manager情報
        info += $"AsyncOperationManager: {(asyncOperationManager != null ? "Active" : "Inactive")}\n";
        info += $"StateChangeManager: {(stateChangeManager != null ? "Active" : "Inactive")}\n";
        info += $"CompatibilityManager: {(compatibilityManager != null ? "Active" : "Inactive")}\n";
        
        // 非同期処理情報
        if (asyncOperationManager != null)
        {
            info += $"\nAsync Operations:\n{asyncOperationManager.GetDebugInfo()}\n";
        }
        
        // 状態変化情報
        if (stateChangeManager != null)
        {
            info += $"\nState Changes:\n{stateChangeManager.GetDebugInfo()}\n";
        }
        
        // 互換性情報
        if (compatibilityManager != null)
        {
            info += $"\nCompatibility:\n{compatibilityManager.GetDebugInfo()}\n";
        }
        
        return info;
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
} 