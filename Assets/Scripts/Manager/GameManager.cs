using UnityEngine;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public int score = 0;
    public bool gameOver = false;
    public bool gameClear = false;

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
        
        // 基本的な変数の初期化
        score = 0;
        gameOver = false;
        gameClear = false;
        
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
} 