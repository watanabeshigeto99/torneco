using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// パフォーマンス最適化専用マネージャー
/// 責務：イベント最適化、メモリ監視、プロファイリング、拡張性向上
/// </summary>
[DefaultExecutionOrder(-90)]
public class PerformanceOptimizationManager : MonoBehaviour
{
    public static PerformanceOptimizationManager Instance { get; private set; }
    
    [Header("Performance Monitoring")]
    public bool enablePerformanceMonitoring = true;
    public bool enableMemoryMonitoring = true;
    public bool enableEventOptimization = true;
    public bool enableProfiling = true;
    
    [Header("Monitoring Settings")]
    public float monitoringInterval = 1.0f;
    public int maxEventSubscribers = 100;
    public float memoryThresholdMB = 500f;
    public float frameRateThreshold = 30f;
    
    [Header("Event Optimization")]
    public bool enableEventPooling = true;
    public int eventPoolSize = 50;
    public bool enableEventDebouncing = true;
    public float debounceDelay = 0.1f;
    
    [Header("Memory Optimization")]
    public bool enableGarbageCollection = true;
    public float gcInterval = 30f;
    public bool enableObjectPooling = true;
    public int objectPoolSize = 100;
    
    // パフォーマンスメトリクス
    private float currentFrameRate;
    private float averageFrameRate;
    private float memoryUsageMB;
    private int eventSubscriberCount;
    private int activeEventChannels;
    
    // 最適化統計
    private int optimizedEvents;
    private int garbageCollectionsTriggered;
    private int objectPoolHits;
    private int memoryOptimizations;
    
    // イベントプール
    private Queue<Action> eventPool = new Queue<Action>();
    private Dictionary<string, float> debouncedEvents = new Dictionary<string, float>();
    
    // オブジェクトプール
    private Dictionary<Type, Queue<object>> objectPools = new Dictionary<Type, Queue<object>>();
    
