using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

/// <summary>
/// プラグインシステム専用マネージャー
/// 責務：動的機能拡張、プラグイン管理、拡張性向上
/// </summary>
[DefaultExecutionOrder(-95)]
public class PluginSystemManager : MonoBehaviour
{
    public static PluginSystemManager Instance { get; private set; }
    
    [Header("Plugin System Settings")]
    public bool enablePluginSystem = true;
    public bool enableAutoDiscovery = true;
    public bool enableHotReload = false;
    
    [Header("Plugin Directories")]
    public string[] pluginDirectories = { "Plugins", "CustomPlugins" };
    
    // プラグイン管理
    private Dictionary<string, IGamePlugin> loadedPlugins = new Dictionary<string, IGamePlugin>();
    private Dictionary<string, PluginMetadata> pluginMetadata = new Dictionary<string, PluginMetadata>();
    
    // プラグインイベント
    public static event Action<string> OnPluginLoaded;
    public static event Action<string> OnPluginUnloaded;
    public static event Action<string, string> OnPluginError;
    
    // プラグイン機能レジストリ
    private Dictionary<string, Delegate> pluginFunctions = new Dictionary<string, Delegate>();
    private Dictionary<string, object> pluginServices = new Dictionary<string, object>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializePluginSystem();
    }
    
    /// <summary>
    /// プラグインシステムの初期化
    /// </summary>
    private void InitializePluginSystem()
    {
        Debug.Log("PluginSystemManager: 初期化開始");
        
        if (!enablePluginSystem)
        {
            Debug.Log("PluginSystemManager: プラグインシステムが無効化されています");
            return;
        }
        
        // プラグインの自動検出
        if (enableAutoDiscovery)
        {
            DiscoverPlugins();
        }
        
        // 初期プラグインの読み込み
        LoadInitialPlugins();
        
        Debug.Log("PluginSystemManager: 初期化完了");
    }
    
    /// <summary>
    /// プラグインの自動検出
    /// </summary>
    private void DiscoverPlugins()
    {
        Debug.Log("PluginSystemManager: プラグインの自動検出を開始");
        
        // アセンブリ内のプラグインクラスを検索
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (typeof(IGamePlugin).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract)
                    {
                        RegisterPluginType(type);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning($"PluginSystemManager: アセンブリ {assembly.FullName} の検索中にエラーが発生しました - {e.Message}");
            }
        }
        
        Debug.Log($"PluginSystemManager: プラグイン検出完了 - {loadedPlugins.Count}個のプラグインを発見");
    }
    
    /// <summary>
    /// プラグインタイプの登録
    /// </summary>
    private void RegisterPluginType(Type pluginType)
    {
        try
        {
            var plugin = (IGamePlugin)Activator.CreateInstance(pluginType);
            var metadata = new PluginMetadata
            {
                Name = pluginType.Name,
                Version = "1.0.0",
                Author = "Unknown",
                Description = "Auto-discovered plugin",
                Type = pluginType
            };
            
            RegisterPlugin(plugin, metadata);
        }
        catch (Exception e)
        {
            Debug.LogError($"PluginSystemManager: プラグイン {pluginType.Name} の登録に失敗しました - {e.Message}");
        }
    }
    
    /// <summary>
    /// 初期プラグインの読み込み
    /// </summary>
    private void LoadInitialPlugins()
    {
        // 基本的なプラグインを読み込み
        LoadPlugin("PerformancePlugin");
        LoadPlugin("AnalyticsPlugin");
        LoadPlugin("DebugPlugin");
    }
    
    /// <summary>
    /// プラグインの登録
    /// </summary>
    public void RegisterPlugin(IGamePlugin plugin, PluginMetadata metadata)
    {
        if (plugin == null)
        {
            Debug.LogError("PluginSystemManager: プラグインがnullです");
            return;
        }
        
        string pluginId = metadata.Name;
        
        if (loadedPlugins.ContainsKey(pluginId))
        {
            Debug.LogWarning($"PluginSystemManager: プラグイン {pluginId} は既に登録されています");
            return;
        }
        
        try
        {
            // プラグインの初期化
            plugin.Initialize();
            
            // プラグインの登録
            loadedPlugins[pluginId] = plugin;
            pluginMetadata[pluginId] = metadata;
            
            // プラグインの機能を登録
            RegisterPluginFunctions(plugin, pluginId);
            
            OnPluginLoaded?.Invoke(pluginId);
            Debug.Log($"PluginSystemManager: プラグイン {pluginId} を登録しました");
        }
        catch (Exception e)
        {
            Debug.LogError($"PluginSystemManager: プラグイン {pluginId} の登録に失敗しました - {e.Message}");
            OnPluginError?.Invoke(pluginId, e.Message);
        }
    }
    
    /// <summary>
    /// プラグイン機能の登録
    /// </summary>
    private void RegisterPluginFunctions(IGamePlugin plugin, string pluginId)
    {
        var type = plugin.GetType();
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        
        foreach (var method in methods)
        {
            if (method.GetCustomAttribute<PluginFunctionAttribute>() != null)
            {
                string functionName = $"{pluginId}.{method.Name}";
                pluginFunctions[functionName] = Delegate.CreateDelegate(method.DeclaringType, plugin, method);
                
                Debug.Log($"PluginSystemManager: プラグイン機能 {functionName} を登録しました");
            }
        }
    }
    
    /// <summary>
    /// プラグインの読み込み
    /// </summary>
    public bool LoadPlugin(string pluginName)
    {
        if (loadedPlugins.ContainsKey(pluginName))
        {
            Debug.LogWarning($"PluginSystemManager: プラグイン {pluginName} は既に読み込まれています");
            return true;
        }
        
        // プラグインの動的読み込み（簡易版）
        Debug.Log($"PluginSystemManager: プラグイン {pluginName} の読み込みを試行");
        
        // 実際の実装では、プラグインの動的読み込みを行う
        // ここでは簡易的な実装として、基本的なプラグインを作成
        
        switch (pluginName)
        {
            case "PerformancePlugin":
                RegisterPlugin(new PerformancePlugin(), new PluginMetadata
                {
                    Name = "PerformancePlugin",
                    Version = "1.0.0",
                    Author = "System",
                    Description = "Performance monitoring plugin"
                });
                return true;
                
            case "AnalyticsPlugin":
                RegisterPlugin(new AnalyticsPlugin(), new PluginMetadata
                {
                    Name = "AnalyticsPlugin",
                    Version = "1.0.0",
                    Author = "System",
                    Description = "Analytics tracking plugin"
                });
                return true;
                
            case "DebugPlugin":
                RegisterPlugin(new DebugPlugin(), new PluginMetadata
                {
                    Name = "DebugPlugin",
                    Version = "1.0.0",
                    Author = "System",
                    Description = "Debug utilities plugin"
                });
                return true;
                
            default:
                Debug.LogWarning($"PluginSystemManager: プラグイン {pluginName} が見つかりません");
                return false;
        }
    }
    
    /// <summary>
    /// プラグインのアンロード
    /// </summary>
    public bool UnloadPlugin(string pluginName)
    {
        if (!loadedPlugins.ContainsKey(pluginName))
        {
            Debug.LogWarning($"PluginSystemManager: プラグイン {pluginName} は読み込まれていません");
            return false;
        }
        
        try
        {
            var plugin = loadedPlugins[pluginName];
            plugin.Shutdown();
            
            loadedPlugins.Remove(pluginName);
            pluginMetadata.Remove(pluginName);
            
            // プラグイン機能の削除
            var keysToRemove = new List<string>();
            foreach (var key in pluginFunctions.Keys)
            {
                if (key.StartsWith(pluginName + "."))
                {
                    keysToRemove.Add(key);
                }
            }
            
            foreach (var key in keysToRemove)
            {
                pluginFunctions.Remove(key);
            }
            
            OnPluginUnloaded?.Invoke(pluginName);
            Debug.Log($"PluginSystemManager: プラグイン {pluginName} をアンロードしました");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"PluginSystemManager: プラグイン {pluginName} のアンロードに失敗しました - {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// プラグイン機能の実行
    /// </summary>
    public T ExecutePluginFunction<T>(string functionName, params object[] parameters)
    {
        if (!pluginFunctions.ContainsKey(functionName))
        {
            Debug.LogWarning($"PluginSystemManager: プラグイン機能 {functionName} が見つかりません");
            return default(T);
        }
        
        try
        {
            var function = pluginFunctions[functionName];
            var result = function.DynamicInvoke(parameters);
            return (T)result;
        }
        catch (Exception e)
        {
            Debug.LogError($"PluginSystemManager: プラグイン機能 {functionName} の実行に失敗しました - {e.Message}");
            return default(T);
        }
    }
    
    /// <summary>
    /// プラグインサービスの登録
    /// </summary>
    public void RegisterService(string serviceName, object service)
    {
        pluginServices[serviceName] = service;
        Debug.Log($"PluginSystemManager: サービス {serviceName} を登録しました");
    }
    
    /// <summary>
    /// プラグインサービスの取得
    /// </summary>
    public T GetService<T>(string serviceName)
    {
        if (pluginServices.ContainsKey(serviceName))
        {
            return (T)pluginServices[serviceName];
        }
        
        Debug.LogWarning($"PluginSystemManager: サービス {serviceName} が見つかりません");
        return default(T);
    }
    
    /// <summary>
    /// プラグイン情報の取得
    /// </summary>
    public PluginMetadata GetPluginMetadata(string pluginName)
    {
        if (pluginMetadata.ContainsKey(pluginName))
        {
            return pluginMetadata[pluginName];
        }
        
        return null;
    }
    
    /// <summary>
    /// 読み込まれたプラグインの一覧を取得
    /// </summary>
    public List<string> GetLoadedPlugins()
    {
        return new List<string>(loadedPlugins.Keys);
    }
    
    /// <summary>
    /// プラグインシステム情報の取得
    /// </summary>
    public string GetPluginSystemInfo()
    {
        return $"Plugin System - Loaded: {loadedPlugins.Count}, Functions: {pluginFunctions.Count}, Services: {pluginServices.Count}";
    }
    
    private void OnDestroy()
    {
        // すべてのプラグインをアンロード
        foreach (var pluginName in new List<string>(loadedPlugins.Keys))
        {
            UnloadPlugin(pluginName);
        }
        
        Debug.Log("PluginSystemManager: 破棄されました");
    }
}

/// <summary>
/// ゲームプラグインインターフェース
/// </summary>
public interface IGamePlugin
{
    void Initialize();
    void Shutdown();
    void Update();
}

/// <summary>
/// プラグイン機能属性
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class PluginFunctionAttribute : Attribute
{
    public string Description { get; set; }
    
    public PluginFunctionAttribute(string description = "")
    {
        Description = description;
    }
}

/// <summary>
/// プラグインメタデータ
/// </summary>
[System.Serializable]
public class PluginMetadata
{
    public string Name;
    public string Version;
    public string Author;
    public string Description;
    public Type Type;
}

/// <summary>
/// パフォーマンスプラグイン（サンプル）
/// </summary>
public class PerformancePlugin : IGamePlugin
{
    public void Initialize()
    {
        Debug.Log("PerformancePlugin: 初期化");
    }
    
    public void Shutdown()
    {
        Debug.Log("PerformancePlugin: シャットダウン");
    }
    
    public void Update()
    {
        // パフォーマンス監視の更新
    }
    
    [PluginFunction("Get performance metrics")]
    public string GetPerformanceMetrics()
    {
        return $"FPS: {1.0f / Time.unscaledDeltaTime:F1}, Memory: {System.GC.GetTotalMemory(false) / (1024f * 1024f):F1}MB";
    }
}

/// <summary>
/// アナリティクスプラグイン（サンプル）
/// </summary>
public class AnalyticsPlugin : IGamePlugin
{
    public void Initialize()
    {
        Debug.Log("AnalyticsPlugin: 初期化");
    }
    
    public void Shutdown()
    {
        Debug.Log("AnalyticsPlugin: シャットダウン");
    }
    
    public void Update()
    {
        // アナリティクスの更新
    }
    
    [PluginFunction("Track event")]
    public void TrackEvent(string eventName, Dictionary<string, object> parameters)
    {
        Debug.Log($"AnalyticsPlugin: イベント追跡 - {eventName}");
    }
}

/// <summary>
/// デバッグプラグイン（サンプル）
/// </summary>
public class DebugPlugin : IGamePlugin
{
    public void Initialize()
    {
        Debug.Log("DebugPlugin: 初期化");
    }
    
    public void Shutdown()
    {
        Debug.Log("DebugPlugin: シャットダウン");
    }
    
    public void Update()
    {
        // デバッグ機能の更新
    }
    
    [PluginFunction("Log debug message")]
    public void LogDebug(string message)
    {
        Debug.Log($"DebugPlugin: {message}");
    }
} 