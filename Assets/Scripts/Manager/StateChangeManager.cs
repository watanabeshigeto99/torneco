using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 状態変化の影響範囲管理専用クラス
/// 責務：状態変化の影響範囲追跡と通知のみ
/// </summary>
[DefaultExecutionOrder(-75)]
public class StateChangeManager : MonoBehaviour
{
    public static StateChangeManager Instance { get; private set; }
    
    [Header("State Change Settings")]
    [SerializeField] private bool enableStateTracking = true;
    [SerializeField] private bool enableDebugLogs = true;
    [SerializeField] private int maxStateHistory = 100;
    
    // 状態変化の履歴
    private Queue<StateChangeRecord> stateHistory = new Queue<StateChangeRecord>();
    private Dictionary<string, StateChangeListener> stateListeners = new Dictionary<string, StateChangeListener>();
    
    // イベント定義
    public static event Action<string, object, object> OnStateChanged;
    public static event Action<string> OnStateChangeReverted;
    public static event Action OnAllStatesReset;
    
    /// <summary>
    /// 状態変化記録
    /// </summary>
    [System.Serializable]
    public class StateChangeRecord
    {
        public string stateId;
        public object oldValue;
        public object newValue;
        public float timestamp;
        public string source;
        public string description;
        
        public StateChangeRecord(string id, object oldVal, object newVal, string src, string desc = "")
        {
            stateId = id;
            oldValue = oldVal;
            newValue = newVal;
            timestamp = Time.time;
            source = src;
            description = desc;
        }
    }
    
    /// <summary>
    /// 状態変化リスナー
    /// </summary>
    [System.Serializable]
    public class StateChangeListener
    {
        public string stateId;
        public Action<object, object> onStateChanged;
        public Action<string> onStateReverted;
        public bool isActive;
        
        public StateChangeListener(string id, Action<object, object> changed, Action<string> reverted = null)
        {
            stateId = id;
            onStateChanged = changed;
            onStateReverted = reverted;
            isActive = true;
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
        
        InitializeStateChangeManager();
    }
    
    /// <summary>
    /// 状態変化マネージャーの初期化
    /// </summary>
    private void InitializeStateChangeManager()
    {
        stateHistory.Clear();
        stateListeners.Clear();
        
        if (enableDebugLogs)
            Debug.Log("StateChangeManager: 初期化完了");
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
        if (!enableStateTracking) return;
        
        var record = new StateChangeRecord(stateId, oldValue, newValue, source, description);
        
        // 履歴に追加
        stateHistory.Enqueue(record);
        
        // 履歴サイズを制限
        if (stateHistory.Count > maxStateHistory)
        {
            stateHistory.Dequeue();
        }
        
        // リスナーに通知
        NotifyStateChangeListeners(stateId, oldValue, newValue);
        
        // イベント発行
        OnStateChanged?.Invoke(stateId, oldValue, newValue);
        
        if (enableDebugLogs)
            Debug.Log($"StateChangeManager: 状態変化を記録しました - {stateId}: {oldValue} -> {newValue} (原因: {source})");
    }
    
    /// <summary>
    /// 状態変化リスナーを登録
    /// </summary>
    /// <param name="stateId">監視する状態ID</param>
    /// <param name="onStateChanged">状態変化時のコールバック</param>
    /// <param name="onStateReverted">状態復元時のコールバック</param>
    public void RegisterStateListener(string stateId, Action<object, object> onStateChanged, Action<string> onStateReverted = null)
    {
        var listener = new StateChangeListener(stateId, onStateChanged, onStateReverted);
        stateListeners[stateId] = listener;
        
        if (enableDebugLogs)
            Debug.Log($"StateChangeManager: 状態リスナーを登録しました - {stateId}");
    }
    
    /// <summary>
    /// 状態変化リスナーを解除
    /// </summary>
    /// <param name="stateId">解除する状態ID</param>
    public void UnregisterStateListener(string stateId)
    {
        if (stateListeners.ContainsKey(stateId))
        {
            stateListeners.Remove(stateId);
            
            if (enableDebugLogs)
                Debug.Log($"StateChangeManager: 状態リスナーを解除しました - {stateId}");
        }
    }
    
    /// <summary>
    /// 状態変化リスナーに通知
    /// </summary>
    private void NotifyStateChangeListeners(string stateId, object oldValue, object newValue)
    {
        if (stateListeners.ContainsKey(stateId) && stateListeners[stateId].isActive)
        {
            stateListeners[stateId].onStateChanged?.Invoke(oldValue, newValue);
        }
    }
    