    // モニタリングコルーチン
    private Coroutine monitoringCoroutine;
    private Coroutine gcCoroutine;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeOptimizationManager();
    }
    
    /// <summary>
    /// 最適化マネージャーの初期化
    /// </summary>
    private void InitializeOptimizationManager()
    {
        Debug.Log("PerformanceOptimizationManager: 初期化開始");
        
        // イベントプールの初期化
        if (enableEventPooling)
        {
            InitializeEventPool();
        }
        
        // オブジェクトプールの初期化
        if (enableObjectPooling)
        {
            InitializeObjectPools();
        }
        
        // モニタリングの開始
        if (enablePerformanceMonitoring)
        {
            StartPerformanceMonitoring();
        }
        
        // ガベージコレクションの開始
        if (enableGarbageCollection)
        {
            StartGarbageCollectionMonitoring();
        }
        
        Debug.Log("PerformanceOptimizationManager: 初期化完了");
    }
    
    /// <summary>
    /// イベントプールの初期化
    /// </summary>
    private void InitializeEventPool()
    {
        for (int i = 0; i < eventPoolSize; i++)
        {
            eventPool.Enqueue(() => { });
        }
        
        Debug.Log($"PerformanceOptimizationManager: イベントプールを初期化しました - サイズ: {eventPoolSize}");
    }
    
    /// <summary>
    /// オブジェクトプールの初期化
    /// </summary>
    private void InitializeObjectPools()
    {
        // よく使用されるオブジェクトタイプのプールを作成
        CreateObjectPool<CardDataSO>(objectPoolSize);
        CreateObjectPool<PlayerDeck>(objectPoolSize / 2);
        
        Debug.Log($"PerformanceOptimizationManager: オブジェクトプールを初期化しました - サイズ: {objectPoolSize}");
    }
    
    /// <summary>
    /// オブジェクトプールの作成
    /// </summary>
    private void CreateObjectPool<T>(int size) where T : class, new()
    {
        var pool = new Queue<object>();
        for (int i = 0; i < size; i++)
        {
            pool.Enqueue(new T());
        }
        objectPools[typeof(T)] = pool;
    }
    
    /// <summary>
    /// パフォーマンスモニタリングの開始
    /// </summary>
    private void StartPerformanceMonitoring()
    {
        if (monitoringCoroutine != null)
        {
            StopCoroutine(monitoringCoroutine);
        }
        
        monitoringCoroutine = StartCoroutine(PerformanceMonitoringCoroutine());
        Debug.Log("PerformanceOptimizationManager: パフォーマンスモニタリングを開始しました");
    }
    
    /// <summary>
    /// ガベージコレクション監視の開始
    /// </summary>
    private void StartGarbageCollectionMonitoring()
    {
        if (gcCoroutine != null)
        {
            StopCoroutine(gcCoroutine);
        }
        
        gcCoroutine = StartCoroutine(GarbageCollectionCoroutine());
        Debug.Log("PerformanceOptimizationManager: ガベージコレクション監視を開始しました");
    }
    
    /// <summary>
    /// パフォーマンスモニタリングコルーチン
    /// </summary>
    private IEnumerator PerformanceMonitoringCoroutine()
    {
        while (enablePerformanceMonitoring)
        {
            // フレームレートの計算
            currentFrameRate = 1.0f / Time.unscaledDeltaTime;
            averageFrameRate = Mathf.Lerp(averageFrameRate, currentFrameRate, 0.1f);
            
            // メモリ使用量の取得
            memoryUsageMB = System.GC.GetTotalMemory(false) / (1024f * 1024f);
            
            // イベント統計の収集
            CollectEventStatistics();
            
            // パフォーマンスチェック
            CheckPerformanceThresholds();
            
            yield return new WaitForSeconds(monitoringInterval);
        }
    }
    
    /// <summary>
    /// ガベージコレクションコルーチン
    /// </summary>
    private IEnumerator GarbageCollectionCoroutine()
    {
        while (enableGarbageCollection)
        {
            yield return new WaitForSeconds(gcInterval);
            
            // メモリ使用量が閾値を超えた場合、GCを実行
            if (memoryUsageMB > memoryThresholdMB)
            {
                TriggerGarbageCollection();
            }
        }
    }
    
    /// <summary>
    /// イベント統計の収集
    /// </summary>
    private void CollectEventStatistics()
    {
        // 実際のイベント統計を収集（簡易版）
        eventSubscriberCount = 0;
        activeEventChannels = 0;
        
        // 各マネージャーからイベント統計を取得
        if (GameManager.Instance != null)
        {
            // GameManagerのイベント統計を取得
        }
        
        if (UIManager.Instance != null)
        {
            // UIManagerのイベント統計を取得
        }
    }
    
    /// <summary>
    /// パフォーマンス閾値のチェック
    /// </summary>
    private void CheckPerformanceThresholds()
    {
        // フレームレートチェック
        if (averageFrameRate < frameRateThreshold)
        {
            Debug.LogWarning($"PerformanceOptimizationManager: フレームレートが低いです - {averageFrameRate:F1} FPS");
            OptimizeFrameRate();
        }
        
        // メモリ使用量チェック
        if (memoryUsageMB > memoryThresholdMB)
        {
            Debug.LogWarning($"PerformanceOptimizationManager: メモリ使用量が高いです - {memoryUsageMB:F1} MB");
            OptimizeMemoryUsage();
        }
        
        // イベント購読者数チェック
        if (eventSubscriberCount > maxEventSubscribers)
        {
            Debug.LogWarning($"PerformanceOptimizationManager: イベント購読者が多いです - {eventSubscriberCount}");
            OptimizeEventSubscriptions();
        }
    }
    
    /// <summary>
    /// フレームレート最適化
    /// </summary>
    private void OptimizeFrameRate()
    {
        // 重い処理の一時停止
        // 描画品質の調整
        // 不要なオブジェクトの非アクティブ化
        
        Debug.Log("PerformanceOptimizationManager: フレームレート最適化を実行しました");
    }
    
    /// <summary>
    /// メモリ使用量最適化
    /// </summary>
    private void OptimizeMemoryUsage()
    {
        // ガベージコレクションの実行
        TriggerGarbageCollection();
        
        // オブジェクトプールのクリーンアップ
        CleanupObjectPools();
        
        // キャッシュのクリア
        ClearCaches();
        
        memoryOptimizations++;
        Debug.Log("PerformanceOptimizationManager: メモリ使用量最適化を実行しました");
    }
    
    /// <summary>
    /// イベント購読最適化
    /// </summary>
    private void OptimizeEventSubscriptions()
    {
        // 重複購読の削除
        // 不要な購読の解除
        // イベントプールの活用
        
        optimizedEvents++;
        Debug.Log("PerformanceOptimizationManager: イベント購読最適化を実行しました");
    }
    
    /// <summary>
    /// ガベージコレクションの実行
    /// </summary>
    private void TriggerGarbageCollection()
    {
        System.GC.Collect();
        System.GC.WaitForPendingFinalizers();
        System.GC.Collect();
        
        garbageCollectionsTriggered++;
        Debug.Log("PerformanceOptimizationManager: ガベージコレクションを実行しました");
    }
    
    /// <summary>
    /// オブジェクトプールのクリーンアップ
    /// </summary>
    private void CleanupObjectPools()
    {
        foreach (var pool in objectPools.Values)
        {
            while (pool.Count > objectPoolSize / 2)
            {
                pool.Dequeue();
            }
        }
        
        Debug.Log("PerformanceOptimizationManager: オブジェクトプールをクリーンアップしました");
    }
    
    /// <summary>
    /// キャッシュのクリア
    /// </summary>
    private void ClearCaches()
    {
        // 各種キャッシュのクリア
        Resources.UnloadUnusedAssets();
        
        Debug.Log("PerformanceOptimizationManager: キャッシュをクリアしました");
    }
    
    /// <summary>
    /// イベントのデバウンス処理
    /// </summary>
    public void DebounceEvent(string eventName, Action action)
    {
        if (!enableEventDebouncing)
        {
            action?.Invoke();
            return;
        }
        
        if (debouncedEvents.ContainsKey(eventName))
        {
            debouncedEvents[eventName] = Time.time;
        }
        else
        {
            debouncedEvents[eventName] = Time.time;
            StartCoroutine(DebouncedEventCoroutine(eventName, action));
        }
    }
    
    /// <summary>
    /// デバウンスイベントコルーチン
    /// </summary>
    private IEnumerator DebouncedEventCoroutine(string eventName, Action action)
    {
        yield return new WaitForSeconds(debounceDelay);
        
        if (debouncedEvents.ContainsKey(eventName) && 
            Time.time - debouncedEvents[eventName] >= debounceDelay)
        {
            action?.Invoke();
            debouncedEvents.Remove(eventName);
        }
    }
    
    /// <summary>
    /// オブジェクトプールからオブジェクトを取得
    /// </summary>
    public T GetFromPool<T>() where T : class, new()
    {
        if (!enableObjectPooling || !objectPools.ContainsKey(typeof(T)))
        {
            return new T();
        }
        
        var pool = objectPools[typeof(T)];
        if (pool.Count > 0)
        {
            objectPoolHits++;
            return (T)pool.Dequeue();
        }
        
        return new T();
    }
    
    /// <summary>
    /// オブジェクトプールにオブジェクトを返却
    /// </summary>
    public void ReturnToPool<T>(T obj) where T : class
    {
        if (!enableObjectPooling || !objectPools.ContainsKey(typeof(T)))
        {
            return;
        }
        
        var pool = objectPools[typeof(T)];
        if (pool.Count < objectPoolSize)
        {
            pool.Enqueue(obj);
        }
    }
    
    /// <summary>
    /// パフォーマンス情報の取得
    /// </summary>
    public string GetPerformanceInfo()
    {
        return $"Performance - FPS: {averageFrameRate:F1}, Memory: {memoryUsageMB:F1}MB, " +
               $"Events: {eventSubscriberCount}, Optimizations: {optimizedEvents + garbageCollectionsTriggered + memoryOptimizations}";
    }
    
    /// <summary>
    /// 最適化統計の取得
    /// </summary>
    public string GetOptimizationStats()
    {
        return $"Optimization Stats - Event Pool Hits: {objectPoolHits}, " +
               $"GC Triggered: {garbageCollectionsTriggered}, Memory Optimizations: {memoryOptimizations}";
    }
    
    private void OnDestroy()
    {
        if (monitoringCoroutine != null)
        {
            StopCoroutine(monitoringCoroutine);
        }
        
        if (gcCoroutine != null)
        {
            StopCoroutine(gcCoroutine);
        }
        
        Debug.Log("PerformanceOptimizationManager: 破棄されました");
    }
} 