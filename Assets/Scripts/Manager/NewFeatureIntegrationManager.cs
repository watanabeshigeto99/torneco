using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 新機能追加時の統合管理専用クラス
/// 責務：新機能の統合と既存システムとの互換性保証のみ
/// </summary>
[DefaultExecutionOrder(-65)]
public class NewFeatureIntegrationManager : MonoBehaviour
{
    public static NewFeatureIntegrationManager Instance { get; private set; }
    
    [Header("New Feature Integration Settings")]
    [SerializeField] private bool enableIntegrationChecks = true;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private bool autoResolveConflicts = false;
    
    // 新機能の登録情報
    private Dictionary<string, NewFeatureInfo> registeredFeatures = new Dictionary<string, NewFeatureInfo>();
    private List<IntegrationRule> integrationRules = new List<IntegrationRule>();
    
    // イベント定義
    public static event Action<string> OnNewFeatureRegistered;
    public static event Action<string> OnNewFeatureIntegrated;
    public static event Action<string, string> OnIntegrationConflictFound;
    public static event Action<string> OnIntegrationConflictResolved;
    
    /// <summary>
    /// 新機能情報
    /// </summary>
    [System.Serializable]
    public class NewFeatureInfo
    {
        public string featureId;
        public string description;
        public string[] dependencies;
        public string[] conflicts;
        public bool isActive;
        public float registrationTime;
        public string status;
        
        public NewFeatureInfo(string id, string desc, string[] deps = null, string[] confs = null)
        {
            featureId = id;
            description = desc;
            dependencies = deps ?? new string[0];
            conflicts = confs ?? new string[0];
            isActive = false;
            registrationTime = Time.time;
            status = "Registered";
        }
    }
    
    /// <summary>
    /// 統合ルール
    /// </summary>
    [System.Serializable]
    public class IntegrationRule
    {
        public string ruleId;
        public string description;
        public Func<string, bool> checkFunction;
        public Action<string> resolveFunction;
        public bool isCritical;
        public string category;
        
        public IntegrationRule(string id, string desc, Func<string, bool> check, Action<string> resolve = null, bool critical = false, string cat = "General")
        {
            ruleId = id;
            description = desc;
            checkFunction = check;
            resolveFunction = resolve;
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
        
        InitializeNewFeatureIntegrationManager();
    }
    
    /// <summary>
    /// 新機能統合マネージャーの初期化
    /// </summary>
    private void InitializeNewFeatureIntegrationManager()
    {
        registeredFeatures.Clear();
        integrationRules.Clear();
        
        // 基本的な統合ルールを登録
        RegisterDefaultIntegrationRules();
        
        if (enableDebugLogs)
            Debug.Log("NewFeatureIntegrationManager: 初期化完了");
    }
    
    /// <summary>
    /// デフォルトの統合ルールを登録
    /// </summary>
    private void RegisterDefaultIntegrationRules()
    {
        // 依存関係チェックルール
        RegisterIntegrationRule("DependencyCheck", "依存関係の存在チェック",
            (featureId) => {
                var feature = registeredFeatures.ContainsKey(featureId) ? registeredFeatures[featureId] : null;
                if (feature == null) return false;
                
                foreach (var dependency in feature.dependencies)
                {
                    if (!registeredFeatures.ContainsKey(dependency))
                        return false;
                }
                return true;
            },
            (featureId) => {
                var feature = registeredFeatures[featureId];
                foreach (var dependency in feature.dependencies)
                {
                    if (!registeredFeatures.ContainsKey(dependency))
                    {
                        Debug.LogWarning($"NewFeatureIntegrationManager: 依存関係 '{dependency}' が見つかりません");
                    }
                }
            },
            true, "Dependency");
            
        // 競合チェックルール
        RegisterIntegrationRule("ConflictCheck", "機能間の競合チェック",
            (featureId) => {
                var feature = registeredFeatures.ContainsKey(featureId) ? registeredFeatures[featureId] : null;
                if (feature == null) return false;
                
                foreach (var conflict in feature.conflicts)
                {
                    if (registeredFeatures.ContainsKey(conflict) && registeredFeatures[conflict].isActive)
                        return false;
                }
                return true;
            },
            (featureId) => {
                var feature = registeredFeatures[featureId];
                foreach (var conflict in feature.conflicts)
                {
                    if (registeredFeatures.ContainsKey(conflict) && registeredFeatures[conflict].isActive)
                    {
                        Debug.LogWarning($"NewFeatureIntegrationManager: 競合する機能 '{conflict}' がアクティブです");
                    }
                }
            },
            true, "Conflict");
            
        // システム互換性チェックルール（CompatibilityManager削除により簡略化）
        RegisterIntegrationRule("SystemCompatibilityCheck", "システム互換性チェック",
            (featureId) => {
                // CompatibilityManagerが削除されたため、常に互換性ありとみなす
                return true;
            },
            (featureId) => {
                // 互換性チェックは不要
            },
            false, "Compatibility");
    }
    
