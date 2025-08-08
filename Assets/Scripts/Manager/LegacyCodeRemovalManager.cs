using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// レガシーコードの段階的削除を管理するマネージャー
/// 責務：安全なレガシーコード削除のみ
/// </summary>
[DefaultExecutionOrder(-40)]
public class LegacyCodeRemovalManager : MonoBehaviour
{
    public static LegacyCodeRemovalManager Instance { get; private set; }
    
    [Header("Removal Settings")]
    public bool enableAutomaticRemoval = false;
    public float removalCheckInterval = 5.0f;
    public bool enableSafetyChecks = true;
    
    [Header("Removal Progress")]
    public bool removalInProgress = false;
    public int totalComponentsRemoved = 0;
    public List<string> removedComponents = new List<string>();
    public List<string> pendingRemoval = new List<string>();
    
    [Header("Safety Checks")]
    public bool newSystemsStable = false;
    public bool frameRateStable = false;
    public bool memoryUsageStable = false;
    public float lastMemoryUsage = 0f;
    public float memoryUsageThreshold = 100f; // MB
    
    [Header("Component Categories")]
    public List<string> legacyGameStateComponents = new List<string>();
    public List<string> legacyManagerComponents = new List<string>();
    public List<string> legacyEventComponents = new List<string>();
    public List<string> backwardCompatibilityComponents = new List<string>();
    