    /// <summary>
    /// 状態を特定の値に戻す
    /// </summary>
    /// <param name="stateId">戻す状態ID</param>
    /// <param name="targetValue">目標値</param>
    /// <param name="source">復元の原因</param>
    public void RevertState(string stateId, object targetValue, string source)
    {
        if (!enableStateTracking) return;
        
        // 履歴から該当する状態を検索
        StateChangeRecord targetRecord = null;
        foreach (var record in stateHistory)
        {
            if (record.stateId == stateId && record.oldValue.Equals(targetValue))
            {
                targetRecord = record;
                break;
            }
        }
        
        if (targetRecord != null)
        {
            RecordStateChange(stateId, GetCurrentStateValue(stateId), targetValue, source, $"状態復元: {targetRecord.description}");
            
            // 復元イベントを発行
            OnStateChangeReverted?.Invoke(stateId);
            
            if (enableDebugLogs)
                Debug.Log($"StateChangeManager: 状態を復元しました - {stateId} -> {targetValue}");
        }
        else
        {
            if (enableDebugLogs)
                Debug.LogWarning($"StateChangeManager: 復元対象の状態が見つかりません - {stateId}");
        }
    }
    
    /// <summary>
    /// 現在の状態値を取得
    /// </summary>
    /// <param name="stateId">状態ID</param>
    /// <returns>現在の値</returns>
    public object GetCurrentStateValue(string stateId)
    {
        // 履歴から最新の値を取得
        StateChangeRecord latestRecord = null;
        foreach (var record in stateHistory)
        {
            if (record.stateId == stateId)
            {
                if (latestRecord == null || record.timestamp > latestRecord.timestamp)
                {
                    latestRecord = record;
                }
            }
        }
        
        return latestRecord?.newValue;
    }
    
    /// <summary>
    /// 状態変化の履歴を取得
    /// </summary>
    /// <param name="stateId">状態ID</param>
    /// <param name="count">取得する履歴数</param>
    /// <returns>状態変化履歴</returns>
    public List<StateChangeRecord> GetStateHistory(string stateId, int count = 10)
    {
        var history = new List<StateChangeRecord>();
        var records = stateHistory.ToArray();
        
        // 新しい順に取得
        for (int i = records.Length - 1; i >= 0 && history.Count < count; i--)
        {
            if (records[i].stateId == stateId)
            {
                history.Add(records[i]);
            }
        }
        
        return history;
    }
    
    /// <summary>
    /// 全ての状態をリセット
    /// </summary>
    public void ResetAllStates()
    {
        stateHistory.Clear();
        
        // 全てのリスナーを無効化
        foreach (var listener in stateListeners.Values)
        {
            listener.isActive = false;
        }
        
        OnAllStatesReset?.Invoke();
        
        if (enableDebugLogs)
            Debug.Log("StateChangeManager: 全状態をリセットしました");
    }
    
    /// <summary>
    /// 状態リスナーを有効化
    /// </summary>
    /// <param name="stateId">状態ID</param>
    public void EnableStateListener(string stateId)
    {
        if (stateListeners.ContainsKey(stateId))
        {
            stateListeners[stateId].isActive = true;
        }
    }
    
    /// <summary>
    /// 状態リスナーを無効化
    /// </summary>
    /// <param name="stateId">状態ID</param>
    public void DisableStateListener(string stateId)
    {
        if (stateListeners.ContainsKey(stateId))
        {
            stateListeners[stateId].isActive = false;
        }
    }
    
    /// <summary>
    /// 状態変化の影響範囲を分析
    /// </summary>
    /// <param name="stateId">分析する状態ID</param>
    /// <returns>影響範囲の分析結果</returns>
    public string AnalyzeStateImpact(string stateId)
    {
        var analysis = $"状態変化影響分析 - {stateId}\n";
        analysis += $"履歴数: {GetStateHistory(stateId).Count}\n";
        analysis += $"リスナー登録: {stateListeners.ContainsKey(stateId)}\n";
        
        var history = GetStateHistory(stateId, 5);
        analysis += "最近の変化:\n";
        foreach (var record in history)
        {
            analysis += $"  {record.timestamp:F2}s: {record.oldValue} -> {record.newValue} (原因: {record.source})\n";
        }
        
        return analysis;
    }
    
    /// <summary>
    /// デバッグ情報を取得
    /// </summary>
    public string GetDebugInfo()
    {
        var info = $"StateChangeManager Debug Info:\n";
        info += $"State Tracking: {enableStateTracking}\n";
        info += $"History Count: {stateHistory.Count}\n";
        info += $"Listener Count: {stateListeners.Count}\n";
        info += $"Active Listeners:\n";
        
        foreach (var kvp in stateListeners)
        {
            info += $"  {kvp.Key}: {(kvp.Value.isActive ? "Active" : "Inactive")}\n";
        }
        
        return info;
    }
} 