    /// <summary>
    /// 統合ルールを登録
    /// </summary>
    /// <param name="ruleId">ルールID</param>
    /// <param name="description">説明</param>
    /// <param name="checkFunction">チェック関数</param>
    /// <param name="resolveFunction">解決関数</param>
    /// <param name="isCritical">重要度</param>
    /// <param name="category">カテゴリ</param>
    public void RegisterIntegrationRule(string ruleId, string description, Func<string, bool> checkFunction, Action<string> resolveFunction = null, bool isCritical = false, string category = "General")
    {
        var rule = new IntegrationRule(ruleId, description, checkFunction, resolveFunction, isCritical, category);
        integrationRules.Add(rule);
        
        if (enableDebugLogs)
            Debug.Log($"NewFeatureIntegrationManager: 統合ルールを登録しました - {ruleId}");
    }
    
    /// <summary>
    /// 新機能を登録
    /// </summary>
    /// <param name="featureId">機能ID</param>
    /// <param name="description">説明</param>
    /// <param name="dependencies">依存関係</param>
    /// <param name="conflicts">競合関係</param>
    /// <returns>登録成功したかどうか</returns>
    public bool RegisterNewFeature(string featureId, string description, string[] dependencies = null, string[] conflicts = null)
    {
        if (registeredFeatures.ContainsKey(featureId))
        {
            Debug.LogWarning($"NewFeatureIntegrationManager: 機能 '{featureId}' は既に登録されています");
            return false;
        }
        
        var feature = new NewFeatureInfo(featureId, description, dependencies, conflicts);
        registeredFeatures[featureId] = feature;
        
        OnNewFeatureRegistered?.Invoke(featureId);
        
        if (enableDebugLogs)
            Debug.Log($"NewFeatureIntegrationManager: 新機能を登録しました - {featureId}: {description}");
        
        return true;
    }
    
    /// <summary>
    /// 新機能を統合
    /// </summary>
    /// <param name="featureId">機能ID</param>
    /// <returns>統合成功したかどうか</returns>
    public bool IntegrateNewFeature(string featureId)
    {
        if (!registeredFeatures.ContainsKey(featureId))
        {
            Debug.LogError($"NewFeatureIntegrationManager: 機能 '{featureId}' が登録されていません");
            return false;
        }
        
        var feature = registeredFeatures[featureId];
        
        // 統合ルールをチェック
        bool canIntegrate = true;
        List<string> conflicts = new List<string>();
        
        foreach (var rule in integrationRules)
        {
            try
            {
                bool isCompatible = rule.checkFunction(featureId);
                
                if (!isCompatible)
                {
                    canIntegrate = false;
                    conflicts.Add($"ルール '{rule.ruleId}': {rule.description}");
                    
                    // 自動解決が有効で解決関数がある場合
                    if (autoResolveConflicts && rule.resolveFunction != null)
                    {
                        try
                        {
                            rule.resolveFunction(featureId);
                            
                            // 解決後に再チェック
                            if (rule.checkFunction(featureId))
                            {
                                conflicts.Remove($"ルール '{rule.ruleId}': {rule.description}");
                                OnIntegrationConflictResolved?.Invoke(rule.ruleId);
                            }
                        }
                        catch (System.Exception e)
                        {
                            conflicts.Add($"自動解決失敗: {rule.ruleId} - {e.Message}");
                        }
                    }
                    
                    if (rule.isCritical)
                    {
                        feature.status = "Critical Conflict";
                    }
                }
            }
            catch (System.Exception e)
            {
                canIntegrate = false;
                conflicts.Add($"ルール実行エラー '{rule.ruleId}': {e.Message}");
            }
        }
        
        if (canIntegrate)
        {
            feature.isActive = true;
            feature.status = "Integrated";
            OnNewFeatureIntegrated?.Invoke(featureId);
            
            if (enableDebugLogs)
                Debug.Log($"NewFeatureIntegrationManager: 新機能を統合しました - {featureId}");
        }
        else
        {
            feature.status = "Integration Failed";
            OnIntegrationConflictFound?.Invoke(featureId, string.Join(", ", conflicts));
            
            if (enableDebugLogs)
                Debug.LogWarning($"NewFeatureIntegrationManager: 新機能の統合に失敗しました - {featureId}: {string.Join(", ", conflicts)}");
        }
        
        return canIntegrate;
    }
    
