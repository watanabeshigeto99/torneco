using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }

    private bool playerTurn = true;
    private bool isProcessing = false;

    public Player player;
    public EnemyManager enemyManager;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        player = FindObjectOfType<Player>();
        enemyManager = FindObjectOfType<EnemyManager>();
    }

    public void OnPlayerCardUsed()
    {
        if (playerTurn && !isProcessing)
        {
            StartCoroutine(HandleTurnSequence());
        }
    }

    private IEnumerator HandleTurnSequence()
    {
        isProcessing = true;
        playerTurn = false;

        yield return new WaitForSeconds(0.3f);

        // 敵のターン
        if (enemyManager != null)
        {
            enemyManager.EnemyTurn();
        }

        yield return new WaitForSeconds(0.3f);

        // プレイヤーのターンに戻る
        playerTurn = true;

        // プレイヤーのターン開始時にログをクリア
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ClearLog();
        }

        // 手札を補充
        if (CardManager.Instance != null)
        {
            CardManager.Instance.DrawHand();
        }

        isProcessing = false;
    }

    public bool IsPlayerTurn()
    {
        return playerTurn && !isProcessing;
    }
} 