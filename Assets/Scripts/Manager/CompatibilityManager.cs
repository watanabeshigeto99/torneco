using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 既存機能との互換性確保専用クラス
/// 責務：既存機能との互換性チェックと保証のみ
/// </summary>
[DefaultExecutionOrder(-70)]
public class CompatibilityManager : MonoBehaviour
{
    public static CompatibilityManager Instance { get; private set; }
    
    [Header("Compatibility Settings")]
    [SerializeField] private bool enableCompatibilityChecks = true;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool autoFixCompatibilityIssues = false;
    
    // 互換性チェック結果
    private Dictionary<string, CompatibilityCheckResult> compatibilityResults = new Dictionary<string, CompatibilityCheckResult>();
    private List<CompatibilityRule> compatibilityRules = new List<CompatibilityRule>();
    
    // イベント定義
    public static event Action<string> OnCompatibilityCheckCompleted;
    public static event Action<string, string> OnCompatibilityIssueFound;
    public static event Action<string> OnCompatibilityIssueFixed;
    public static event Action OnAllCompatibilityChecksCompleted;
    
    /// <summary>
    /// 互換性チェック結果
    /// </summary>
    [System.Serializable]
    public class CompatibilityCheckResult
    {
        public string featureId;
        public bool isCompatible;
        public List<string> issues;
        public List<string> warnings;
        public float checkTime;
        public string status;
        
        public CompatibilityCheckResult(string id)
        {
            featureId = id;
            isCompatible = true;
            issues = new List<string>();
            warnings = new List<string>();
            checkTime = Time.time;
            status = "Pending";
        }
    }
    
    /// <summary>
    /// 互換性ルール
    /// </summary>
    [System.Serializable]
    public class CompatibilityRule
    {
        public string ruleId;
        public string description;
        public Func<bool> checkFunction;
        public Action fixFunction;
        public bool isCritical;
        public string category;
        
