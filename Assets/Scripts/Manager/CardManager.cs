using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-60)]
public class CardManager : MonoBehaviour
{
    public static CardManager Instance { get; private set; }
    
    public static CardManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("CardManager");
            Instance = go.AddComponent<CardManager>();
        }
        return Instance;
    }

    public Transform handArea;
    public GameObject cardUIPrefab;
    public CardDataSO[] cardPool;
    public int handSize = 7;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        // 設定項目のnullチェック
        if (handArea == null)
        {
            Debug.LogError("CardManager: handAreaが設定されていません");
        }
        
        if (cardUIPrefab == null)
        {
            Debug.LogError("CardManager: cardUIPrefabが設定されていません");
        }
        
        if (cardPool == null || cardPool.Length == 0)
        {
            Debug.LogError("CardManager: cardPoolが設定されていません");
        }
    }

    private void Start()
    {
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
    }
    
    private void UnsubscribeFromEvents()
    {
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
    }
    
    private void OnAllObjectsInitialized()
    {
        DrawHand();
    }

    public void DrawHand()
    {
        if (handArea == null)
        {
            Debug.LogError("CardManager: handAreaが設定されていません");
            return;
        }
        
        if (cardUIPrefab == null)
        {
            Debug.LogError("CardManager: cardUIPrefabが設定されていません");
            return;
        }
        
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.GetPlayerDeck() != null)
        {
            var playerDeck = gameManager.GetPlayerDeck();
            
            if (playerDeck.drawPile.Count == 0 || playerDeck.drawPile.Count < handSize)
            {
                playerDeck.InitializeDrawPile();
            }
            
            DrawHandFromDeck();
        }
        else
        {
            DrawHandFromPool();
        }
    }
    
    /// <summary>
    /// デッキから手札をドロー
    /// </summary>
    private void DrawHandFromDeck()
    {
        var playerDeck = GameManager.GetOrCreateInstance().GetPlayerDeck();
        
        if (playerDeck == null)
        {
            Debug.LogError("CardManager: PlayerDeckがnullです");
            return;
        }
        
        if (playerDeck.drawPile.Count == 0 || playerDeck.drawPile.Count < handSize)
        {
            playerDeck.InitializeDrawPile();
        }
        
        foreach (Transform child in handArea)
            Destroy(child.gameObject);

        int createdCount = 0;
        for (int i = 0; i < handSize; i++)
        {
            CardDataSO drawnCard = playerDeck.DrawCard();
            
            if (drawnCard == null)
            {
                Debug.LogWarning($"CardManager: ドロー可能なカードがありません (手札{i + 1}枚目)");
                break;
            }
            
            GameObject cardObj = Instantiate(cardUIPrefab, handArea);
            CardUI ui = cardObj.GetComponent<CardUI>();
            
            if (ui == null)
            {
                Debug.LogError($"CardManager: カード{i + 1}にCardUIコンポーネントが見つかりません");
                continue;
            }
            
            ui.Setup(drawnCard, OnCardClicked);
            createdCount++;
        }
    }
    
    /// <summary>
    /// カードプールから手札をドロー（フォールバック）
    /// </summary>
    private void DrawHandFromPool()
    {
        if (cardPool == null || cardPool.Length == 0)
        {
            Debug.LogError("CardManager: cardPoolが設定されていません");
            return;
        }
        
        foreach (Transform child in handArea)
            Destroy(child.gameObject);

        for (int i = 0; i < handSize; i++)
        {
            CardDataSO randomCard = cardPool[UnityEngine.Random.Range(0, cardPool.Length)];
            GameObject cardObj = Instantiate(cardUIPrefab, handArea);
            CardUI ui = cardObj.GetComponent<CardUI>();
            
            if (ui == null)
            {
                Debug.LogError($"CardManager: カード{i + 1}にCardUIコンポーネントが見つかりません");
                continue;
            }
            
            ui.Setup(randomCard, OnCardClicked);
        }
    }

    private void OnCardClicked(CardDataSO card)
    {
        if (card == null)
        {
            Debug.LogError("CardManager: クリックされたカードがnullです");
            return;
        }
        
        if (Player.Instance == null)
        {
            Debug.LogError("CardManager: Player.Instanceが見つかりません");
            return;
        }

        if (card.type == CardType.Move)
        {
            Player.Instance.StartMoveSelection(card.GetEffectiveMoveDistance());
        }
        else if (card.type == CardType.Attack)
        {
            Player.Instance.StartAttackSelection(card.GetEffectivePower());
        }
        else
        {
            // CardExecutorを使用してカード効果を実行
            if (CardExecutor.Instance != null && Player.Instance != null)
            {
                CardExecutor.Instance.ExecuteCardEffect(card, Player.Instance);
            }
            else
            {
                Debug.LogError("CardManager: CardExecutorまたはPlayerが見つかりません");
            }
            
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
            else
            {
                Debug.LogError("CardManager: TurnManager.Instanceが見つかりません");
            }
        }
    }
    
    public void EnhanceCard(CardDataSO card)
    {
        if (card == null) return;
        
        if (card.LevelUp())
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"カード強化！{card.cardName} Lv.{card.level}");
            }
            
            UpdateHandUI();
        }
        else
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"{card.cardName} は最大レベルです");
            }
        }
    }
    
    private void UpdateHandUI()
    {
        foreach (Transform child in handArea)
        {
            CardUI cardUI = child.GetComponent<CardUI>();
            if (cardUI != null)
            {
                cardUI.UpdateLevelDisplay();
            }
        }
    }
    
    public void AddCardToHand(CardDataSO card)
    {
        if (card == null || handArea == null || cardUIPrefab == null)
        {
            Debug.LogError("CardManager: AddCardToHand - 必要なコンポーネントが設定されていません");
            return;
        }
        
        GameObject cardObj = Instantiate(cardUIPrefab, handArea);
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        
        if (cardUI != null)
        {
            cardUI.Setup(card, OnCardClicked);
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"カードを獲得: {card.cardName}");
            }
        }
        else
        {
            Debug.LogError("CardManager: AddCardToHand - CardUIコンポーネントが見つかりません");
            Destroy(cardObj);
        }
    }

    private IEnumerator ExecuteCardEffect(CardDataSO card)
    {
        if (CardExecutor.Instance != null && Player.Instance != null)
        {
            CardExecutor.Instance.ExecuteCardEffect(card, Player.Instance);
        }
        else
        {
            Debug.LogError("CardManager: CardExecutorまたはPlayerが見つかりません");
        }

        yield return new WaitForSeconds(0.3f);
        
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.OnPlayerCardUsed();
        }
    }
    
    public void CleanupForSceneTransition()
    {
        if (handArea != null)
        {
            foreach (Transform child in handArea)
            {
                if (child != null)
                {
                    DestroyImmediate(child.gameObject);
                }
            }
        }
    }
} 