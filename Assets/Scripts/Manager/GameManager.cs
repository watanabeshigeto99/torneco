using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public int score = 0;
    public bool gameOver = false;
    public bool gameClear = false;
    
    // 階層管理
    [Header("Floor Management")]
    public int currentFloor = 1;
    public int maxFloor = 10; // 最大階層数（ゲームクリア条件）

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
        
        // 永続化（次の階層でも保持）
        DontDestroyOnLoad(gameObject);
        
        // 基本的な変数の初期化
        score = 0;
        gameOver = false;
        gameClear = false;
        currentFloor = 1;
        
        Debug.Log("GameManager: Awake完了");
    }

    public void GameOver()
    {
        Debug.Log("GameManager: ゲームオーバー");
        gameOver = true;
        
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
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog("ゲームクリア！");
        }
    }

    public void EnemyDefeated()
    {
        score += 10;
        Debug.Log($"Score: {score}");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"敵を倒した！スコア: {score}");
        }
    }
    
    // 次の階層に進む
    public void GoToNextFloor()
    {
        currentFloor++;
        Debug.Log($"階層: {currentFloor}");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"階層 {currentFloor} に進みました！");
            UIManager.Instance.UpdateFloorDisplay(currentFloor);
        }
        
        // 最大階層に到達したらゲームクリア
        if (currentFloor > maxFloor)
        {
            GameClear();
            return;
        }
        
        // 新しい階層を生成
        if (GridManager.Instance != null)
        {
            GridManager.Instance.GenerateNewFloor();
        }
        
        // 敵を再スポーン
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.RespawnEnemies();
        }
        
        // プレイヤー位置をリセット
        if (Player.Instance != null)
        {
            Player.Instance.ResetPlayerPosition();
        }
    }
} 