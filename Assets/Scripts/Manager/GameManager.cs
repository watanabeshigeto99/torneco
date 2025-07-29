using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int score = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void GameOver()
    {
        Debug.Log("ゲームオーバー！");
        // TODO: UI表示やリトライ処理
    }

    public void GameClear()
    {
        Debug.Log("ゲームクリア！");
        // TODO: UI表示やリトライ処理
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