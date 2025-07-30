using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-60)]
public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }

    public Transform handArea;
    public GameObject cardUIPrefab;
    public CardDataSO[] cardPool;
    public int handSize = 3;

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
        DrawHand();
    }

    public void DrawHand()
    {
        // 既存のカードを削除
        foreach (Transform child in handArea)
            Destroy(child.gameObject);

        // 新しいカードを生成
        for (int i = 0; i < handSize; i++)
        {
            CardDataSO randomCard = cardPool[Random.Range(0, cardPool.Length)];
            GameObject cardObj = Instantiate(cardUIPrefab, handArea);
            CardUI ui = cardObj.GetComponent<CardUI>();
            ui.Setup(randomCard, OnCardClicked);
        }
    }

    private void OnCardClicked(CardDataSO card)
    {
        if (Player.Instance == null) return;

        if (card.type == CardType.Move)
        {
            Player.Instance.StartMoveSelection(card.moveDistance);
        }
        else
        {
            Player.Instance.ExecuteCardEffect(card);
            TurnManager.Instance.OnPlayerCardUsed();
        }
    }

    private IEnumerator ExecuteCardEffect(CardDataSO card)
    {
        // プレイヤーにカード効果を実行させる
        if (Player.Instance != null)
        {
            Player.Instance.ExecuteCardEffect(card);
        }

        yield return new WaitForSeconds(0.3f);
        
        // ターン管理に通知
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerCardUsed();
        }
    }
} 