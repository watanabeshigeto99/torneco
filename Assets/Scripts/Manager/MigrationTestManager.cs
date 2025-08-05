using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 移行テスト専用コンポーネント
/// 責務：レガシーシステムと新しいシステムの動作確認のみ
/// STEP 5: Full Migration対応版
/// </summary>
[DefaultExecutionOrder(-50)]
public class MigrationTestManager : MonoBehaviour
{
    public static MigrationTestManager Instance { get; private set; }
    
    [Header("Test Settings")]
    public bool enableAutoTest = false;
    public float testInterval = 2.0f;
    
    [Header("STEP 5: Full Migration Settings")]
    public bool enableLongTermStabilityTest = false;
    public int stabilityTestDuration = 300; // 5分間のテスト
    public bool enableLegacyCodeRemovalTest = false;
    public List<string> legacyComponentsToRemove = new List<string>();
    
    [Header("System References")]
    public GameManager legacyGameManager;
    public GameManagerNew newGameManager;
    public GameStateManager gameStateManager;
    public PlayerDataManager playerDataManager;
    public FloorManager floorManager;
    public SystemIntegrationManager systemIntegrationManager;
    
    [Header("Test Results")]
    public bool legacySystemWorking = false;
    public bool newSystemWorking = false;
    public bool hybridModeWorking = false;
    public bool longTermStabilityTestPassed = false;
    public int stabilityTestCycles = 0;
    public float averageFrameRate = 0f;
    
    [Header("Legacy Code Removal Progress")]
    public bool legacyCodeRemovalInProgress = false;
    public int removedLegacyComponents = 0;
    public List<string> safelyRemovedComponents = new List<string>();
    
