using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using PlayerDataSystem;

/// <summary>
/// ゲーム全体の管理を行うクラス（旧版 - 段階的移行中）
/// 新しいシステムとの互換性レイヤーを追加
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Legacy Game State")]
    public int score = 0;
    public int playerLevel = 1;
    public int playerExp = 0;
    public int playerExpToNext = 10;
    public int playerMaxHP = 20;
    public int playerCurrentHP = 20;
    public int playerMaxLevel = 10;
    public int currentFloor = 1;
    public int maxFloor = 10;
    public bool gameOver = false;
    public bool gameClear = false;
    
    [Header("New System Integration")]
    public GameManagerNew newGameManager;
    public GameStateManager gameStateManager;
    public PlayerDataSystem.PlayerDataManager playerDataManager;
    public FloorManager floorManager;
    public SystemIntegrationManager systemIntegrationManager;
    
    [Header("Legacy Managers")]
    public SaveSystem.SaveManager saveManager;
    public DeckSystem.DeckManager deckManager;
    public UISystem.UIManager uiManager;
    public SoundManager soundManager;
    
    [Header("Event Channels")]
    public SaveSystem.SaveEventChannel saveEventChannel;
    public DeckSystem.DeckEventChannel deckEventChannel;
    public UISystem.UIEventChannel uiEventChannel;
    
    [Header("Auto Setup")]
    public AutoSetupManager autoSetupManager;
    
    // レガシーイベント（後方互換性のため）
    public static event System.Action<int> OnScoreChanged;
    public static event System.Action<int> OnPlayerLevelUp;
    public static event System.Action<int> OnPlayerExpGained;
    public static event System.Action<int, int> OnPlayerHPChanged;
    public static event System.Action<int> OnFloorChanged;
    public static event System.Action OnGameOver;
    public static event System.Action OnGameClear;
    
    // 移行フラグ
    [Header("Migration Settings")]
    public bool useNewSystems = true;  // 新しいシステムを有効化
    public bool enableLegacyMode = true;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeGameManager();
    }
    
    /// <summary>
    /// ゲームマネージャーの初期化
    /// </summary>
    private void InitializeGameManager()
    {
        Debug.Log("GameManager: 初期化開始");
        
        // 新しいシステムの参照を取得
        if (newGameManager == null)
            newGameManager = GameManagerNew.Instance;
        if (gameStateManager == null)
            gameStateManager = GameStateManager.Instance;
        if (playerDataManager == null)
        {
            playerDataManager = PlayerDataSystem.PlayerDataManager.Instance;
            if (playerDataManager == null)
            {
                Debug.LogWarning("GameManager: PlayerDataManager.Instanceがnullです。PlayerDataManagerを探して取得します。");
                playerDataManager = FindObjectOfType<PlayerDataSystem.PlayerDataManager>();
                if (playerDataManager == null)
                {
                    Debug.LogError("GameManager: PlayerDataManagerが見つかりません。新しいPlayerDataManagerを作成します。");
                    GameObject playerDataManagerObj = new GameObject("PlayerDataManager");
                    playerDataManager = playerDataManagerObj.AddComponent<PlayerDataSystem.PlayerDataManager>();
                }
            }
        }
        if (floorManager == null)
        {
            floorManager = FloorManager.Instance;
            if (floorManager == null)
            {
                Debug.LogWarning("GameManager: FloorManager.Instanceがnullです。FloorManagerを探して取得します。");
                floorManager = FindObjectOfType<FloorManager>();
                if (floorManager == null)
                {
                    Debug.LogError("GameManager: FloorManagerが見つかりません。新しいFloorManagerを作成します。");
                    GameObject floorManagerObj = new GameObject("FloorManager");
                    floorManager = floorManagerObj.AddComponent<FloorManager>();
                }
            }
        }
        if (systemIntegrationManager == null)
            systemIntegrationManager = SystemIntegrationManager.Instance;
        
        // AutoSetupManagerの設定
        if (autoSetupManager == null)
            autoSetupManager = FindObjectOfType<AutoSetupManager>();
            
        // レガシーモードが有効な場合のみ旧システムを初期化
        if (enableLegacyMode)
        {
            InitializeLegacySystems();
        }
        
        // 新しいシステムとの統合を設定
        if (useNewSystems)
        {
            SetupNewSystemIntegration();
        }
        
        // 初期化完了後のデータ同期
        StartCoroutine(DelayedInitialization());
        
        Debug.Log("GameManager: 初期化完了（移行モード）");
    }
    
    /// <summary>
    /// 遅延初期化処理
    /// </summary>
    private System.Collections.IEnumerator DelayedInitialization()
    {
        // 1フレーム待機して他のシステムの初期化を待つ
        yield return null;
        
        // データ同期
        if (useNewSystems)
        {
            SyncLegacyDataToNewSystems();
        }
        
        Debug.Log("GameManager: 遅延初期化完了");
    }
    
    /// <summary>
    /// レガシーシステムの初期化
    /// </summary>
    private void InitializeLegacySystems()
    {
        // 初期値の設定
        score = 0;
        playerLevel = 1;
        playerExp = 0;
        playerExpToNext = 10;
        playerMaxHP = 20;
        playerCurrentHP = 20;
        playerMaxLevel = 10;
        currentFloor = 1; // 必ず1階からスタート
        maxFloor = 10;
        gameOver = false;
        gameClear = false;
        
        Debug.Log("GameManager: レガシーシステムを初期化しました - 階層: " + currentFloor);
    }
    
    /// <summary>
    /// 新しいシステムとの統合設定
    /// </summary>
    private void SetupNewSystemIntegration()
    {
        if (newGameManager != null)
        {
            // 新しいシステムのイベントを購読
            GameManagerNew.OnGameInitialized += OnNewGameInitialized;
            GameManagerNew.OnGameStateChanged += OnNewGameStateChanged;
        }
        
        Debug.Log("GameManager: 新しいシステムとの統合を設定しました");
    }
    
    // 新しいシステムイベントハンドラー
    
    /// <summary>
    /// 新しいゲーム初期化完了時の処理
    /// </summary>
    private void OnNewGameInitialized()
    {
        Debug.Log("GameManager: 新しいゲームシステムの初期化を検知しました");
        
        // レガシーデータを新しいシステムに同期
        SyncLegacyDataToNewSystems();
    }
    
    /// <summary>
    /// 新しいゲーム状態変更時の処理
    /// </summary>
    private void OnNewGameStateChanged()
    {
        Debug.Log("GameManager: 新しいゲーム状態変更を検知しました");
        
        // 新しいシステムからレガシーデータに同期
        SyncNewDataToLegacySystems();
    }
    
    /// <summary>
    /// レガシーデータを新しいシステムに同期
    /// </summary>
    private void SyncLegacyDataToNewSystems()
    {
        if (gameStateManager != null)
        {
            gameStateManager.SetScore(score);
        }
        
        if (playerDataManager != null)
        {
            playerDataManager.SetPlayerLevel(playerLevel);
            playerDataManager.SetPlayerHP(playerCurrentHP, playerMaxHP);
            // playerMaxLevelは新しいシステムでは固定値として扱う
        }
        
        if (floorManager != null)
        {
            floorManager.SetFloor(currentFloor);
        }
        
        Debug.Log("GameManager: レガシーデータを新しいシステムに同期しました");
    }
    
    /// <summary>
    /// 新しいシステムからレガシーデータに同期
    /// </summary>
    private void SyncNewDataToLegacySystems()
    {
        if (gameStateManager != null)
        {
            score = gameStateManager.score;
            gameOver = gameStateManager.gameOver;
            gameClear = gameStateManager.gameClear;
        }
        
        if (playerDataManager != null && playerDataManager.GetPlayerData() != null)
        {
            var playerData = playerDataManager.GetPlayerData();
            playerLevel = playerData.level;
            playerExp = playerData.experience;
            playerExpToNext = playerData.experienceToNext;
            playerCurrentHP = playerData.currentHP;
            playerMaxHP = playerData.maxHP;
            // playerMaxLevelは新しいシステムでは固定値として扱う
        }
        
        if (floorManager != null)
        {
            currentFloor = floorManager.currentFloor;
            maxFloor = floorManager.maxFloor;
        }
        
        Debug.Log("GameManager: 新しいシステムからレガシーデータに同期しました");
    }
    
    // レガシーメソッド（後方互換性のため）
    
    /// <summary>
    /// スコアを加算（レガシー）
    /// </summary>
    /// <param name="amount">加算するスコア</param>
    public void AddScore(int amount)
    {
        if (useNewSystems && gameStateManager != null)
        {
            gameStateManager.AddScore(amount);
        }
        else
        {
            score += amount;
            OnScoreChanged?.Invoke(score);
        }
        
        Debug.Log($"GameManager: スコアを加算しました - +{amount} (合計: {score})");
    }
    
    /// <summary>
    /// プレイヤーに経験値を加算（レガシー）
    /// </summary>
    /// <param name="amount">加算する経験値</param>
    public void AddPlayerExp(int amount)
    {
        Debug.Log($"GameManager: AddPlayerExp({amount})呼び出し - useNewSystems: {useNewSystems}, playerDataManager: {(playerDataManager != null ? "NotNull" : "Null")}, 現在のplayerExp: {playerExp}");
        if (useNewSystems && playerDataManager != null)
        {
            playerDataManager.AddPlayerExp(amount);
            Debug.Log($"GameManager: 新しいシステムで経験値を加算しました - +{amount}");
        }
        else
        {
            playerExp += amount;
            Debug.Log($"GameManager: レガシーシステムでプレイヤーに経験値を加算しました - +{amount} (合計: {playerExp})");
            CheckLevelUp();
            Debug.Log($"GameManager: CheckLevelUp()後 - playerExp: {playerExp}");
            OnPlayerExpGained?.Invoke(playerExp);
        }
    }
    
    /// <summary>
    /// レベルアップチェック（レガシー）
    /// </summary>
    private void CheckLevelUp()
    {
        while (playerExp >= playerExpToNext && playerLevel < 10)
        {
            playerExp -= playerExpToNext;
            playerLevel++;
            
            // レベルアップ時の成長
            int oldMaxHP = playerMaxHP;
            playerMaxHP += 5;
            playerCurrentHP = playerMaxHP; // レベルアップ時はHP全回復
            playerExpToNext = Mathf.RoundToInt(playerExpToNext * 1.5f);
            
            OnPlayerLevelUp?.Invoke(playerLevel);
            OnPlayerHPChanged?.Invoke(playerCurrentHP, playerMaxHP);
            
            Debug.Log($"GameManager: プレイヤーがレベルアップしました - レベル{playerLevel}, HP {oldMaxHP} → {playerMaxHP}");
        }
    }
    
    /// <summary>
    /// プレイヤーのHPを設定（レガシー）
    /// </summary>
    /// <param name="currentHP">現在のHP</param>
    /// <param name="maxHP">最大HP（-1の場合は変更しない）</param>
    public void SetPlayerHP(int currentHP, int maxHP = -1)
    {
        if (useNewSystems && playerDataManager != null)
        {
            playerDataManager.SetPlayerHP(currentHP, maxHP);
        }
        else
        {
            if (currentHP != playerCurrentHP)
            {
                playerCurrentHP = Mathf.Clamp(currentHP, 0, playerMaxHP);
            }
            
            if (maxHP > 0 && maxHP != playerMaxHP)
            {
                playerMaxHP = maxHP;
                playerCurrentHP = Mathf.Clamp(playerCurrentHP, 0, playerMaxHP);
            }
            
            OnPlayerHPChanged?.Invoke(playerCurrentHP, playerMaxHP);
        }
        
        Debug.Log($"GameManager: プレイヤーHPを設定しました - {playerCurrentHP}/{playerMaxHP}");
    }
    
    /// <summary>
    /// プレイヤーのレベルを設定（レガシー）
    /// </summary>
    /// <param name="level">新しいレベル</param>
    public void SetPlayerLevel(int level)
    {
        if (useNewSystems && playerDataManager != null)
        {
            playerDataManager.SetPlayerLevel(level);
        }
        else
        {
            if (level != playerLevel && level >= 1 && level <= 10)
            {
                int oldLevel = playerLevel;
                playerLevel = level;
                
                OnPlayerLevelUp?.Invoke(playerLevel);
            }
        }
        
        Debug.Log($"GameManager: プレイヤーレベルを設定しました - {playerLevel}");
    }
    
    /// <summary>
    /// 次の階層に進む（レガシー）
    /// </summary>
    public void GoToNextFloor()
    {
        Debug.Log($"GameManager: GoToNextFloor()開始 - useNewSystems: {useNewSystems}, floorManager: {(floorManager != null ? "存在" : "null")}");
        
        if (useNewSystems && floorManager != null)
        {
            Debug.Log("GameManager: 新しいシステムを使用して階層進行");
            floorManager.GoToNextFloor();
        }
        else
        {
            Debug.Log("GameManager: レガシーシステムを使用して階層進行");
            if (currentFloor < maxFloor)
            {
                int oldFloor = currentFloor;
                currentFloor++;
                
                OnFloorChanged?.Invoke(currentFloor);
                
                Debug.Log($"GameManager: 階層を進行しました - {oldFloor} → {currentFloor}");
                
                // 最終階層到達時の処理
                if (currentFloor >= maxFloor)
                {
                    GameClear();
                }
            }
            else
            {
                Debug.LogWarning($"GameManager: 既に最終階層です - currentFloor: {currentFloor}, maxFloor: {maxFloor}");
            }
        }
    }
    
    /// <summary>
    /// ゲームオーバー（レガシー）
    /// </summary>
    public void GameOver()
    {
        if (useNewSystems && gameStateManager != null)
        {
            gameStateManager.Defeat();
        }
        else
        {
            gameOver = true;
            OnGameOver?.Invoke();
        }
        
        Debug.Log("GameManager: ゲームオーバー");
    }
    
    /// <summary>
    /// ゲームクリア（レガシー）
    /// </summary>
    public void GameClear()
    {
        if (useNewSystems && gameStateManager != null)
        {
            gameStateManager.Victory();
        }
        else
        {
            gameClear = true;
            OnGameClear?.Invoke();
        }
        
        Debug.Log("GameManager: ゲームクリア");
    }
    
    /// <summary>
    /// 新しいシステムに移行
    /// </summary>
    public void MigrateToNewSystems()
    {
        useNewSystems = true;
        enableLegacyMode = false;
        
        // データを新しいシステムに同期
        SyncLegacyDataToNewSystems();
        
        Debug.Log("GameManager: 新しいシステムに移行しました");
    }
    
    /// <summary>
    /// レガシーモードに戻す
    /// </summary>
    public void RevertToLegacyMode()
    {
        useNewSystems = false;
        enableLegacyMode = true;
        
        // レガシーシステムを再初期化
        InitializeLegacySystems();
        
        Debug.Log("GameManager: レガシーモードに戻しました");
    }
    
    /// <summary>
    /// ハイブリッドモードを有効化
    /// </summary>
    public void EnableHybridMode()
    {
        useNewSystems = true;
        enableLegacyMode = true;
        
        Debug.Log("GameManager: ハイブリッドモードを有効化しました");
    }
    
    private void OnDestroy()
    {
        if (newGameManager != null)
        {
            GameManagerNew.OnGameInitialized -= OnNewGameInitialized;
            GameManagerNew.OnGameStateChanged -= OnNewGameStateChanged;
        }
    }
    
    /// <summary>
    /// ゲームマネージャーの情報を取得
    /// </summary>
    /// <returns>ゲームマネージャーの情報文字列</returns>
    public string GetGameManagerInfo()
    {
        return $"GameManager - Mode: {(useNewSystems ? "New" : "Legacy")}, " +
               $"LegacyMode: {enableLegacyMode}, " +
               $"Score: {score}, Level: {playerLevel}, Floor: {currentFloor}, " +
               $"GameOver: {gameOver}, GameClear: {gameClear}";
    }
    
    // 後方互換性のためのメソッド
    
    /// <summary>
    /// GameManagerインスタンスを取得または作成（後方互換性）
    /// </summary>
    /// <returns>GameManagerインスタンス</returns>
    public static GameManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameManager existingManager = FindObjectOfType<GameManager>();
            if (existingManager != null)
            {
                Instance = existingManager;
            }
            else
            {
                GameObject gameManagerObj = new GameObject("GameManager");
                Instance = gameManagerObj.AddComponent<GameManager>();
            }
        }
        
        return Instance;
    }
    
    /// <summary>
    /// プレイヤーデッキを取得（後方互換性）
    /// </summary>
    /// <returns>プレイヤーデッキ</returns>
    public PlayerDeck GetPlayerDeck()
    {
        // 新しいシステムが利用可能な場合はそちらを使用
        if (useNewSystems && deckManager != null)
        {
            // 新しいデッキシステムはDeckDataSOを使用するため、
            // レガシーデッキを返す（後方互換性のため）
            Debug.LogWarning("GameManager: 新しいデッキシステムはPlayerDeckと互換性がありません。レガシーデッキを使用します。");
        }
        
        // レガシーデッキを返す（後方互換性のため）
        if (playerDeck == null)
        {
            Debug.LogWarning("GameManager: playerDeckがnullです");
        }
        return playerDeck;
    }
    
    /// <summary>
    /// プレイヤーデッキを設定（後方互換性）
    /// </summary>
    /// <param name="deck">設定するデッキ</param>
    public void SetPlayerDeck(PlayerDeck deck)
    {
        if (deck == null)
        {
            Debug.LogError("GameManager: SetPlayerDeck - deckがnullです");
            return;
        }
        
        // 新しいシステムが利用可能な場合はそちらを使用
        if (useNewSystems && deckManager != null)
        {
            // 新しいデッキシステムはDeckDataSOを使用するため、
            // レガシーデッキを設定（後方互換性のため）
            Debug.LogWarning("GameManager: 新しいデッキシステムはPlayerDeckと互換性がありません。レガシーデッキを使用します。");
        }
        
        // レガシーデッキを設定
        playerDeck = deck;
        playerDeck.InitializeDrawPile();
    }
    
    /// <summary>
    /// メインシーン用の初期化（後方互換性）
    /// </summary>
    public void InitializeForMainScene()
    {
        gameOver = false;
        gameClear = false;
        
        if (currentFloor <= 0)
        {
            currentFloor = 1;
        }
        
        // プレイヤーデータを適用
        ApplyPlayerDataToPlayer();
    }
    
    /// <summary>
    /// デッキビルダーシーン用の初期化（後方互換性）
    /// </summary>
    public void InitializeForDeckBuilderScene()
    {
        // プレイヤーデータを同期
        SyncPlayerDataFromPlayer();
    }
    
    /// <summary>
    /// プレイヤーデータをプレイヤーに適用（後方互換性）
    /// </summary>
    public void ApplyPlayerDataToPlayer()
    {
        // プレイヤーデータシステムが利用可能な場合はそちらを使用
        if (useNewSystems && playerDataManager != null)
        {
            playerDataManager.ApplyToGameManager();
        }
        else
        {
            // 従来の処理（後方互換性のため）
            if (Player.Instance != null)
            {
                Player.Instance.level = playerLevel;
                Player.Instance.exp = playerExp;
                Player.Instance.expToNext = playerExpToNext;
                Player.Instance.maxHP = playerMaxHP;
                Player.Instance.currentHP = playerCurrentHP;
                Player.Instance.maxLevel = playerMaxLevel;
                
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
                    UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
                }
            }
        }
    }
    
    /// <summary>
    /// プレイヤーからプレイヤーデータを同期（後方互換性）
    /// </summary>
    public void SyncPlayerDataFromPlayer()
    {
        // プレイヤーデータシステムが利用可能な場合はそちらを使用
        if (useNewSystems && playerDataManager != null)
        {
            playerDataManager.SyncWithGameManager();
        }
        else
        {
            // 従来の処理（後方互換性のため）
            if (Player.Instance != null)
            {
                playerLevel = Player.Instance.level;
                playerExp = Player.Instance.exp;
                playerExpToNext = Player.Instance.expToNext;
                playerMaxHP = Player.Instance.maxHP;
                playerCurrentHP = Player.Instance.currentHP;
                playerMaxLevel = Player.Instance.maxLevel;
            }
        }
    }
    
    /// <summary>
    /// 敵撃破時の処理（後方互換性）
    /// </summary>
    public void EnemyDefeated()
    {
        Debug.Log("GameManager: EnemyDefeated()が呼び出されました");
        
        // 新しいシステムが利用可能な場合はそちらを使用
        if (useNewSystems && gameStateManager != null)
        {
            gameStateManager.AddScore(100);
            Debug.Log("GameManager: 新しいシステムでスコアを加算しました");
            
            // 経験値はPlayer.GainExp()で処理されるため、ここでは加算しない
            // if (playerDataManager != null)
            // {
            //     playerDataManager.AddPlayerExp(10);
            // }
        }
        else
        {
            // 従来の処理（後方互換性のため）
            score += 100;
            AddPlayerExp(10);
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"敵を倒した！スコア: {score}");
            }
            
            Debug.Log("GameManager: レガシーシステムでスコアと経験値を加算しました");
        }
    }
    
    // レガシーデッキシステム（後方互換性のため）
    [Header("Legacy Deck System")]
    public PlayerDeck playerDeck;
} 