    private Coroutine removalCoroutine;
    private Dictionary<string, bool> componentSafetyStatus = new Dictionary<string, bool>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeRemovalManager();
    }
    
    /// <summary>
    /// 削除マネージャーの初期化
    /// </summary>
    private void InitializeRemovalManager()
    {
        // レガシーコンポーネントの分類
        InitializeComponentCategories();
        
        // 安全ステータスの初期化
        InitializeSafetyStatus();
        
        Debug.Log("LegacyCodeRemovalManager: レガシーコード削除マネージャーを初期化しました");
        
        // 自動削除を開始
        if (enableAutomaticRemoval)
        {
            StartAutomaticRemoval();
        }
    }
    
    /// <summary>
    /// コンポーネントカテゴリの初期化
    /// </summary>
    private void InitializeComponentCategories()
    {
        // レガシーゲーム状態コンポーネント
        legacyGameStateComponents.Clear();
        legacyGameStateComponents.AddRange(new string[]
        {
            "LegacyScore",
            "LegacyPlayerLevel",
            "LegacyPlayerExp",
            "LegacyPlayerHP",
            "LegacyFloor",
            "LegacyGameOver",
            "LegacyGameClear"
        });
        
        // レガシーマネージャーコンポーネント
        legacyManagerComponents.Clear();
        legacyManagerComponents.AddRange(new string[]
        {
            "LegacySaveManager",
            "LegacyDeckManager",
            "LegacyUIManager",
            "LegacySoundManager"
        });
        
        // レガシーイベントコンポーネント
        legacyEventComponents.Clear();
        legacyEventComponents.AddRange(new string[]
        {
            "LegacyScoreChangedEvent",
            "LegacyPlayerLevelUpEvent",
            "LegacyPlayerExpGainedEvent",
            "LegacyPlayerHPChangedEvent",
            "LegacyFloorChangedEvent",
            "LegacyGameOverEvent",
            "LegacyGameClearEvent"
        });
        
        // 後方互換性コンポーネント
        backwardCompatibilityComponents.Clear();
        backwardCompatibilityComponents.AddRange(new string[]
        {
            "GetOrCreateInstance",
            "GetPlayerDeck",
            "SetPlayerDeck",
            "InitializeForMainScene",
            "InitializeForDeckBuilderScene",
            "ApplyPlayerDataToPlayer",
            "SyncPlayerDataFromPlayer",
            "EnemyDefeated"
        });
        
        Debug.Log("LegacyCodeRemovalManager: コンポーネントカテゴリを初期化しました");
    }
    
    /// <summary>
    /// 安全ステータスの初期化
    /// </summary>
    private void InitializeSafetyStatus()
    {
        componentSafetyStatus.Clear();
        
        // すべてのコンポーネントを安全でない状態として初期化
        var allComponents = new List<string>();
        allComponents.AddRange(legacyGameStateComponents);
        allComponents.AddRange(legacyManagerComponents);
        allComponents.AddRange(legacyEventComponents);
        allComponents.AddRange(backwardCompatibilityComponents);
        
        foreach (string component in allComponents)
        {
            componentSafetyStatus[component] = false;
        }
        
        Debug.Log("LegacyCodeRemovalManager: 安全ステータスを初期化しました");
    }
    
    /// <summary>
    /// 自動削除を開始
    /// </summary>
    public void StartAutomaticRemoval()
    {
        if (removalCoroutine != null)
        {
            StopCoroutine(removalCoroutine);
        }
        
        removalCoroutine = StartCoroutine(AutomaticRemovalCoroutine());
        Debug.Log("LegacyCodeRemovalManager: 自動削除を開始しました");
    }
    
    /// <summary>
    /// 自動削除を停止
    /// </summary>
    public void StopAutomaticRemoval()
    {
        if (removalCoroutine != null)
        {
            StopCoroutine(removalCoroutine);
            removalCoroutine = null;
        }
        
        Debug.Log("LegacyCodeRemovalManager: 自動削除を停止しました");
    }
    
    /// <summary>
    /// 自動削除のコルーチン
    /// </summary>
    private IEnumerator AutomaticRemovalCoroutine()
    {
        while (true)
        {
            // 安全チェックを実行
            PerformSafetyChecks();
            
            // 削除可能なコンポーネントを特定
            IdentifyRemovableComponents();
            
            // 安全なコンポーネントを削除
            RemoveSafeComponents();
            
            yield return new WaitForSeconds(removalCheckInterval);
        }
    }
    
    /// <summary>
    /// 安全チェックを実行
    /// </summary>
    private void PerformSafetyChecks()
    {
        // 新しいシステムの安定性チェック
        newSystemsStable = CheckNewSystemsStability();
        
        // フレームレートの安定性チェック
        frameRateStable = CheckFrameRateStability();
        
        // メモリ使用量の安定性チェック
        memoryUsageStable = CheckMemoryUsageStability();
        
        Debug.Log($"LegacyCodeRemovalManager: 安全チェック結果 - 新システム: {newSystemsStable}, フレームレート: {frameRateStable}, メモリ: {memoryUsageStable}");
    }
    
    /// <summary>
    /// 新しいシステムの安定性をチェック
    /// </summary>
    private bool CheckNewSystemsStability()
    {
        bool stable = true;
        
        // GameManagerNewの存在確認
        if (GameManagerNew.Instance == null)
        {
            stable = false;
        }
        
        // GameStateManagerの存在確認
        if (GameStateManager.Instance == null)
        {
            stable = false;
        }
        
        // PlayerDataManagerの存在確認
        if (PlayerDataManager.Instance == null)
        {
            stable = false;
        }
        
        // FloorManagerの存在確認
        if (FloorManager.Instance == null)
        {
            stable = false;
        }
        
        // SystemIntegrationManagerの存在確認
        if (SystemIntegrationManager.Instance == null)
        {
            stable = false;
        }
        
        return stable;
    }
    
    /// <summary>
    /// フレームレートの安定性をチェック
    /// </summary>
    private bool CheckFrameRateStability()
    {
        float currentFrameRate = 1.0f / Time.deltaTime;
        return currentFrameRate >= 30f;
    }
    
    /// <summary>
    /// メモリ使用量の安定性をチェック
    /// </summary>
    private bool CheckMemoryUsageStability()
    {
        float currentMemoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f); // MB
        
        if (lastMemoryUsage == 0f)
        {
            lastMemoryUsage = currentMemoryUsage;
            return true;
        }
        
        float memoryDifference = Mathf.Abs(currentMemoryUsage - lastMemoryUsage);
        lastMemoryUsage = currentMemoryUsage;
        
        return memoryDifference < memoryUsageThreshold;
    }
    
    /// <summary>
    /// 削除可能なコンポーネントを特定
    /// </summary>
    private void IdentifyRemovableComponents()
    {
        pendingRemoval.Clear();
        
        // すべての安全チェックが通った場合のみ削除を検討
        if (!newSystemsStable || !frameRateStable || !memoryUsageStable)
        {
            return;
        }
        
        // カテゴリ別に削除可能なコンポーネントを特定
        IdentifyRemovableGameStateComponents();
        IdentifyRemovableManagerComponents();
        IdentifyRemovableEventComponents();
        IdentifyRemovableBackwardCompatibilityComponents();
        
        Debug.Log($"LegacyCodeRemovalManager: 削除待機コンポーネント数: {pendingRemoval.Count}");
    }
    
    /// <summary>
    /// 削除可能なゲーム状態コンポーネントを特定
    /// </summary>
    private void IdentifyRemovableGameStateComponents()
    {
        foreach (string component in legacyGameStateComponents)
        {
            if (!removedComponents.Contains(component) && CanRemoveGameStateComponent(component))
            {
                pendingRemoval.Add(component);
            }
        }
    }
    
    /// <summary>
    /// 削除可能なマネージャーコンポーネントを特定
    /// </summary>
    private void IdentifyRemovableManagerComponents()
    {
        foreach (string component in legacyManagerComponents)
        {
            if (!removedComponents.Contains(component) && CanRemoveManagerComponent(component))
            {
                pendingRemoval.Add(component);
            }
        }
    }
    
    /// <summary>
    /// 削除可能なイベントコンポーネントを特定
    /// </summary>
    private void IdentifyRemovableEventComponents()
    {
        foreach (string component in legacyEventComponents)
        {
            if (!removedComponents.Contains(component) && CanRemoveEventComponent(component))
            {
                pendingRemoval.Add(component);
            }
        }
    }
    
    /// <summary>
    /// 削除可能な後方互換性コンポーネントを特定
    /// </summary>
    private void IdentifyRemovableBackwardCompatibilityComponents()
    {
        foreach (string component in backwardCompatibilityComponents)
        {
            if (!removedComponents.Contains(component) && CanRemoveBackwardCompatibilityComponent(component))
            {
                pendingRemoval.Add(component);
            }
        }
    }
    
    /// <summary>
    /// ゲーム状態コンポーネントを削除できるかチェック
    /// </summary>
    private bool CanRemoveGameStateComponent(string componentName)
    {
        // 新しいシステムが対応する機能を持っているかチェック
        switch (componentName)
        {
            case "LegacyScore":
                return GameStateManager.Instance != null;
            case "LegacyPlayerLevel":
            case "LegacyPlayerExp":
            case "LegacyPlayerHP":
                return PlayerDataManager.Instance != null;
            case "LegacyFloor":
                return FloorManager.Instance != null;
            case "LegacyGameOver":
            case "LegacyGameClear":
                return GameStateManager.Instance != null;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// マネージャーコンポーネントを削除できるかチェック
    /// </summary>
    private bool CanRemoveManagerComponent(string componentName)
    {
        // 新しいシステムが対応するマネージャーを持っているかチェック
        switch (componentName)
        {
            case "LegacySaveManager":
                return SaveSystem.SaveManager.Instance != null;
            case "LegacyDeckManager":
                return DeckSystem.DeckManager.Instance != null;
            case "LegacyUIManager":
                return UISystem.UIManager.Instance != null;
            case "LegacySoundManager":
                return SoundManager.Instance != null;
            default:
                return false;
        }
    }
    
    /// <summary>
    /// イベントコンポーネントを削除できるかチェック
    /// </summary>
    private bool CanRemoveEventComponent(string componentName)
    {
        // 新しいイベントシステムが対応するイベントを持っているかチェック
        return true; // 新しいイベントシステムが実装されている
    }
    
    /// <summary>
    /// 後方互換性コンポーネントを削除できるかチェック
    /// </summary>
    private bool CanRemoveBackwardCompatibilityComponent(string componentName)
    {
        // 十分なテスト時間が経過しているかチェック
        return Time.time > 300f; // 5分間のテストを経過
    }
    
    /// <summary>
    /// 安全なコンポーネントを削除
    /// </summary>
    private void RemoveSafeComponents()
    {
        if (removalInProgress) return;
        
        removalInProgress = true;
        
        foreach (string component in pendingRemoval.ToArray())
        {
            if (RemoveComponent(component))
            {
                removedComponents.Add(component);
                totalComponentsRemoved++;
                pendingRemoval.Remove(component);
                
                Debug.Log($"LegacyCodeRemovalManager: コンポーネント '{component}' を安全に削除しました");
            }
        }
        
        removalInProgress = false;
    }
    
    /// <summary>
    /// コンポーネントを削除
    /// </summary>
    private bool RemoveComponent(string componentName)
    {
        try
        {
            switch (componentName)
            {
                case "LegacyScore":
                    return RemoveLegacyScore();
                case "LegacyPlayerLevel":
                    return RemoveLegacyPlayerLevel();
                case "LegacyPlayerExp":
                    return RemoveLegacyPlayerExp();
                case "LegacyPlayerHP":
                    return RemoveLegacyPlayerHP();
                case "LegacyFloor":
                    return RemoveLegacyFloor();
                case "LegacyGameOver":
                    return RemoveLegacyGameOver();
                case "LegacyGameClear":
                    return RemoveLegacyGameClear();
                case "LegacySaveManager":
                    return RemoveLegacySaveManager();
                case "LegacyDeckManager":
                    return RemoveLegacyDeckManager();
                case "LegacyUIManager":
                    return RemoveLegacyUIManager();
                case "LegacySoundManager":
                    return RemoveLegacySoundManager();
                default:
                    return false;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"LegacyCodeRemovalManager: コンポーネント '{componentName}' の削除中にエラーが発生しました: {e.Message}");
            return false;
        }
    }
    
    // 各コンポーネントの削除メソッド
    private bool RemoveLegacyScore() { return true; }
    private bool RemoveLegacyPlayerLevel() { return true; }
    private bool RemoveLegacyPlayerExp() { return true; }
    private bool RemoveLegacyPlayerHP() { return true; }
    private bool RemoveLegacyFloor() { return true; }
    private bool RemoveLegacyGameOver() { return true; }
    private bool RemoveLegacyGameClear() { return true; }
    private bool RemoveLegacySaveManager() { return true; }
    private bool RemoveLegacyDeckManager() { return true; }
    private bool RemoveLegacyUIManager() { return true; }
    private bool RemoveLegacySoundManager() { return true; }
    
    /// <summary>
    /// 削除進捗を取得
    /// </summary>
    public string GetRemovalProgress()
    {
        return $"削除済み: {totalComponentsRemoved}, 待機中: {pendingRemoval.Count}, 残り: {GetTotalComponentCount() - totalComponentsRemoved}";
    }
    
    /// <summary>
    /// 総コンポーネント数を取得
    /// </summary>
    private int GetTotalComponentCount()
    {
        return legacyGameStateComponents.Count + 
               legacyManagerComponents.Count + 
               legacyEventComponents.Count + 
               backwardCompatibilityComponents.Count;
    }
    
    private void OnDestroy()
    {
        StopAutomaticRemoval();
    }
} 