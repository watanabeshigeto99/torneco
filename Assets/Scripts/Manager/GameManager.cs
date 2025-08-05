using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    // ゲーム状態
    [Header("Game State")]
    public int score = 0;
    public bool gameOver = false;
    public bool gameClear = false;
    
    // 階層システム関連（準備段階）
    [Header("Floor System - Preparation")]
    public int currentFloor = 1;
    public int maxFloor = 10;
    
    // プレイヤーデータ（永続化）
    [Header("Player Data - Persistent")]
    public int playerLevel = 1;
    public int playerExp = 0;
    public int playerExpToNext = 10;
    public int playerMaxHP = 20;
    public int playerCurrentHP = 20;
    public int playerMaxLevel = 10;
    
    // 階層システムイベント（準備段階）
    public static event System.Action<int> OnFloorChanged;
    public static event System.Action OnGameClear;
    public static event System.Action OnGameOver;
    public static event System.Action<int> OnPlayerLevelUp; // プレイヤーレベルアップイベント
    
    // デッキシステム
    [Header("Deck System")]
    public PlayerDeck playerDeck;
    
    // 戦闘システム統合
    [Header("Battle System Integration")]
    public BattleSystem.BattleStarter battleStarter;
    public BattleSystem.BattleStateSO battleState;
    public BattleSystem.BattleEventChannel battleEventChannel;
    
    // プレイヤーデータシステム統合
    [Header("Player Data System Integration")]
    public PlayerDataSystem.PlayerDataManager playerDataManager;
    public PlayerDataSystem.PlayerDataSO playerData;
    public PlayerDataSystem.PlayerEventChannel playerEventChannel;
    
    // 階層システム統合
    [Header("Floor System Integration")]
    public FloorSystem.FloorManager floorManager;
    public FloorSystem.FloorDataSO floorData;
    public FloorSystem.FloorEventChannel floorEventChannel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        score = 0;
        gameOver = false;
        gameClear = false;
        currentFloor = 1;
        
        if (playerLevel <= 0)
        {
            InitializePlayerData();
        }
        
        // 戦闘システムイベントの購読
        SubscribeToBattleEvents();
        
        // プレイヤーデータシステムイベントの購読
        SubscribeToPlayerDataEvents();
        
        // 階層システムイベントの購読
        SubscribeToFloorEvents();
    }
    
    private void InitializePlayerData()
    {
        playerLevel = 1;
        playerExp = 0;
        playerExpToNext = 10;
        playerMaxHP = 20;
        playerCurrentHP = 20;
        playerMaxLevel = 10;
    }
    
    public void SyncPlayerDataFromPlayer()
    {
        // プレイヤーデータシステムが利用可能な場合はそちらを使用
        if (playerDataManager != null)
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
    
    public void ApplyPlayerDataToPlayer()
    {
        // プレイヤーデータシステムが利用可能な場合はそちらを使用
        if (playerDataManager != null)
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
    
    public void AddPlayerExp(int amount)
    {
        // プレイヤーデータシステムが利用可能な場合はそちらを使用
        if (playerDataManager != null)
        {
            playerDataManager.AddPlayerExp(amount);
        }
        else
        {
            // 従来の処理（後方互換性のため）
            if (playerLevel >= playerMaxLevel) return;
            
            playerExp += amount;
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"経験値獲得！+{amount}");
                UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
            }
            
            while (playerExp >= playerExpToNext && playerLevel < playerMaxLevel)
            {
                playerExp -= playerExpToNext;
                PlayerLevelUp();
            }
            
            if (Player.Instance != null)
            {
                Player.Instance.level = playerLevel;
                Player.Instance.exp = playerExp;
                Player.Instance.expToNext = playerExpToNext;
            }
        }
    }
    
    private void PlayerLevelUp()
    {
        playerLevel++;
        
        int oldMaxHP = playerMaxHP;
        playerMaxHP += 5;
        playerCurrentHP = playerMaxHP;
        playerExpToNext = Mathf.RoundToInt(playerExpToNext * 1.5f);
        
        OnPlayerLevelUp?.Invoke(playerLevel);
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
            UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
            UIManager.Instance.AddLog($"レベルアップ！レベル {playerLevel}、HP {playerMaxHP}");
        }
        
        if (Player.Instance != null)
        {
            Player.Instance.level = playerLevel;
            Player.Instance.maxHP = playerMaxHP;
            Player.Instance.currentHP = playerCurrentHP;
            Player.Instance.expToNext = playerExpToNext;
        }
    }
    
    public void SetPlayerHP(int currentHP, int maxHP = -1)
    {
        // プレイヤーデータシステムが利用可能な場合はそちらを使用
        if (playerDataManager != null)
        {
            playerDataManager.SetPlayerHP(currentHP, maxHP);
        }
        else
        {
            // 従来の処理（後方互換性のため）
            playerCurrentHP = currentHP;
            if (maxHP > 0)
            {
                playerMaxHP = maxHP;
            }
            
            if (Player.Instance != null)
            {
                Player.Instance.currentHP = playerCurrentHP;
                Player.Instance.maxHP = playerMaxHP;
            }
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
            }
        }
    }
    
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

    public void GameOver()
    {
        gameOver = true;
        OnGameOver?.Invoke();
        
        // GameStateManagerに敗北状態を通知
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Defeat();
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene("GameOverScene");
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog("ゲームオーバー！");
        }
    }

    public void GameClear()
    {
        gameClear = true;
        OnGameClear?.Invoke();
        
        // GameStateManagerに勝利状態を通知
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.Victory();
        }
        
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene("GameClearScene");
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog("ゲームクリア！");
        }
    }
    
    public void EnemyDefeated()
    {
        // 戦闘システムが利用可能な場合はそちらを使用
        if (battleStarter != null)
        {
            battleStarter.HandleEnemyDefeated();
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
        }
    }
    
    public void GoToNextFloor()
    {
        // 階層システムが利用可能な場合はそちらを使用
        if (floorManager != null)
        {
            floorManager.GoToNextFloor();
        }
        else
        {
            // 従来の処理（後方互換性のため）
            if (gameOver || gameClear) 
            {
                return;
            }
            
            currentFloor++;
            OnFloorChanged?.Invoke(currentFloor);
            
            if (currentFloor > maxFloor)
            {
                GameClear();
                return;
            }
            
            StartCoroutine(GoToDeckBuilderCoroutine());
        }
    }
    
    private System.Collections.IEnumerator GoToDeckBuilderCoroutine()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"階層 {currentFloor} に進みます。デッキを再構築してください。");
        }
        
        yield return new WaitForSeconds(1f);
        
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithFade("DeckBuilderScene");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
        }
    }
    
    private System.Collections.IEnumerator GenerateFloorCoroutine(int floorNumber)
    {
        if (GridManager.Instance != null)
        {
            GridManager.Instance.GenerateNewFloor();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogError("GameManager: GridManager.Instanceが見つかりません");
        }
        
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RespawnEnemies();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogError("GameManager: EnemyManager.Instanceが見つかりません");
        }
        
        if (Player.Instance != null)
        {
            Player.Instance.ResetPlayerPosition();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogError("GameManager: Player.Instanceが見つかりません");
        }
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"階層 {floorNumber} に到達しました");
        }
    }
    
    public void SetPlayerDeck(PlayerDeck deck)
    {
        if (deck == null)
        {
            Debug.LogError("GameManager: SetPlayerDeck - deckがnullです");
            return;
        }
        
        playerDeck = deck;
        playerDeck.InitializeDrawPile();
    }
    
    public PlayerDeck GetPlayerDeck()
    {
        if (playerDeck == null)
        {
            Debug.LogWarning("GameManager: GetPlayerDeck - playerDeckがnullです");
        }
        return playerDeck;
    }
    
    public void InitializeForMainScene()
    {
        gameOver = false;
        gameClear = false;
        
        if (currentFloor <= 0)
        {
            currentFloor = 1;
        }
        
        ApplyPlayerDataToPlayer();
    }
    
    public void InitializeForDeckBuilderScene()
    {
        SyncPlayerDataFromPlayer();
    }
    
    // 戦闘システム統合用メソッド
    
    /// <summary>
    /// 戦闘システムイベントの購読
    /// </summary>
    private void SubscribeToBattleEvents()
    {
        if (battleEventChannel != null)
        {
            battleEventChannel.OnBattleEnded.AddListener(OnBattleEnded);
            battleEventChannel.OnUnitDefeated.AddListener(OnUnitDefeated);
        }
    }
    
    /// <summary>
    /// 戦闘終了時の処理
    /// </summary>
    private void OnBattleEnded(BattleSystem.BattleStateSO battleState)
    {
        switch (battleState.battleResult)
        {
            case BattleSystem.BattleResult.PlayerVictory:
                HandlePlayerVictory();
                break;
            case BattleSystem.BattleResult.EnemyVictory:
                HandlePlayerDefeat();
                break;
        }
    }
    
    /// <summary>
    /// ユニット撃破時の処理
    /// </summary>
    private void OnUnitDefeated(BattleSystem.BattleStateSO battleState)
    {
        // プレイヤーデータの同期
        SyncPlayerDataWithBattleSystem();
    }
    
    /// <summary>
    /// プレイヤー勝利時の処理
    /// </summary>
    private void HandlePlayerVictory()
    {
        GameClear();
    }
    
    /// <summary>
    /// プレイヤー敗北時の処理
    /// </summary>
    private void HandlePlayerDefeat()
    {
        GameOver();
    }
    
    /// <summary>
    /// プレイヤーデータを戦闘システムと同期
    /// </summary>
    private void SyncPlayerDataWithBattleSystem()
    {
        if (battleState != null && battleState.playerUnit != null)
        {
            // プレイヤーデータを戦闘システムと同期
            playerCurrentHP = battleState.playerUnit.currentHP;
            playerMaxHP = battleState.playerUnit.maxHP;
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
            }
        }
    }
    
    /// <summary>
    /// 戦闘システム統合の情報を取得
    /// </summary>
    public string GetBattleSystemIntegrationInfo()
    {
        return $"BattleSystem Integration - Starter: {(battleStarter != null ? "✓" : "✗")}, " +
               $"State: {(battleState != null ? "✓" : "✗")}, " +
               $"EventChannel: {(battleEventChannel != null ? "✓" : "✗")}";
    }
    
    // プレイヤーデータシステム統合用メソッド
    
    /// <summary>
    /// プレイヤーデータシステムイベントの購読
    /// </summary>
    private void SubscribeToPlayerDataEvents()
    {
        if (playerEventChannel != null)
        {
            playerEventChannel.OnPlayerLevelUp.AddListener(OnPlayerDataLevelUp);
            playerEventChannel.OnPlayerExpGained.AddListener(OnPlayerExpGained);
            playerEventChannel.OnPlayerHPChanged.AddListener(OnPlayerHPChanged);
        }
    }
    
    /// <summary>
    /// プレイヤーレベルアップ時の処理
    /// </summary>
    private void OnPlayerDataLevelUp(int newLevel)
    {
        // 既存のレベルアップ処理を呼び出し
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateLevelDisplay(newLevel, playerData?.experience ?? 0, playerData?.experienceToNext ?? 10);
            UIManager.Instance.AddLog($"レベルアップ！レベル {newLevel}");
        }
    }
    
    /// <summary>
    /// プレイヤー経験値獲得時の処理
    /// </summary>
    private void OnPlayerExpGained(int expAmount)
    {
        // 既存の経験値獲得処理を呼び出し
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"経験値獲得！+{expAmount}");
        }
    }
    
    /// <summary>
    /// プレイヤーHP変更時の処理
    /// </summary>
    private void OnPlayerHPChanged(int currentHP, int maxHP)
    {
        // 既存のHP変更処理を呼び出し
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
        }
    }
    
    /// <summary>
    /// プレイヤーデータシステム統合の情報を取得
    /// </summary>
    public string GetPlayerDataSystemIntegrationInfo()
    {
        return $"PlayerDataSystem Integration - Manager: {(playerDataManager != null ? "✓" : "✗")}, " +
               $"Data: {(playerData != null ? "✓" : "✗")}, " +
               $"EventChannel: {(playerEventChannel != null ? "✓" : "✗")}";
    }
    
    // 階層システム統合用メソッド
    
    /// <summary>
    /// 階層システムイベントの購読
    /// </summary>
    private void SubscribeToFloorEvents()
    {
        if (floorEventChannel != null)
        {
            floorEventChannel.OnFloorChanged.AddListener(OnFloorSystemChanged);
            floorEventChannel.OnGameClear.AddListener(OnFloorGameClear);
        }
    }
    
    /// <summary>
    /// 階層変更時の処理
    /// </summary>
    private void OnFloorSystemChanged(int newFloor)
    {
        // 既存の階層変更処理を呼び出し
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"階層 {newFloor} に進みました");
        }
    }
    
    /// <summary>
    /// 階層システムからのゲームクリア処理
    /// </summary>
    private void OnFloorGameClear()
    {
        // 既存のゲームクリア処理を呼び出し
        GameClear();
    }
    
    /// <summary>
    /// 階層データを階層システムと同期
    /// </summary>
    private void SyncFloorDataWithFloorSystem()
    {
        if (floorData != null)
        {
            currentFloor = floorData.currentFloor;
            maxFloor = floorData.maxFloor;
        }
    }
    
    /// <summary>
    /// 階層データを階層システムに適用
    /// </summary>
    private void ApplyFloorDataToFloorSystem()
    {
        if (floorData != null)
        {
            floorData.SetFloor(currentFloor);
        }
    }
    
    /// <summary>
    /// 階層システム統合の情報を取得
    /// </summary>
    public string GetFloorSystemIntegrationInfo()
    {
        return $"FloorSystem Integration - Manager: {(floorManager != null ? "✓" : "✗")}, " +
               $"Data: {(floorData != null ? "✓" : "✗")}, " +
               $"EventChannel: {(floorEventChannel != null ? "✓" : "✗")}";
    }
} 