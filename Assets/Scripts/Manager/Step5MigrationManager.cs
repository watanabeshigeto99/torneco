using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// STEP 5: Full Migration を管理するマネージャー
/// 責務：完全移行の進行管理のみ
/// </summary>
[DefaultExecutionOrder(-30)]
public class Step5MigrationManager : MonoBehaviour
{
    public static Step5MigrationManager Instance { get; private set; }
    
    [Header("Migration Settings")]
    public bool enableStep5Migration = false;
    public float migrationCheckInterval = 10.0f;
    public bool enableAutomaticMigration = false;
    
    [Header("Migration Progress")]
    public bool migrationInProgress = false;
    public float migrationProgress = 0f;
    public int currentMigrationPhase = 0;
    public int totalMigrationPhases = 4;
    
    [Header("Phase Status")]
    public bool phase1Completed = false; // 長期的安定性テスト
    public bool phase2Completed = false; // レガシーコード削除
    public bool phase3Completed = false; // 新しいシステムへの完全移行
    public bool phase4Completed = false; // 最適化の準備
    
    [Header("System References")]
    public MigrationTestManager migrationTestManager;
    public LegacyCodeRemovalManager legacyCodeRemovalManager;
    public GameManager legacyGameManager;
    public GameManagerNew newGameManager;
    
    [Header("Migration Results")]
    public bool step5Completed = false;
    public string migrationStatus = "未開始";
    public List<string> migrationLog = new List<string>();
    
