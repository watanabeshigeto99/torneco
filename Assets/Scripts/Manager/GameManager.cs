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

    private void Awake()
    {
        Debug.Log("GameManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager: 重複するGameManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化（階層間で保持）
        DontDestroyOnLoad(gameObject);
        
        // 基本的な変数の初期化
        score = 0;
        gameOver = false;
        gameClear = false;
        currentFloor = 1; // 階層システム準備
        
        // プレイヤーデータの初期化（初回のみ）
        if (playerLevel <= 0)
        {
            InitializePlayerData();
        }
        
        // デッキが既に設定されているかチェック
        if (playerDeck != null)
        {
            Debug.Log($"GameManager: 既存のデッキを保持 - {playerDeck.selectedDeck.Count}枚");
        }
        
        Debug.Log("GameManager: Awake完了");
    }
    
    /// <summary>
    /// プレイヤーデータの初期化
    /// </summary>
    private void InitializePlayerData()
    {
        playerLevel = 1;
        playerExp = 0;
        playerExpToNext = 10;
        playerMaxHP = 20;
        playerCurrentHP = 20;
        playerMaxLevel = 10;
        
        Debug.Log("GameManager: プレイヤーデータを初期化しました");
    }
    
    /// <summary>
    /// プレイヤーデータをPlayerクラスから同期
    /// </summary>
    public void SyncPlayerDataFromPlayer()
    {
        if (Player.Instance != null)
        {
            playerLevel = Player.Instance.level;
            playerExp = Player.Instance.exp;
            playerExpToNext = Player.Instance.expToNext;
            playerMaxHP = Player.Instance.maxHP;
            playerCurrentHP = Player.Instance.currentHP;
            playerMaxLevel = Player.Instance.maxLevel;
            
            Debug.Log($"GameManager: プレイヤーデータを同期しました - レベル: {playerLevel}, 経験値: {playerExp}/{playerExpToNext}, HP: {playerCurrentHP}/{playerMaxHP}");
        }
    }
    
    /// <summary>
    /// プレイヤーデータをPlayerクラスに適用
    /// </summary>
    public void ApplyPlayerDataToPlayer()
    {
        if (Player.Instance != null)
        {
            Player.Instance.level = playerLevel;
            Player.Instance.exp = playerExp;
            Player.Instance.expToNext = playerExpToNext;
            Player.Instance.maxHP = playerMaxHP;
            Player.Instance.currentHP = playerCurrentHP;
            Player.Instance.maxLevel = playerMaxLevel;
            
            Debug.Log($"GameManager: プレイヤーデータを適用しました - レベル: {playerLevel}, 経験値: {playerExp}/{playerExpToNext}, HP: {playerCurrentHP}/{playerMaxHP}");
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
                UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
            }
        }
    }
    
    /// <summary>
    /// プレイヤーに経験値を追加
    /// </summary>
    public void AddPlayerExp(int amount)
    {
        if (playerLevel >= playerMaxLevel) return; // 最大レベルに達している場合は経験値を獲得しない
        
        playerExp += amount;
        Debug.Log($"GameManager: プレイヤー経験値獲得！+{amount} (現在: {playerExp}/{playerExpToNext})");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"経験値獲得！+{amount}");
            UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
        }
        
        // レベルアップチェック
        while (playerExp >= playerExpToNext && playerLevel < playerMaxLevel)
        {
            playerExp -= playerExpToNext;
            PlayerLevelUp();
        }
        
        // Playerクラスにも同期
        if (Player.Instance != null)
        {
            Player.Instance.level = playerLevel;
            Player.Instance.exp = playerExp;
            Player.Instance.expToNext = playerExpToNext;
        }
    }
    
    /// <summary>
    /// プレイヤーレベルアップ処理
    /// </summary>
    private void PlayerLevelUp()
    {
        playerLevel++;
        
        // レベルアップ時のステータス上昇
        int oldMaxHP = playerMaxHP;
        playerMaxHP += 5; // レベルアップでHP+5
        playerCurrentHP = playerMaxHP; // HP全回復
        playerExpToNext = Mathf.RoundToInt(playerExpToNext * 1.5f); // 次のレベルに必要な経験値を1.5倍
        
        Debug.Log($"GameManager: プレイヤーレベルアップ！レベル {playerLevel}、HP {oldMaxHP}→{playerMaxHP}");
        
        // レベルアップイベントを発行
        OnPlayerLevelUp?.Invoke(playerLevel);
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
            UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
            UIManager.Instance.AddLog($"レベルアップ！レベル {playerLevel}、HP {playerMaxHP}");
        }
        
        // Playerクラスにも同期
        if (Player.Instance != null)
        {
            Player.Instance.level = playerLevel;
            Player.Instance.maxHP = playerMaxHP;
            Player.Instance.currentHP = playerCurrentHP;
            Player.Instance.expToNext = playerExpToNext;
        }
    }
    
    /// <summary>
    /// プレイヤーのHPを変更
    /// </summary>
    public void SetPlayerHP(int currentHP, int maxHP = -1)
    {
        playerCurrentHP = currentHP;
        if (maxHP > 0)
        {
            playerMaxHP = maxHP;
        }
        
        Debug.Log($"GameManager: プレイヤーHP設定 - {playerCurrentHP}/{playerMaxHP}");
        
        // Playerクラスにも同期
        if (Player.Instance != null)
        {
            Player.Instance.currentHP = playerCurrentHP;
            Player.Instance.maxHP = playerMaxHP;
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
        }
    }
    
    /// <summary>
    /// GameManagerインスタンスを取得（存在しない場合は作成）
    /// </summary>
    public static GameManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            Debug.Log("GameManager: インスタンスが存在しないため、新しく作成します");
            
            // シーン内でGameManagerを探す
            GameManager existingManager = FindObjectOfType<GameManager>();
            if (existingManager != null)
            {
                Instance = existingManager;
                Debug.Log("GameManager: 既存のGameManagerを見つけました");
            }
            else
            {
                // 新しいGameManagerを作成
                GameObject gameManagerObj = new GameObject("GameManager");
                Instance = gameManagerObj.AddComponent<GameManager>();
                Debug.Log("GameManager: 新しいGameManagerを作成しました");
            }
        }
        
        return Instance;
    }

    public void GameOver()
    {
        Debug.Log("GameManager: ゲームオーバー");
        gameOver = true;
        
        // ゲームオーバーイベントを発行
        OnGameOver?.Invoke();
        
        // BGMをゲームオーバー用に切り替え
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene("GameOverScene");
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog("ゲームオーバー！");
        }
    }

    public void GameClear()
    {
        Debug.Log("GameManager: ゲームクリア");
        gameClear = true;
        
        // ゲームクリアイベントを発行
        OnGameClear?.Invoke();
        
        // BGMをゲームクリア用に切り替え
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene("GameClearScene");
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog("ゲームクリア！");
        }
    }
    
    // 敵撃破時の処理
    public void EnemyDefeated()
    {
        score += 100; // 敵撃破で100ポイント加算
        
        // 敵撃破で経験値獲得
        AddPlayerExp(10); // 敵撃破で10経験値獲得
        
        Debug.Log($"GameManager: 敵撃破！スコア: {score}");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"敵を倒した！スコア: {score}");
        }
    }
    
    // 階層システム実装（段階3実装）
    public void GoToNextFloor()
    {
        if (gameOver || gameClear) 
        {
            Debug.Log("GameManager: ゲーム終了中なので階層進行をスキップ");
            return;
        }
        
        currentFloor++;
        Debug.Log($"GameManager: 階層 {currentFloor} に進みます");
        
        // 階層変更イベントを発行
        OnFloorChanged?.Invoke(currentFloor);
        
        // 最大階層に到達したらクリア
        if (currentFloor > maxFloor)
        {
            GameClear();
            return;
        }
        
        // デッキビルダーシーンに遷移してデッキを再構築
        StartCoroutine(GoToDeckBuilderCoroutine());
    }
    
    /// <summary>
    /// デッキビルダーシーンに遷移するコルーチン
    /// </summary>
    private System.Collections.IEnumerator GoToDeckBuilderCoroutine()
    {
        Debug.Log("GameManager: デッキビルダーシーンに遷移します");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"階層 {currentFloor} に進みます。デッキを再構築してください。");
        }
        
        // 少し待ってからシーン遷移
        yield return new WaitForSeconds(1f);
        
        // デッキビルダーシーンに遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // DeckBuilderScene index
    }
    
    // 階層生成処理
    private System.Collections.IEnumerator GenerateFloorCoroutine(int floorNumber)
    {
        Debug.Log($"GameManager: 階層 {floorNumber} の生成を開始");
        
        // 1. グリッドの再生成
        if (GridManager.Instance != null)
        {
            GridManager.Instance.GenerateNewFloor();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogError("GameManager: GridManager.Instanceが見つかりません");
        }
        
        // 2. 敵の再スポーン
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RespawnEnemies();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogError("GameManager: EnemyManager.Instanceが見つかりません");
        }
        
        // 3. プレイヤーの位置リセット
        if (Player.Instance != null)
        {
            Player.Instance.ResetPlayerPosition();
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.LogError("GameManager: Player.Instanceが見つかりません");
        }
        
        // 4. UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"階層 {floorNumber} に到達しました");
        }
        
        Debug.Log($"GameManager: 階層 {floorNumber} の生成完了");
    }
    
    /// <summary>
    /// プレイヤーデッキを設定
    /// </summary>
    public void SetPlayerDeck(PlayerDeck deck)
    {
        if (deck == null)
        {
            Debug.LogError("GameManager: SetPlayerDeck - deckがnullです");
            return;
        }
        
        playerDeck = deck;
        Debug.Log($"GameManager: プレイヤーデッキを設定 - {deck.selectedDeck.Count}枚");
        
        // デッキの内容をログ出力
        for (int i = 0; i < deck.selectedDeck.Count; i++)
        {
            var card = deck.selectedDeck[i];
            Debug.Log($"GameManager: デッキ[{i}]: {card.cardName} ({card.type})");
        }
    }
    
    /// <summary>
    /// プレイヤーデッキを取得
    /// </summary>
    public PlayerDeck GetPlayerDeck()
    {
        if (playerDeck == null)
        {
            Debug.LogWarning("GameManager: GetPlayerDeck - playerDeckがnullです");
        }
        else
        {
            Debug.Log($"GameManager: GetPlayerDeck - デッキサイズ: {playerDeck.selectedDeck.Count}枚");
        }
        return playerDeck;
    }
    
    /// <summary>
    /// メインシーン用の初期化処理
    /// </summary>
    public void InitializeForMainScene()
    {
        Debug.Log("GameManager: メインシーン初期化開始");
        
        // ゲーム状態の初期化
        gameOver = false;
        gameClear = false;
        
        // 階層システムの初期化
        if (currentFloor <= 0)
        {
            currentFloor = 1;
        }
        
        // プレイヤーデータをPlayerクラスに適用
        ApplyPlayerDataToPlayer();
        
        Debug.Log($"GameManager: メインシーン初期化完了 - 現在階層: {currentFloor}, プレイヤーレベル: {playerLevel}");
    }
    
    /// <summary>
    /// デッキビルダーシーン用の初期化処理
    /// </summary>
    public void InitializeForDeckBuilderScene()
    {
        Debug.Log("GameManager: デッキビルダーシーン初期化開始");
        
        // プレイヤーデータをPlayerクラスから同期
        SyncPlayerDataFromPlayer();
        
        Debug.Log($"GameManager: デッキビルダーシーン初期化完了 - プレイヤーレベル: {playerLevel}");
    }
} 