using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

/// <summary>
/// SOアセットの自動設定専用マネージャー
/// 責務：SOアセットの自動検出と設定
/// </summary>
[DefaultExecutionOrder(-99)]
public class AutoSetupManager : MonoBehaviour
{
    public static AutoSetupManager Instance { get; private set; }
    
    [Header("Auto Setup Settings")]
    public bool enableAutoSetup = true;
    public bool enableEventChannelSetup = true;
    public bool enableDataObjectSetup = true;
    
    [Header("Auto Setup Results")]
    public bool eventChannelsSetup = false;
    public bool dataObjectsSetup = false;
    public bool managersSetup = false;
    
    // 設定済みアセットの追跡
    private Dictionary<string, bool> setupStatus = new Dictionary<string, bool>();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        if (enableAutoSetup)
        {
            StartAutoSetup();
        }
    }
    
    /// <summary>
    /// 自動設定の開始
    /// </summary>
    private void StartAutoSetup()
    {
        Debug.Log("AutoSetupManager: 自動設定を開始します");
        
        // イベントチャンネルの設定
        if (enableEventChannelSetup)
        {
            SetupEventChannels();
        }
        
        // データオブジェクトの設定
        if (enableDataObjectSetup)
        {
            SetupDataObjects();
        }
        
        // マネージャーの設定
        SetupManagers();
        
        Debug.Log("AutoSetupManager: 自動設定が完了しました");
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
        
        eventChannelsSetup = true;
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
        
        dataObjectsSetup = true;
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
        var playerDataManager = FindObjectOfType<PlayerDataSystem.PlayerDataManager>();
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
        
        managersSetup = true;
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
            setupStatus["SaveEventChannel"] = true;
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
            setupStatus["DeckEventChannel"] = true;
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
            setupStatus["UIEventChannel"] = true;
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
                setupStatus["SaveDataSO"] = true;
                Debug.Log("AutoSetupManager: SaveDataSOをSaveManagerに設定しました");
            }
        }
    }
    
    /// <summary>
    /// PlayerDataSOの設定
    /// </summary>
    private void SetupPlayerDataSO(PlayerDataSystem.PlayerDataSO playerData)
    {
        var playerDataManager = FindObjectOfType<PlayerDataSystem.PlayerDataManager>();
        if (playerDataManager != null)
        {
            // PlayerDataManagerのplayerDataフィールドを設定
            var field = typeof(PlayerDataSystem.PlayerDataManager).GetField("playerData", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field != null)
            {
                field.SetValue(playerDataManager, playerData);
                setupStatus["PlayerDataSO"] = true;
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
                setupStatus["DeckDataSO"] = true;
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
            if (gameManager.newGameManager == null)
                gameManager.newGameManager = FindObjectOfType<GameManagerNew>();
            
            if (gameManager.gameStateManager == null)
                gameManager.gameStateManager = FindObjectOfType<GameStateManager>();
            
            if (gameManager.playerDataManager == null)
                gameManager.playerDataManager = FindObjectOfType<PlayerDataSystem.PlayerDataManager>();
            
            if (gameManager.floorManager == null)
                gameManager.floorManager = FindObjectOfType<FloorManager>();
            
            if (gameManager.systemIntegrationManager == null)
                gameManager.systemIntegrationManager = FindObjectOfType<SystemIntegrationManager>();
            
            setupStatus["GameManager"] = true;
            Debug.Log("AutoSetupManager: GameManagerの設定が完了しました");
        }
    }
    
    /// <summary>
    /// SaveManagerの設定
    /// </summary>
    private void SetupSaveManager(SaveSystem.SaveManager saveManager)
    {
        // SaveManagerの設定（必要に応じて）
        setupStatus["SaveManager"] = true;
        Debug.Log("AutoSetupManager: SaveManagerの設定が完了しました");
    }
    
    /// <summary>
    /// PlayerDataManagerの設定
    /// </summary>
    private void SetupPlayerDataManager(PlayerDataSystem.PlayerDataManager playerDataManager)
    {
        // PlayerDataManagerの設定（必要に応じて）
        setupStatus["PlayerDataManager"] = true;
        Debug.Log("AutoSetupManager: PlayerDataManagerの設定が完了しました");
    }
    
    /// <summary>
    /// DeckManagerの設定
    /// </summary>
    private void SetupDeckManager(DeckSystem.DeckManager deckManager)
    {
        // DeckManagerの設定（必要に応じて）
        setupStatus["DeckManager"] = true;
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
        
        status.Add($"Event Channels: {(eventChannelsSetup ? "✅" : "❌")}");
        status.Add($"Data Objects: {(dataObjectsSetup ? "✅" : "❌")}");
        status.Add($"Managers: {(managersSetup ? "✅" : "❌")}");
        
        foreach (var kvp in setupStatus)
        {
            status.Add($"{kvp.Key}: {(kvp.Value ? "✅" : "❌")}");
        }
        
        return string.Join("\n", status);
    }
    
    /// <summary>
    /// 自動設定の再実行
    /// </summary>
    public void ReRunAutoSetup()
    {
        Debug.Log("AutoSetupManager: 自動設定を再実行します");
        setupStatus.Clear();
        eventChannelsSetup = false;
        dataObjectsSetup = false;
        managersSetup = false;
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