        public CompatibilityRule(string id, string desc, Func<bool> check, Action fix = null, bool critical = false, string cat = "General")
        {
            ruleId = id;
            description = desc;
            checkFunction = check;
            fixFunction = fix;
            isCritical = critical;
            category = cat;
        }
    }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeCompatibilityManager();
    }
    
    /// <summary>
    /// 互換性マネージャーの初期化
    /// </summary>
    private void InitializeCompatibilityManager()
    {
        compatibilityResults.Clear();
        compatibilityRules.Clear();
        
        // 基本的な互換性ルールを登録
        RegisterDefaultCompatibilityRules();
        
        if (enableDebugLogs)
            Debug.Log("CompatibilityManager: 初期化完了");
    }
    
    /// <summary>
    /// デフォルトの互換性ルールを登録
    /// </summary>
    private void RegisterDefaultCompatibilityRules()
    {
        // GameManager関連のルール
        RegisterCompatibilityRule("GameManager_Instance", "GameManagerのインスタンス存在チェック",
            () => GameManager.Instance != null,
            () => { if (GameManager.Instance == null) GameManager.GetOrCreateInstance(); },
            true, "GameManager");
            
        RegisterCompatibilityRule("GameManagerNew_Instance", "GameManagerNewのインスタンス存在チェック",
            () => GameManagerNew.Instance != null,
            () => { /* GameManagerNew.Instance will be set by its own Awake method */ },
            true, "GameManager");
            
        // UI関連のルール
        RegisterCompatibilityRule("UIManager_Instance", "UIManagerのインスタンス存在チェック",
            () => UIManager.Instance != null,
            () => { if (UIManager.Instance == null) UIManager.GetOrCreateInstance(); },
            true, "UI");
            
        // システム関連のルール
        RegisterCompatibilityRule("SystemIntegration_Instance", "SystemIntegrationManagerのインスタンス存在チェック",
            () => SystemIntegrationManager.Instance != null,
            () => { /* SystemIntegrationManager.Instance will be set by its own Awake method */ },
            true, "System");
            
        // イベントチャンネル関連のルール
        RegisterCompatibilityRule("EventChannels_Resources", "イベントチャンネルのリソース存在チェック",
            () => Resources.Load<SaveSystem.SaveEventChannel>("SO/SaveSystem/SaveEventChannel") != null,
            null, false, "EventChannel");
            
        RegisterCompatibilityRule("DeckEventChannel_Resources", "デッキイベントチャンネルのリソース存在チェック",
            () => Resources.Load<DeckSystem.DeckEventChannel>("SO/DeckSystem/DeckEventChannel") != null,
            null, false, "EventChannel");
            
        RegisterCompatibilityRule("UIEventChannel_Resources", "UIイベントチャンネルのリソース存在チェック",
            () => Resources.Load<UISystem.UIEventChannel>("SO/UISystem/UIEventChannel") != null,
            null, false, "EventChannel");
    }
    
    /// <summary>
    /// 互換性ルールを登録
    /// </summary>
    /// <param name="ruleId">ルールID</param>
    /// <param name="description">説明</param>
    /// <param name="checkFunction">チェック関数</param>
    /// <param name="fixFunction">修正関数</param>
    /// <param name="isCritical">重要度</param>
    /// <param name="category">カテゴリ</param>
    public void RegisterCompatibilityRule(string ruleId, string description, Func<bool> checkFunction, Action fixFunction = null, bool isCritical = false, string category = "General")
    {
        var rule = new CompatibilityRule(ruleId, description, checkFunction, fixFunction, isCritical, category);
        compatibilityRules.Add(rule);
        
        if (enableDebugLogs)
            Debug.Log($"CompatibilityManager: 互換性ルールを登録しました - {ruleId}");
    }
    
    /// <summary>
    /// 特定の機能の互換性をチェック
    /// </summary>
    /// <param name="featureId">機能ID</param>
    /// <returns>互換性チェック結果</returns>
    public CompatibilityCheckResult CheckFeatureCompatibility(string featureId)
    {
        if (!enableCompatibilityChecks) return null;
        
        var result = new CompatibilityCheckResult(featureId);
        
        // 該当するルールを実行
        foreach (var rule in compatibilityRules)
        {
            try
            {
                bool isCompatible = rule.checkFunction();
                
                if (!isCompatible)
                {
                    result.isCompatible = false;
                    result.issues.Add($"ルール '{rule.ruleId}': {rule.description}");
                    
                    // 自動修正が有効で修正関数がある場合
                    if (autoFixCompatibilityIssues && rule.fixFunction != null)
                    {
                        try
                        {
                            rule.fixFunction();
                            result.warnings.Add($"自動修正実行: {rule.ruleId}");
                            
                            // 修正後に再チェック
                            if (rule.checkFunction())
                            {
                                result.issues.Remove($"ルール '{rule.ruleId}': {rule.description}");
                                OnCompatibilityIssueFixed?.Invoke(rule.ruleId);
                            }
                        }
                        catch (System.Exception e)
                        {
                            result.issues.Add($"自動修正失敗: {rule.ruleId} - {e.Message}");
                        }
                    }
                    
                    if (rule.isCritical)
                    {
                        result.status = "Critical Issue";
                    }
                }
            }
            catch (System.Exception e)
            {
                result.isCompatible = false;
                result.issues.Add($"ルール実行エラー '{rule.ruleId}': {e.Message}");
            }
        }
        
        result.status = result.isCompatible ? "Compatible" : "Incompatible";
        compatibilityResults[featureId] = result;
        
        OnCompatibilityCheckCompleted?.Invoke(featureId);
        
        if (!result.isCompatible)
        {
            OnCompatibilityIssueFound?.Invoke(featureId, string.Join(", ", result.issues));
        }
        
        if (enableDebugLogs)
        {
            if (result.isCompatible)
                Debug.Log($"CompatibilityManager: 互換性チェック完了 - {featureId}: 互換");
            else
                Debug.LogWarning($"CompatibilityManager: 互換性チェック完了 - {featureId}: 非互換 - {string.Join(", ", result.issues)}");
        }
        
        return result;
    }
    
    /// <summary>
    /// 全ての機能の互換性をチェック
    /// </summary>
    /// <returns>全チェック結果</returns>
    public Dictionary<string, CompatibilityCheckResult> CheckAllCompatibility()
    {
        var results = new Dictionary<string, CompatibilityCheckResult>();
        
        // 主要な機能をチェック
        string[] features = {
            "GameManager",
            "UISystem", 
            "DeckSystem",
            "SaveSystem",
            "BattleSystem",
            "FloorSystem"
        };
        
        foreach (var feature in features)
        {
            results[feature] = CheckFeatureCompatibility(feature);
        }
        
        OnAllCompatibilityChecksCompleted?.Invoke();
        
        if (enableDebugLogs)
            Debug.Log($"CompatibilityManager: 全互換性チェック完了 - {results.Count}機能");
        
        return results;
    }
    
    /// <summary>
    /// 特定の機能の互換性結果を取得
    /// </summary>
    /// <param name="featureId">機能ID</param>
    /// <returns>互換性チェック結果</returns>
    public CompatibilityCheckResult GetCompatibilityResult(string featureId)
    {
        return compatibilityResults.ContainsKey(featureId) ? compatibilityResults[featureId] : null;
    }
    
    /// <summary>
    /// 互換性問題を手動修正
    /// </summary>
    /// <param name="featureId">機能ID</param>
    /// <param name="ruleId">ルールID</param>
    /// <returns>修正成功したかどうか</returns>
    public bool FixCompatibilityIssue(string featureId, string ruleId)
    {
        var rule = compatibilityRules.Find(r => r.ruleId == ruleId);
        if (rule == null || rule.fixFunction == null) return false;
        
        try
        {
            rule.fixFunction();
            
            // 修正後に再チェック
            if (rule.checkFunction())
            {
                OnCompatibilityIssueFixed?.Invoke(ruleId);
                
                if (enableDebugLogs)
                    Debug.Log($"CompatibilityManager: 互換性問題を修正しました - {featureId}: {ruleId}");
                
                return true;
            }
        }
        catch (System.Exception e)
        {
            if (enableDebugLogs)
                Debug.LogError($"CompatibilityManager: 互換性問題の修正に失敗しました - {featureId}: {ruleId} - {e.Message}");
        }
        
        return false;
    }
    
    /// <summary>
    /// 新機能追加前の互換性チェック
    /// </summary>
    /// <param name="newFeatureId">新機能ID</param>
    /// <param name="dependencies">依存関係</param>
    /// <returns>互換性チェック結果</returns>
    public CompatibilityCheckResult CheckNewFeatureCompatibility(string newFeatureId, string[] dependencies)
    {
        var result = new CompatibilityCheckResult(newFeatureId);
        
        // 依存関係のチェック
        foreach (var dependency in dependencies)
        {
            var depResult = GetCompatibilityResult(dependency);
            if (depResult != null && !depResult.isCompatible)
            {
                result.isCompatible = false;
                result.issues.Add($"依存関係 '{dependency}' に互換性問題があります");
            }
        }
        
        // 新機能固有のルールをチェック
        foreach (var rule in compatibilityRules)
        {
            if (rule.category == "NewFeature")
            {
                try
                {
                    bool isCompatible = rule.checkFunction();
                    if (!isCompatible)
                    {
                        result.isCompatible = false;
                        result.issues.Add($"新機能ルール '{rule.ruleId}': {rule.description}");
                    }
                }
                catch (System.Exception e)
                {
                    result.isCompatible = false;
                    result.issues.Add($"新機能ルール実行エラー '{rule.ruleId}': {e.Message}");
                }
            }
        }
        
        result.status = result.isCompatible ? "Compatible" : "Incompatible";
        compatibilityResults[newFeatureId] = result;
        
        OnCompatibilityCheckCompleted?.Invoke(newFeatureId);
        
        if (enableDebugLogs)
        {
            if (result.isCompatible)
                Debug.Log($"CompatibilityManager: 新機能互換性チェック完了 - {newFeatureId}: 互換");
            else
                Debug.LogWarning($"CompatibilityManager: 新機能互換性チェック完了 - {newFeatureId}: 非互換 - {string.Join(", ", result.issues)}");
        }
        
        return result;
    }
    
    /// <summary>
    /// 互換性レポートを生成
    /// </summary>
    /// <returns>レポート文字列</returns>
    public string GenerateCompatibilityReport()
    {
        var report = "=== 互換性レポート ===\n";
        report += $"生成時刻: {DateTime.Now}\n";
        report += $"チェック済み機能数: {compatibilityResults.Count}\n\n";
        
        foreach (var kvp in compatibilityResults)
        {
            var result = kvp.Value;
            report += $"機能: {result.featureId}\n";
            report += $"状態: {result.status}\n";
            report += $"互換性: {(result.isCompatible ? "○" : "×")}\n";
            
            if (result.issues.Count > 0)
            {
                report += "問題:\n";
                foreach (var issue in result.issues)
                {
                    report += $"  - {issue}\n";
                }
            }
            
            if (result.warnings.Count > 0)
            {
                report += "警告:\n";
                foreach (var warning in result.warnings)
                {
                    report += $"  - {warning}\n";
                }
            }
            
            report += "\n";
        }
        
        return report;
    }
    
    /// <summary>
    /// デバッグ情報を取得
    /// </summary>
    public string GetDebugInfo()
    {
        var info = $"CompatibilityManager Debug Info:\n";
        info += $"Compatibility Checks: {enableCompatibilityChecks}\n";
        info += $"Auto Fix: {autoFixCompatibilityIssues}\n";
        info += $"Rules Count: {compatibilityRules.Count}\n";
        info += $"Results Count: {compatibilityResults.Count}\n";
        info += $"Categories:\n";
        
        var categories = new HashSet<string>();
        foreach (var rule in compatibilityRules)
        {
            categories.Add(rule.category);
        }
        
        foreach (var category in categories)
        {
            var count = compatibilityRules.FindAll(r => r.category == category).Count;
            info += $"  {category}: {count} rules\n";
        }
        
        return info;
    }
} 