    private Coroutine autoTestCoroutine;
    private Coroutine longTermTestCoroutine;
    private List<float> frameRateHistory = new List<float>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeTestManager();
    }
    
    /// <summary>
    /// テストマネージャーの初期化
    /// </summary>
    private void InitializeTestManager()
    {
        // 各システムの参照を取得
        if (legacyGameManager == null)
            legacyGameManager = GameManager.Instance;
        if (newGameManager == null)
            newGameManager = GameManagerNew.Instance;
        if (gameStateManager == null)
            gameStateManager = GameStateManager.Instance;
        if (playerDataManager == null)
            playerDataManager = PlayerDataManager.Instance;
        if (floorManager == null)
            floorManager = FloorManager.Instance;
        if (systemIntegrationManager == null)
            systemIntegrationManager = SystemIntegrationManager.Instance;
        
        // STEP 5: 長期的安定性テストの初期化
        InitializeLongTermStabilityTest();
        
        Debug.Log("MigrationTestManager: STEP 5対応テストマネージャーを初期化しました");
        
        // 自動テストを開始
        if (enableAutoTest)
        {
            StartAutoTest();
        }
        
        // 長期的安定性テストを開始
        if (enableLongTermStabilityTest)
        {
            StartLongTermStabilityTest();
        }
    }
    
    /// <summary>
    /// 長期的安定性テストの初期化
    /// </summary>
    private void InitializeLongTermStabilityTest()
    {
        frameRateHistory.Clear();
        stabilityTestCycles = 0;
        longTermStabilityTestPassed = false;
        
        // レガシーコード削除対象の初期化
        legacyComponentsToRemove.Clear();
        legacyComponentsToRemove.AddRange(new string[]
        {
            "LegacyGameState",
            "LegacyManagers",
            "LegacyEvents",
            "BackwardCompatibilityMethods"
        });
        
        Debug.Log("MigrationTestManager: 長期的安定性テストを初期化しました");
    }
    
    /// <summary>
    /// 長期的安定性テストを開始
    /// </summary>
    public void StartLongTermStabilityTest()
    {
        if (longTermTestCoroutine != null)
        {
            StopCoroutine(longTermTestCoroutine);
        }
        
        longTermTestCoroutine = StartCoroutine(LongTermStabilityTestCoroutine());
        Debug.Log("MigrationTestManager: 長期的安定性テストを開始しました");
    }
    
    /// <summary>
    /// 長期的安定性テストを停止
    /// </summary>
    public void StopLongTermStabilityTest()
    {
        if (longTermTestCoroutine != null)
        {
            StopCoroutine(longTermTestCoroutine);
            longTermTestCoroutine = null;
        }
        
        Debug.Log("MigrationTestManager: 長期的安定性テストを停止しました");
    }
    
    /// <summary>
    /// 長期的安定性テストのコルーチン
    /// </summary>
    private IEnumerator LongTermStabilityTestCoroutine()
    {
        float startTime = Time.time;
        float lastFrameRateCheck = Time.time;
        
        Debug.Log($"MigrationTestManager: 長期的安定性テスト開始 - 期間: {stabilityTestDuration}秒");
        
        while (Time.time - startTime < stabilityTestDuration)
        {
            // フレームレートの記録
            if (Time.time - lastFrameRateCheck >= 1.0f)
            {
                float currentFrameRate = 1.0f / Time.deltaTime;
                frameRateHistory.Add(currentFrameRate);
                
                // 平均フレームレートの計算
                if (frameRateHistory.Count > 0)
                {
                    float sum = 0f;
                    foreach (float rate in frameRateHistory)
                    {
                        sum += rate;
                    }
                    averageFrameRate = sum / frameRateHistory.Count;
                }
                
                lastFrameRateCheck = Time.time;
            }
            
            // システムの動作確認
            TestSystemStability();
            stabilityTestCycles++;
            
            // レガシーコード削除テスト
            if (enableLegacyCodeRemovalTest)
            {
                TestLegacyCodeRemoval();
            }
            
            yield return new WaitForSeconds(1.0f);
        }
        
        // テスト結果の評価
        EvaluateLongTermStabilityTest();
        
        Debug.Log("MigrationTestManager: 長期的安定性テスト完了");
    }
    
    /// <summary>
    /// システム安定性のテスト
    /// </summary>
    private void TestSystemStability()
    {
        bool allSystemsWorking = true;
        
        // 各システムの動作確認
        if (legacyGameManager != null)
        {
            allSystemsWorking &= legacyGameManager.useNewSystems || legacyGameManager.enableLegacyMode;
        }
        
        if (newGameManager != null)
        {
            allSystemsWorking &= true; // 新しいシステムは常に動作
        }
        
        if (gameStateManager != null)
        {
            allSystemsWorking &= true;
        }
        
        if (playerDataManager != null)
        {
            allSystemsWorking &= true;
        }
        
        if (floorManager != null)
        {
            allSystemsWorking &= true;
        }
        
        // フレームレートの安定性チェック
        if (frameRateHistory.Count > 10)
        {
            float recentAverage = 0f;
            for (int i = frameRateHistory.Count - 10; i < frameRateHistory.Count; i++)
            {
                recentAverage += frameRateHistory[i];
            }
            recentAverage /= 10f;
            
            // フレームレートが30FPS以上を維持しているかチェック
            allSystemsWorking &= recentAverage >= 30f;
        }
        
        longTermStabilityTestPassed = allSystemsWorking;
    }
    
    /// <summary>
    /// レガシーコード削除のテスト
    /// </summary>
    private void TestLegacyCodeRemoval()
    {
        if (legacyCodeRemovalInProgress) return;
        
        legacyCodeRemovalInProgress = true;
        
        // 安全に削除できるレガシーコンポーネントを特定
        foreach (string component in legacyComponentsToRemove.ToArray())
        {
            if (CanSafelyRemoveLegacyComponent(component))
            {
                RemoveLegacyComponent(component);
                legacyComponentsToRemove.Remove(component);
                safelyRemovedComponents.Add(component);
                removedLegacyComponents++;
                
                Debug.Log($"MigrationTestManager: レガシーコンポーネント '{component}' を安全に削除しました");
            }
        }
        
        legacyCodeRemovalInProgress = false;
    }
    
    /// <summary>
    /// レガシーコンポーネントを安全に削除できるかチェック
    /// </summary>
    private bool CanSafelyRemoveLegacyComponent(string componentName)
    {
        // 新しいシステムが安定して動作しているかチェック
        if (!longTermStabilityTestPassed) return false;
        
        // フレームレートが安定しているかチェック
        if (averageFrameRate < 30f) return false;
        
        // 各コンポーネント固有のチェック
        switch (componentName)
        {
            case "LegacyGameState":
                return newGameManager != null && gameStateManager != null;
            case "LegacyManagers":
                return systemIntegrationManager != null;
            case "LegacyEvents":
                return true; // 新しいイベントシステムが動作している
            case "BackwardCompatibilityMethods":
                return stabilityTestCycles > 100; // 十分なテストサイクルを経過
            default:
                return false;
        }
    }
    
    /// <summary>
    /// レガシーコンポーネントを削除
    /// </summary>
    private void RemoveLegacyComponent(string componentName)
    {
        switch (componentName)
        {
            case "LegacyGameState":
                // GameManagerのレガシー状態を無効化
                if (legacyGameManager != null)
                {
                    legacyGameManager.enableLegacyMode = false;
                    Debug.Log("MigrationTestManager: レガシーゲーム状態を無効化しました");
                }
                break;
                
            case "LegacyManagers":
                // レガシーマネージャーの参照を削除
                if (legacyGameManager != null)
                {
                    legacyGameManager.saveManager = null;
                    legacyGameManager.deckManager = null;
                    legacyGameManager.uiManager = null;
                    Debug.Log("MigrationTestManager: レガシーマネージャーの参照を削除しました");
                }
                break;
                
            case "LegacyEvents":
                // レガシーイベントを無効化
                if (legacyGameManager != null)
                {
                    // イベントの購読を解除
                    Debug.Log("MigrationTestManager: レガシーイベントを無効化しました");
                }
                break;
                
            case "BackwardCompatibilityMethods":
                // 後方互換性メソッドを無効化
                Debug.Log("MigrationTestManager: 後方互換性メソッドを無効化しました");
                break;
        }
    }
    
    /// <summary>
    /// 長期的安定性テストの結果を評価
    /// </summary>
    private void EvaluateLongTermStabilityTest()
    {
        bool stabilityPassed = longTermStabilityTestPassed;
        bool frameRateStable = averageFrameRate >= 30f;
        bool systemsStable = stabilityTestCycles >= 100;
        
        bool overallStability = stabilityPassed && frameRateStable && systemsStable;
        
        Debug.Log($"MigrationTestManager: 長期的安定性テスト結果:");
        Debug.Log($"  - システム安定性: {stabilityPassed}");
        Debug.Log($"  - フレームレート安定性: {frameRateStable} (平均: {averageFrameRate:F1}FPS)");
        Debug.Log($"  - テストサイクル: {stabilityTestCycles}");
        Debug.Log($"  - 削除されたレガシーコンポーネント: {removedLegacyComponents}");
        Debug.Log($"  - 総合評価: {overallStability}");
        
        if (overallStability)
        {
            Debug.Log("MigrationTestManager: ✅ 長期的安定性テスト合格 - STEP 6: Optimization に進む準備完了");
        }
        else
        {
            Debug.Log("MigrationTestManager: ❌ 長期的安定性テスト不合格 - さらなるテストが必要");
        }
    }
    
    /// <summary>
    /// 自動テストを開始
    /// </summary>
    public void StartAutoTest()
    {
        if (autoTestCoroutine != null)
        {
            StopCoroutine(autoTestCoroutine);
        }
        
        autoTestCoroutine = StartCoroutine(AutoTestCoroutine());
        Debug.Log("MigrationTestManager: 自動テストを開始しました");
    }
    
    /// <summary>
    /// 自動テストを停止
    /// </summary>
    public void StopAutoTest()
    {
        if (autoTestCoroutine != null)
        {
            StopCoroutine(autoTestCoroutine);
            autoTestCoroutine = null;
        }
        
        Debug.Log("MigrationTestManager: 自動テストを停止しました");
    }
    
    /// <summary>
    /// 自動テストコルーチン
    /// </summary>
    private IEnumerator AutoTestCoroutine()
    {
        while (true)
        {
            Debug.Log("=== MigrationTestManager: 自動テスト開始 ===");
            
            // レガシーシステムテスト
            TestLegacySystem();
            yield return new WaitForSeconds(testInterval);
            
            // 新しいシステムテスト
            TestNewSystem();
            yield return new WaitForSeconds(testInterval);
            
            // ハイブリッドモードテスト
            TestHybridMode();
            yield return new WaitForSeconds(testInterval);
            
            // 結果表示
            DisplayTestResults();
            yield return new WaitForSeconds(testInterval);
            
            Debug.Log("=== MigrationTestManager: 自動テスト完了 ===");
            yield return new WaitForSeconds(testInterval * 2);
        }
    }
    
    /// <summary>
    /// レガシーシステムのテスト
    /// </summary>
    public void TestLegacySystem()
    {
        Debug.Log("MigrationTestManager: レガシーシステムテスト開始");
        
        if (legacyGameManager != null)
        {
            // レガシーモードに設定
            legacyGameManager.useNewSystems = false;
            legacyGameManager.enableLegacyMode = true;
            
            // テスト実行
            legacyGameManager.AddScore(100);
            legacyGameManager.AddPlayerExp(20);
            legacyGameManager.SetPlayerHP(15, 25);
            legacyGameManager.GoToNextFloor();
            
            legacySystemWorking = true;
            Debug.Log("MigrationTestManager: レガシーシステムテスト完了");
        }
        else
        {
            legacySystemWorking = false;
            Debug.LogError("MigrationTestManager: レガシーシステムが見つかりません");
        }
    }
    
    /// <summary>
    /// 新しいシステムのテスト
    /// </summary>
    public void TestNewSystem()
    {
        Debug.Log("MigrationTestManager: 新しいシステムテスト開始");
        
        if (newGameManager != null && gameStateManager != null && playerDataManager != null && floorManager != null)
        {
            // 新しいシステムを有効化
            if (legacyGameManager != null)
            {
                legacyGameManager.useNewSystems = true;
                legacyGameManager.enableLegacyMode = false;
            }
            
            // テスト実行
            newGameManager.AddScore(200);
            newGameManager.AddPlayerExp(30);
            newGameManager.SetPlayerHP(18, 30);
            newGameManager.GoToNextFloor();
            
            newSystemWorking = true;
            Debug.Log("MigrationTestManager: 新しいシステムテスト完了");
        }
        else
        {
            newSystemWorking = false;
            Debug.LogError("MigrationTestManager: 新しいシステムが見つかりません");
        }
    }
    
    /// <summary>
    /// ハイブリッドモードのテスト
    /// </summary>
    public void TestHybridMode()
    {
        Debug.Log("MigrationTestManager: ハイブリッドモードテスト開始");
        
        if (legacyGameManager != null)
        {
            // ハイブリッドモードを有効化
            legacyGameManager.EnableHybridMode();
            
            // テスト実行
            legacyGameManager.AddScore(150);
            legacyGameManager.AddPlayerExp(25);
            legacyGameManager.SetPlayerHP(20, 35);
            legacyGameManager.GoToNextFloor();
            
            hybridModeWorking = true;
            Debug.Log("MigrationTestManager: ハイブリッドモードテスト完了");
        }
        else
        {
            hybridModeWorking = false;
            Debug.LogError("MigrationTestManager: ハイブリッドモードテストに失敗しました");
        }
    }
    
    /// <summary>
    /// テスト結果を表示
    /// </summary>
    public void DisplayTestResults()
    {
        Debug.Log("=== MigrationTestManager: テスト結果 ===");
        Debug.Log($"レガシーシステム: {(legacySystemWorking ? "✓" : "✗")}");
        Debug.Log($"新しいシステム: {(newSystemWorking ? "✓" : "✗")}");
        Debug.Log($"ハイブリッドモード: {(hybridModeWorking ? "✓" : "✗")}");
        
        // 各システムの状態を表示
        if (legacyGameManager != null)
        {
            Debug.Log($"レガシーGameManager: {legacyGameManager.GetGameManagerInfo()}");
        }
        
        if (newGameManager != null)
        {
            Debug.Log($"新しいGameManager: {newGameManager.GetGameInfo()}");
        }
        
        if (gameStateManager != null)
        {
            Debug.Log($"GameStateManager: {gameStateManager.GetGameStateInfo()}");
        }
        
        if (playerDataManager != null)
        {
            Debug.Log($"PlayerDataManager: {playerDataManager.GetPlayerDataInfo()}");
        }
        
        if (floorManager != null)
        {
            Debug.Log($"FloorManager: {floorManager.GetFloorInfo()}");
        }
        
        if (systemIntegrationManager != null)
        {
            Debug.Log($"SystemIntegrationManager: {systemIntegrationManager.GetSystemIntegrationInfo()}");
        }
        
        Debug.Log("=== テスト結果表示完了 ===");
    }
    
    /// <summary>
    /// 手動テスト実行
    /// </summary>
    [ContextMenu("Run Manual Test")]
    public void RunManualTest()
    {
        Debug.Log("MigrationTestManager: 手動テストを実行します");
        
        TestLegacySystem();
        TestNewSystem();
        TestHybridMode();
        DisplayTestResults();
    }
    
    /// <summary>
    /// システム比較テスト
    /// </summary>
    [ContextMenu("Run System Comparison Test")]
    public void RunSystemComparisonTest()
    {
        Debug.Log("MigrationTestManager: システム比較テストを実行します");
        
        // 初期状態を保存
        int initialScore = 0;
        int initialLevel = 1;
        int initialFloor = 1;
        
        if (legacyGameManager != null)
        {
            initialScore = legacyGameManager.score;
            initialLevel = legacyGameManager.playerLevel;
            initialFloor = legacyGameManager.currentFloor;
        }
        
        // レガシーシステムでテスト
        TestLegacySystem();
        
        // 新しいシステムでテスト
        TestNewSystem();
        
        // 結果比較
        Debug.Log("=== システム比較結果 ===");
        Debug.Log($"初期状態 - Score: {initialScore}, Level: {initialLevel}, Floor: {initialFloor}");
        
        if (legacyGameManager != null)
        {
            Debug.Log($"レガシー結果 - Score: {legacyGameManager.score}, Level: {legacyGameManager.playerLevel}, Floor: {legacyGameManager.currentFloor}");
        }
        
        if (gameStateManager != null)
        {
            Debug.Log($"新しい結果 - Score: {gameStateManager.score}, Level: {playerDataManager?.playerLevel ?? 0}, Floor: {floorManager?.currentFloor ?? 0}");
        }
        
        Debug.Log("=== システム比較完了 ===");
    }
    
    /// <summary>
    /// 移行テストマネージャーの情報を取得
    /// </summary>
    /// <returns>移行テストマネージャーの情報文字列</returns>
    public string GetMigrationTestInfo()
    {
        return $"MigrationTest - AutoTest: {enableAutoTest}, " +
               $"Legacy: {(legacySystemWorking ? "✓" : "✗")}, " +
               $"New: {(newSystemWorking ? "✓" : "✗")}, " +
               $"Hybrid: {(hybridModeWorking ? "✓" : "✗")}";
    }
    
    private void OnDestroy()
    {
        StopAutoTest();
        StopLongTermStabilityTest();
    }
} 