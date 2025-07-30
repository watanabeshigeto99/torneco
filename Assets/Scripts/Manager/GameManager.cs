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
    
    // 階層システムイベント（準備段階）
    public static event System.Action<int> OnFloorChanged;
    public static event System.Action OnGameClear;
    public static event System.Action OnGameOver;

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
        
        Debug.Log("GameManager: Awake完了");
    }

    public void GameOver()
    {
        Debug.Log("GameManager: ゲームオーバー");
        gameOver = true;
        
        // ゲームオーバーイベントを発行
        OnGameOver?.Invoke();
        
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
        
        // 新しい階層の生成を開始
        StartCoroutine(GenerateFloorCoroutine(currentFloor));
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
} 