    /// <summary>
    /// 新機能を無効化
    /// </summary>
    /// <param name="featureId">機能ID</param>
    public void DisableNewFeature(string featureId)
    {
        if (registeredFeatures.ContainsKey(featureId))
        {
            registeredFeatures[featureId].isActive = false;
            registeredFeatures[featureId].status = "Disabled";
            
            if (enableDebugLogs)
                Debug.Log($"NewFeatureIntegrationManager: 新機能を無効化しました - {featureId}");
        }
    }
    
    /// <summary>
    /// 新機能を削除
    /// </summary>
    /// <param name="featureId">機能ID</param>
    public void RemoveNewFeature(string featureId)
    {
        if (registeredFeatures.ContainsKey(featureId))
        {
            registeredFeatures.Remove(featureId);
            
            if (enableDebugLogs)
                Debug.Log($"NewFeatureIntegrationManager: 新機能を削除しました - {featureId}");
        }
    }
    
    /// <summary>
    /// 新機能の情報を取得
    /// </summary>
    /// <param name="featureId">機能ID</param>
    /// <returns>新機能情報</returns>
    public NewFeatureInfo GetNewFeatureInfo(string featureId)
    {
        return registeredFeatures.ContainsKey(featureId) ? registeredFeatures[featureId] : null;
    }
    
    /// <summary>
    /// 全ての新機能情報を取得
    /// </summary>
    /// <returns>全新機能情報</returns>
    public Dictionary<string, NewFeatureInfo> GetAllNewFeatureInfo()
    {
        return new Dictionary<string, NewFeatureInfo>(registeredFeatures);
    }
    
    /// <summary>
    /// 新機能統合レポートを生成
    /// </summary>
    /// <returns>レポート文字列</returns>
    public string GenerateIntegrationReport()
    {
        var report = "=== 新機能統合レポート ===\n";
        report += $"生成時刻: {DateTime.Now}\n";
        report += $"登録済み機能数: {registeredFeatures.Count}\n";
        report += $"アクティブ機能数: {registeredFeatures.Values.Count(f => f.isActive)}\n\n";
        
        foreach (var kvp in registeredFeatures)
        {
            var feature = kvp.Value;
            report += $"機能: {feature.featureId}\n";
            report += $"説明: {feature.description}\n";
            report += $"状態: {feature.status}\n";
            report += $"アクティブ: {(feature.isActive ? "○" : "×")}\n";
            
            if (feature.dependencies.Length > 0)
            {
                report += "依存関係:\n";
                foreach (var dep in feature.dependencies)
                {
                    report += $"  - {dep}\n";
                }
            }
            
            if (feature.conflicts.Length > 0)
            {
                report += "競合関係:\n";
                foreach (var conflict in feature.conflicts)
                {
                    report += $"  - {conflict}\n";
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
        var info = $"NewFeatureIntegrationManager Debug Info:\n";
        info += $"Integration Checks: {enableIntegrationChecks}\n";
        info += $"Auto Resolve: {autoResolveConflicts}\n";
        info += $"Registered Features: {registeredFeatures.Count}\n";
        info += $"Integration Rules: {integrationRules.Count}\n";
        info += $"Active Features:\n";
        
        foreach (var kvp in registeredFeatures)
        {
            var feature = kvp.Value;
            info += $"  {feature.featureId}: {(feature.isActive ? "Active" : "Inactive")} - {feature.status}\n";
        }
        
        return info;
    }
} 