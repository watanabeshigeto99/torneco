using System.Collections;
using UnityEngine;

[DefaultExecutionOrder(-70)]
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private bool playerTurn = true;
    private bool isProcessing = false;

    public Player player;
    public EnemyManager enemyManager;

    private void Awake()
    {
        Debug.Log("TurnManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TurnManager: 重複するTurnManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        Debug.Log("TurnManager: Awake完了");
    }

    private void Start()
    {
        Debug.Log("TurnManager: Start開始");
        
        player = Player.Instance;
        enemyManager = EnemyManager.Instance;
        
        if (player == null)
        {
            Debug.LogError("TurnManager: Player.Instanceが見つかりません");
        }
        
        if (enemyManager == null)
        {
            Debug.LogError("TurnManager: EnemyManager.Instanceが見つかりません");
        }
        
        Debug.Log("TurnManager: Start完了");
    }

    public void OnPlayerCardUsed()
    {
        Debug.Log("TurnManager: プレイヤーカード使用");
        
        if (player == null)
        {
            Debug.LogError("TurnManager: playerがnullです");
            return;
        }
        
        if (enemyManager == null)
        {
            Debug.LogError("TurnManager: enemyManagerがnullです");
            return;
        }
        
        playerTurn = false;
        isProcessing = true;
        
        // 敵のターンを実行
        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        Debug.Log("TurnManager: 敵ターン開始");
        
        yield return new WaitForSeconds(0.5f);
        
        if (enemyManager != null)
        {
            enemyManager.EnemyTurn();
        }
        else
        {
            Debug.LogError("TurnManager: enemyManagerがnullのため敵ターンをスキップ");
        }
        
        // プレイヤーのターンに戻る
        playerTurn = true;
        isProcessing = false;
        
        Debug.Log("TurnManager: 敵ターン完了、プレイヤーターンに戻る");
    }

    public bool IsPlayerTurn()
    {
        return playerTurn && !isProcessing;
    }
} 