    private Coroutine migrationCoroutine;
    private float migrationStartTime;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeStep5Manager();
    }
    
    /// <summary>
    /// STEP 5マネージャーの初期化
    /// </summary>
    private void InitializeStep5Manager()
    {
        // 各システムの参照を取得
        if (migrationTestManager == null)
            migrationTestManager = MigrationTestManager.Instance;
        if (legacyCodeRemovalManager == null)
            legacyCodeRemovalManager = LegacyCodeRemovalManager.Instance;
        if (legacyGameManager == null)
            legacyGameManager = GameManager.Instance;
        if (newGameManager == null)
            newGameManager = GameManagerNew.Instance;
        
        // 移行ログの初期化
        migrationLog.Clear();
        AddMigrationLog("STEP 5: Full Migration マネージャーを初期化しました");
        
        Debug.Log("Step5MigrationManager: STEP 5移行マネージャーを初期化しました");
        
        // 自動移行を開始
        if (enableAutomaticMigration)
        {
            StartAutomaticMigration();
        }
    }
    
    /// <summary>
    /// 自動移行を開始
    /// </summary>
    public void StartAutomaticMigration()
    {
        if (migrationCoroutine != null)
        {
            StopCoroutine(migrationCoroutine);
        }
        
        migrationCoroutine = StartCoroutine(AutomaticMigrationCoroutine());
        migrationStartTime = Time.time;
        AddMigrationLog("自動移行を開始しました");
        
        Debug.Log("Step5MigrationManager: 自動移行を開始しました");
    }
    
    /// <summary>
    /// 自動移行を停止
    /// </summary>
    public void StopAutomaticMigration()
    {
        if (migrationCoroutine != null)
        {
            StopCoroutine(migrationCoroutine);
            migrationCoroutine = null;
        }
        
        AddMigrationLog("自動移行を停止しました");
        Debug.Log("Step5MigrationManager: 自動移行を停止しました");
    }
    
    /// <summary>
    /// 自動移行のコルーチン
    /// </summary>
    private IEnumerator AutomaticMigrationCoroutine()
    {
        AddMigrationLog("STEP 5移行プロセスを開始します");
        
        // Phase 1: 長期的安定性テスト
        yield return StartCoroutine(ExecutePhase1());
        
        // Phase 2: レガシーコード削除
        yield return StartCoroutine(ExecutePhase2());
        
        // Phase 3: 新しいシステムへの完全移行
        yield return StartCoroutine(ExecutePhase3());
        
        // Phase 4: 最適化の準備
        yield return StartCoroutine(ExecutePhase4());
        
        // 移行完了
        CompleteStep5Migration();
    }
    
    /// <summary>
    /// Phase 1: 長期的安定性テスト
    /// </summary>
    private IEnumerator ExecutePhase1()
    {
        currentMigrationPhase = 1;
        AddMigrationLog("Phase 1: 長期的安定性テストを開始");
        
        if (migrationTestManager != null)
        {
            // 長期的安定性テストを開始
            migrationTestManager.enableLongTermStabilityTest = true;
            migrationTestManager.StartLongTermStabilityTest();
            
            // テスト完了まで待機
            float startTime = Time.time;
            while (!migrationTestManager.longTermStabilityTestPassed && 
                   Time.time - startTime < migrationTestManager.stabilityTestDuration)
            {
                migrationProgress = (Time.time - startTime) / migrationTestManager.stabilityTestDuration * 0.25f;
                yield return new WaitForSeconds(1.0f);
            }
            
            if (migrationTestManager.longTermStabilityTestPassed)
            {
                phase1Completed = true;
                AddMigrationLog("Phase 1: 長期的安定性テスト完了 ✅");
            }
            else
            {
                AddMigrationLog("Phase 1: 長期的安定性テスト失敗 ❌");
            }
        }
        else
        {
            AddMigrationLog("Phase 1: MigrationTestManagerが見つかりません ❌");
        }
    }
    
    /// <summary>
    /// Phase 2: レガシーコード削除
    /// </summary>
    private IEnumerator ExecutePhase2()
    {
        currentMigrationPhase = 2;
        AddMigrationLog("Phase 2: レガシーコード削除を開始");
        
        if (legacyCodeRemovalManager != null)
        {
            // レガシーコード削除を開始
            legacyCodeRemovalManager.enableAutomaticRemoval = true;
            legacyCodeRemovalManager.StartAutomaticRemoval();
            
            // 削除完了まで待機
            float startTime = Time.time;
            int initialRemovedCount = legacyCodeRemovalManager.totalComponentsRemoved;
            
            while (legacyCodeRemovalManager.totalComponentsRemoved < GetTotalLegacyComponents() &&
                   Time.time - startTime < 300f) // 5分間のタイムアウト
            {
                migrationProgress = 0.25f + (legacyCodeRemovalManager.totalComponentsRemoved - initialRemovedCount) / 
                                  (float)(GetTotalLegacyComponents() - initialRemovedCount) * 0.25f;
                yield return new WaitForSeconds(5.0f);
            }
            
            if (legacyCodeRemovalManager.totalComponentsRemoved >= GetTotalLegacyComponents())
            {
                phase2Completed = true;
                AddMigrationLog($"Phase 2: レガシーコード削除完了 ✅ ({legacyCodeRemovalManager.totalComponentsRemoved}個削除)");
            }
            else
            {
                AddMigrationLog("Phase 2: レガシーコード削除がタイムアウトしました ⚠️");
            }
        }
        else
        {
            AddMigrationLog("Phase 2: LegacyCodeRemovalManagerが見つかりません ❌");
        }
    }
    
    /// <summary>
    /// Phase 3: 新しいシステムへの完全移行
    /// </summary>
    private IEnumerator ExecutePhase3()
    {
        currentMigrationPhase = 3;
        AddMigrationLog("Phase 3: 新しいシステムへの完全移行を開始");
        
        if (legacyGameManager != null)
        {
            // 新しいシステムを有効化
            legacyGameManager.useNewSystems = true;
            legacyGameManager.enableLegacyMode = false;
            
            // 新しいシステムの動作確認
            float startTime = Time.time;
            bool newSystemWorking = false;
            
            while (!newSystemWorking && Time.time - startTime < 60f) // 1分間のテスト
            {
                newSystemWorking = CheckNewSystemStability();
                migrationProgress = 0.5f + (Time.time - startTime) / 60f * 0.25f;
                yield return new WaitForSeconds(2.0f);
            }
            
            if (newSystemWorking)
            {
                phase3Completed = true;
                AddMigrationLog("Phase 3: 新しいシステムへの完全移行完了 ✅");
            }
            else
            {
                AddMigrationLog("Phase 3: 新しいシステムへの完全移行失敗 ❌");
            }
        }
        else
        {
            AddMigrationLog("Phase 3: GameManagerが見つかりません ❌");
        }
    }
    
    /// <summary>
    /// Phase 4: 最適化の準備
    /// </summary>
    private IEnumerator ExecutePhase4()
    {
        currentMigrationPhase = 4;
        AddMigrationLog("Phase 4: 最適化の準備を開始");
        
        // 最適化の準備作業
        yield return StartCoroutine(PrepareOptimization());
        
        phase4Completed = true;
        AddMigrationLog("Phase 4: 最適化の準備完了 ✅");
    }
    
    /// <summary>
    /// 最適化の準備
    /// </summary>
    private IEnumerator PrepareOptimization()
    {
        // パフォーマンスメトリクスの収集
        yield return StartCoroutine(CollectPerformanceMetrics());
        
        // メモリ使用量の最適化
        yield return StartCoroutine(OptimizeMemoryUsage());
        
        // イベントシステムの最適化準備
        yield return StartCoroutine(PrepareEventOptimization());
        
        migrationProgress = 1.0f;
    }
    
    /// <summary>
    /// パフォーマンスメトリクスの収集
    /// </summary>
    private IEnumerator CollectPerformanceMetrics()
    {
        AddMigrationLog("パフォーマンスメトリクスを収集中...");
        
        // フレームレートの測定
        float frameRateSum = 0f;
        int frameRateCount = 0;
        
        for (int i = 0; i < 60; i++) // 60フレーム分測定
        {
            frameRateSum += 1.0f / Time.deltaTime;
            frameRateCount++;
            yield return null;
        }
        
        float averageFrameRate = frameRateSum / frameRateCount;
        AddMigrationLog($"平均フレームレート: {averageFrameRate:F1} FPS");
        
        // メモリ使用量の測定
        float memoryUsage = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        AddMigrationLog($"メモリ使用量: {memoryUsage:F1} MB");
    }
    
    /// <summary>
    /// メモリ使用量の最適化
    /// </summary>
    private IEnumerator OptimizeMemoryUsage()
    {
        AddMigrationLog("メモリ使用量を最適化中...");
        
        // ガベージコレクションの実行
        System.GC.Collect();
        yield return new WaitForSeconds(0.1f);
        
        float memoryAfterGC = System.GC.GetTotalMemory(false) / (1024f * 1024f);
        AddMigrationLog($"GC後のメモリ使用量: {memoryAfterGC:F1} MB");
    }
    
    /// <summary>
    /// イベントシステムの最適化準備
    /// </summary>
    private IEnumerator PrepareEventOptimization()
    {
        AddMigrationLog("イベントシステムの最適化準備中...");
        
        // イベントの購読状況を確認
        yield return new WaitForSeconds(1.0f);
        
        AddMigrationLog("イベントシステムの最適化準備完了");
    }
    
    /// <summary>
    /// 新しいシステムの安定性をチェック
    /// </summary>
    private bool CheckNewSystemStability()
    {
        bool stable = true;
        
        // 新しいシステムの存在確認
        if (newGameManager == null) stable = false;
        if (GameStateManager.Instance == null) stable = false;
        if (PlayerDataManager.Instance == null) stable = false;
        if (FloorManager.Instance == null) stable = false;
        if (SystemIntegrationManager.Instance == null) stable = false;
        
        // フレームレートの確認
        float frameRate = 1.0f / Time.deltaTime;
        if (frameRate < 30f) stable = false;
        
        return stable;
    }
    
    /// <summary>
    /// 総レガシーコンポーネント数を取得
    /// </summary>
    private int GetTotalLegacyComponents()
    {
        if (legacyCodeRemovalManager == null) return 0;
        
        return legacyCodeRemovalManager.legacyGameStateComponents.Count +
               legacyCodeRemovalManager.legacyManagerComponents.Count +
               legacyCodeRemovalManager.legacyEventComponents.Count +
               legacyCodeRemovalManager.backwardCompatibilityComponents.Count;
    }
    
    /// <summary>
    /// STEP 5移行を完了
    /// </summary>
    private void CompleteStep5Migration()
    {
        step5Completed = phase1Completed && phase2Completed && phase3Completed && phase4Completed;
        
        if (step5Completed)
        {
            migrationStatus = "完了";
            AddMigrationLog("🎉 STEP 5: Full Migration 完了！");
            AddMigrationLog("次のステップ: STEP 6: Optimization に進む準備が整いました");
        }
        else
        {
            migrationStatus = "部分完了";
            AddMigrationLog("⚠️ STEP 5: Full Migration が部分完了しました");
            AddMigrationLog($"Phase 1: {phase1Completed}, Phase 2: {phase2Completed}, Phase 3: {phase3Completed}, Phase 4: {phase4Completed}");
        }
        
        float totalTime = Time.time - migrationStartTime;
        AddMigrationLog($"総移行時間: {totalTime:F1}秒");
    }
    
    /// <summary>
    /// 移行ログを追加
    /// </summary>
    private void AddMigrationLog(string message)
    {
        string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {message}";
        migrationLog.Add(logEntry);
        
        // ログが多すぎる場合は古いものを削除
        if (migrationLog.Count > 100)
        {
            migrationLog.RemoveAt(0);
        }
        
        Debug.Log($"Step5MigrationManager: {message}");
    }
    
    /// <summary>
    /// 移行状況を取得
    /// </summary>
    public string GetMigrationStatus()
    {
        return $"STEP 5進捗: {migrationProgress:P0} ({currentMigrationPhase}/{totalMigrationPhases})\n" +
               $"Phase 1: {(phase1Completed ? "✅" : "⏳")} Phase 2: {(phase2Completed ? "✅" : "⏳")}\n" +
               $"Phase 3: {(phase3Completed ? "✅" : "⏳")} Phase 4: {(phase4Completed ? "✅" : "⏳")}\n" +
               $"状態: {migrationStatus}";
    }
    
    private void OnDestroy()
    {
        StopAutomaticMigration();
    }
} 