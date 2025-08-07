using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

/// <summary>
/// 自動セットアップマネージャー
/// 責務：シーンの自動セットアップのみ
/// </summary>
[DefaultExecutionOrder(-50)]
public class AutoSetupManager : MonoBehaviour
{
    public static AutoSetupManager Instance { get; private set; }
    
    [Header("Auto Setup Settings")]
    public bool enableAutoSetup = true;
    public bool enableDebugLogs = true;
    
    // Managerキャッシュシステム
    private static Dictionary<System.Type, MonoBehaviour> cachedManagers = new Dictionary<System.Type, MonoBehaviour>();
    
    // イベント定義
    public static event System.Action OnAutoSetupCompleted;
    public static event System.Action<string> OnSetupStepCompleted;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeAutoSetupManager();
    }
    
    /// <summary>
    /// AutoSetupManagerの初期化
    /// </summary>
    private void InitializeAutoSetupManager()
    {
        if (enableAutoSetup)
        {
            StartAutoSetup();
        }
    }
    
    /// <summary>
    /// キャッシュされたManagerを取得
    /// </summary>
    public static T GetCachedManager<T>() where T : MonoBehaviour
    {
        if (cachedManagers.TryGetValue(typeof(T), out var manager))
        {
            return manager as T;
        }
        
        var found = FindObjectOfType<T>();
        if (found != null)
        {
            cachedManagers[typeof(T)] = found;
        }
        return found;
    }
    
    /// <summary>
    /// キャッシュをクリア
    /// </summary>
    public static void ClearCache()
    {
        cachedManagers.Clear();
    }
    
    /// <summary>
    /// 自動設定の開始
    /// </summary>
    private void StartAutoSetup()
    {
        Debug.Log("AutoSetupManager: 自動設定を開始します");
        
        // イベントチャンネルの設定
        SetupEventChannels();
        
        // データオブジェクトの設定
        SetupDataObjects();
        
        // マネージャーの設定
        SetupManagers();
        
        Debug.Log("AutoSetupManager: 自動設定が完了しました");
        OnAutoSetupCompleted?.Invoke();
    }
    
    /// <summary>
    /// イベントチャンネルの設定
    /// </summary>
    public void SetupEventChannels()
    {
        Debug.Log("AutoSetupManager: イベントチャンネルの設定を開始");
        
        // SaveEventChannelの設定
        var saveEventChannel = FindAsset<SaveSystem.SaveEventChannel>("SaveEventChannel");
        if (saveEventChannel != null)
        {
            SetupSaveEventChannel(saveEventChannel);
        }
        
        // DeckEventChannelの設定
        var deckEventChannel = FindAsset<DeckSystem.DeckEventChannel>("DeckEventChannel");
        if (deckEventChannel != null)
        {
            SetupDeckEventChannel(deckEventChannel);
        }
        
        // UIEventChannelの設定
        var uiEventChannel = FindAsset<UISystem.UIEventChannel>("UIEventChannel");
        if (uiEventChannel != null)
        {
            SetupUIEventChannel(uiEventChannel);
        }
        
        OnSetupStepCompleted?.Invoke("Event Channels");
        Debug.Log("AutoSetupManager: イベントチャンネルの設定が完了しました");
    }
    
    /// <summary>
    /// データオブジェクトの設定
    /// </summary>
    public void SetupDataObjects()
    {
        Debug.Log("AutoSetupManager: データオブジェクトの設定を開始");
        
        // SaveDataSOの設定
        var saveDataSO = FindAsset<SaveSystem.SaveDataSO>("SaveDataSO");
        if (saveDataSO != null)
        {
            SetupSaveDataSO(saveDataSO);
        }
        
        // PlayerDataSOの設定
        var playerDataSO = FindAsset<PlayerDataSystem.PlayerDataSO>("PlayerDataSO");
        if (playerDataSO != null)
        {
            SetupPlayerDataSO(playerDataSO);
        }
        
        // DeckDataSOの設定
        var deckDataSO = FindAsset<DeckSystem.DeckDataSO>("DeckDataSO");
        if (deckDataSO != null)
        {
            SetupDeckDataSO(deckDataSO);
        }
        
        OnSetupStepCompleted?.Invoke("Data Objects");
        Debug.Log("AutoSetupManager: データオブジェクトの設定が完了しました");
    }
    
    /// <summary>
    /// マネージャーの設定
    /// </summary>
    private void SetupManagers()
    {
        Debug.Log("AutoSetupManager: マネージャーの設定を開始");
        
        // GameManagerの設定
        if (GameManager.Instance != null)
        {
            SetupGameManager();
        }
        
        // SaveManagerの設定
        var saveManager = FindObjectOfType<SaveSystem.SaveManager>();
        if (saveManager != null)
        {
            SetupSaveManager(saveManager);
        }
        
        // PlayerDataManagerの設定
        var playerDataManager = FindObjectOfType<PlayerDataManager>();
        if (playerDataManager != null)
        {
            SetupPlayerDataManager(playerDataManager);
        }
        
        // DeckManagerの設定
        var deckManager = FindObjectOfType<DeckSystem.DeckManager>();
        if (deckManager != null)
        {
            SetupDeckManager(deckManager);
        }
        
        OnSetupStepCompleted?.Invoke("Managers");
        Debug.Log("AutoSetupManager: マネージャーの設定が完了しました");
    }
    
    /// <summary>
    /// SaveEventChannelの設定
    /// </summary>
    private void SetupSaveEventChannel(SaveSystem.SaveEventChannel eventChannel)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.saveEventChannel = eventChannel;
            Debug.Log("AutoSetupManager: SaveEventChannelをGameManagerに設定しました");
        }
        
        var saveManager = FindObjectOfType<SaveSystem.SaveManager>();
        if (saveManager != null)
        {
            // SaveManagerのsaveEventChannelフィールドを設定
            var field = typeof(SaveSystem.SaveManager).GetField("saveEventChannel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(saveManager, eventChannel);
                Debug.Log("AutoSetupManager: SaveEventChannelをSaveManagerに設定しました");
            }
        }
    }
    
    /// <summary>
    /// DeckEventChannelの設定
    /// </summary>
    private void SetupDeckEventChannel(DeckSystem.DeckEventChannel eventChannel)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.deckEventChannel = eventChannel;
            Debug.Log("AutoSetupManager: DeckEventChannelをGameManagerに設定しました");
        }
        
        var deckManager = FindObjectOfType<DeckSystem.DeckManager>();
        if (deckManager != null)
        {
            // DeckManagerのdeckEventChannelフィールドを設定
            var field = typeof(DeckSystem.DeckManager).GetField("deckEventChannel", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(deckManager, eventChannel);
                Debug.Log("AutoSetupManager: DeckEventChannelをDeckManagerに設定しました");
            }
        }
    }
    
    /// <summary>
    /// UIEventChannelの設定
    /// </summary>
    private void SetupUIEventChannel(UISystem.UIEventChannel eventChannel)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.uiEventChannel = eventChannel;
            Debug.Log("AutoSetupManager: UIEventChannelをGameManagerに設定しました");
        }
    }
    
    /// <summary>
    /// SaveDataSOの設定
    /// </summary>
    private void SetupSaveDataSO(SaveSystem.SaveDataSO saveData)
    {
        var saveManager = FindObjectOfType<SaveSystem.SaveManager>();
        if (saveManager != null)
        {
            // SaveManagerのsaveDataフィールドを設定
            var field = typeof(SaveSystem.SaveManager).GetField("saveData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(saveManager, saveData);
                Debug.Log("AutoSetupManager: SaveDataSOをSaveManagerに設定しました");
            }
        }
    }
    
    /// <summary>
    /// PlayerDataSOの設定
    /// </summary>
    private void SetupPlayerDataSO(PlayerDataSystem.PlayerDataSO playerData)
    {
        var playerDataManager = FindObjectOfType<PlayerDataManager>();
        if (playerDataManager != null)
        {
            // PlayerDataManagerのplayerDataフィールドを設定
            var field = typeof(PlayerDataManager).GetField("playerData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(playerDataManager, playerData);
                Debug.Log("AutoSetupManager: PlayerDataSOをPlayerDataManagerに設定しました");
            }
        }
    }
    
    /// <summary>
    /// DeckDataSOの設定
    /// </summary>
    private void SetupDeckDataSO(DeckSystem.DeckDataSO deckData)
    {
        var deckManager = FindObjectOfType<DeckSystem.DeckManager>();
        if (deckManager != null)
        {
            // DeckManagerのdeckDataフィールドを設定
            var field = typeof(DeckSystem.DeckManager).GetField("deckData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(deckManager, deckData);
                Debug.Log("AutoSetupManager: DeckDataSOをDeckManagerに設定しました");
            }
        }
    }
    
    /// <summary>
    /// GameManagerの設定
    /// </summary>
    private void SetupGameManager()
    {
        // GameManagerの新しいシステム参照を設定
        if (GameManager.Instance != null)
        {
            var gameManager = GameManager.Instance;
            
            // 新しいマネージャーの参照を設定
            if (gameManager.gameStateManager == null)
                gameManager.gameStateManager = FindObjectOfType<GameStateManager>();
            
            if (gameManager.playerDataManager == null)
                gameManager.playerDataManager = FindObjectOfType<PlayerDataManager>();
            
            if (gameManager.floorManager == null)
                gameManager.floorManager = FindObjectOfType<FloorManager>();
            
            if (gameManager.systemIntegrationManager == null)
                gameManager.systemIntegrationManager = FindObjectOfType<SystemIntegrationManager>();
            
            Debug.Log("AutoSetupManager: GameManagerの設定が完了しました");
        }
    }
    
    /// <summary>
    /// SaveManagerの設定
    /// </summary>
    private void SetupSaveManager(SaveSystem.SaveManager saveManager)
    {
        // SaveManagerの設定（必要に応じて）
        Debug.Log("AutoSetupManager: SaveManagerの設定が完了しました");
    }
    
    /// <summary>
    /// PlayerDataManagerの設定
    /// </summary>
    private void SetupPlayerDataManager(PlayerDataManager playerDataManager)
    {
        // PlayerDataManagerの設定（必要に応じて）
        Debug.Log("AutoSetupManager: PlayerDataManagerの設定が完了しました");
    }
    
    /// <summary>
    /// DeckManagerの設定
    /// </summary>
    private void SetupDeckManager(DeckSystem.DeckManager deckManager)
    {
        // DeckManagerの設定（必要に応じて）
        Debug.Log("AutoSetupManager: DeckManagerの設定が完了しました");
    }
    
    /// <summary>
    /// アセットの検索
    /// </summary>
    private T FindAsset<T>(string assetName) where T : Object
    {
        // Resourcesフォルダから検索
        var assets = Resources.FindObjectsOfTypeAll<T>();
        var asset = assets.FirstOrDefault(a => a.name == assetName);
        
        if (asset != null)
        {
            Debug.Log($"AutoSetupManager: {assetName}をResourcesから検索しました");
            return asset;
        }
        
        // 全アセットから検索
        var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var loadedAsset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (loadedAsset != null && loadedAsset.name == assetName)
            {
                Debug.Log($"AutoSetupManager: {assetName}をAssetDatabaseから検索しました");
                return loadedAsset;
            }
        }
        
        Debug.LogWarning($"AutoSetupManager: {assetName}が見つかりませんでした");
        return null;
    }
    
    /// <summary>
    /// 設定状況の取得
    /// </summary>
    public string GetSetupStatus()
    {
        var status = new List<string>();
        
        status.Add($"Event Channels: {(cachedManagers.ContainsKey(typeof(SaveSystem.SaveEventChannel)) ? "✅" : "❌")}");
        status.Add($"Data Objects: {(cachedManagers.ContainsKey(typeof(SaveSystem.SaveDataSO)) ? "✅" : "❌")}");
        status.Add($"Managers: {(cachedManagers.ContainsKey(typeof(GameManager)) ? "✅" : "❌")}");
        
        foreach (var kvp in cachedManagers)
        {
            status.Add($"{kvp.Key.Name}: {(kvp.Value != null ? "✅" : "❌")}");
        }
        
        return string.Join("\n", status);
    }
    
    /// <summary>
    /// 自動設定の再実行
    /// </summary>
    public void ReRunAutoSetup()
    {
        Debug.Log("AutoSetupManager: 自動設定を再実行します");
        cachedManagers.Clear(); // キャッシュをクリア
        StartAutoSetup();
    }
    
    private void OnDestroy()
    {
        Debug.Log("AutoSetupManager: 破棄されました");
    }
}

