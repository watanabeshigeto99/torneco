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

        Debug.Log("プレイヤーのターン終了");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetTurnText(false);
            UIManager.Instance.AddLog("プレイヤーのターン終了");
        }

        yield return new WaitForSeconds(0.3f);

        // 敵のターン
        if (enemyManager != null)
        {
            enemyManager.EnemyTurn();
        }

        yield return new WaitForSeconds(0.3f);

        // プレイヤーのターンに戻る
        playerTurn = true;
        Debug.Log("プレイヤーのターン開始");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.SetTurnText(true);
            UIManager.Instance.AddLog("プレイヤーのターン開始");
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