#if UNITY_EDITOR
/// <summary>
/// エディタ用の自動設定ツール
/// </summary>
public class AutoSetupEditor
{
    [MenuItem("Tools/Auto Setup/Setup All Assets")]
    public static void SetupAllAssets()
    {
        var autoSetupManager = UnityEngine.Object.FindObjectOfType<AutoSetupManager>();
        if (autoSetupManager == null)
        {
            var go = new GameObject("AutoSetupManager");
            autoSetupManager = go.AddComponent<AutoSetupManager>();
        }
        
        autoSetupManager.ReRunAutoSetup();
        Debug.Log("AutoSetupEditor: 全アセットの設定が完了しました");
    }
    
    [MenuItem("Tools/Auto Setup/Setup Event Channels")]
    public static void SetupEventChannels()
    {
        var autoSetupManager = UnityEngine.Object.FindObjectOfType<AutoSetupManager>();
        if (autoSetupManager == null)
        {
            var go = new GameObject("AutoSetupManager");
            autoSetupManager = go.AddComponent<AutoSetupManager>();
        }
        
        autoSetupManager.SetupEventChannels();
        Debug.Log("AutoSetupEditor: イベントチャンネルの設定が完了しました");
    }
    
    [MenuItem("Tools/Auto Setup/Setup Data Objects")]
    public static void SetupDataObjects()
    {
        var autoSetupManager = UnityEngine.Object.FindObjectOfType<AutoSetupManager>();
        if (autoSetupManager == null)
        {
            var go = new GameObject("AutoSetupManager");
            autoSetupManager = go.AddComponent<AutoSetupManager>();
        }
        
        autoSetupManager.SetupDataObjects();
        Debug.Log("AutoSetupEditor: データオブジェクトの設定が完了しました");
    }
    
    [MenuItem("Tools/Auto Setup/Show Setup Status")]
    public static void ShowSetupStatus()
    {
        var autoSetupManager = UnityEngine.Object.FindObjectOfType<AutoSetupManager>();
        if (autoSetupManager != null)
        {
            Debug.Log("AutoSetupEditor: 設定状況を表示します");
            Debug.Log(autoSetupManager.GetSetupStatus());
        }
        else
        {
            Debug.LogWarning("AutoSetupEditor: AutoSetupManagerが見つかりません");
        }
    }
}
#endif 