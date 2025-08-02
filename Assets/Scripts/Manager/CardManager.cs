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
    public int handSize = 3;

    private void Awake()
    {
        Debug.Log("CardManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("CardManager: 重複するCardManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化（シーン間で保持）
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
        
        Debug.Log("CardManager: Awake完了");
    }

    private void Start()
    {
        Debug.Log("CardManager: Start開始");
        
        // イベントを購読
        SubscribeToEvents();
        
        Debug.Log("CardManager: Start完了");
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        // 全オブジェクト初期化完了イベントを購読
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
        
        Debug.Log("CardManager: イベント購読完了");
    }
    
    private void UnsubscribeFromEvents()
    {
        // イベントの購読を解除
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
        
        Debug.Log("CardManager: イベント購読解除完了");
    }
    
    private void OnAllObjectsInitialized()
    {
        Debug.Log("CardManager: 全オブジェクト初期化完了イベント受信");
        
        // カード管理の初期化
        DrawHand();
        
        Debug.Log("CardManager: カード管理初期化完了");
    }

    public void DrawHand()
    {
        Debug.Log($"CardManager: 手札を引く開始 手札サイズ: {handSize}");
        
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
        
        // PlayerDeckからカードをドロー
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.GetPlayerDeck() != null)
        {
            DrawHandFromDeck();
        }
        else
        {
            // フォールバック: 従来のcardPoolからドロー
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
        
        Debug.Log($"CardManager: PlayerDeckから手札をドロー開始 - デッキサイズ: {playerDeck.selectedDeck.Count}");
        
        // 既存のカードを削除
        int childCount = handArea.childCount;
        foreach (Transform child in handArea)
            Destroy(child.gameObject);
        
        Debug.Log($"CardManager: 既存カード削除完了 削除数: {childCount}");

        // デッキからカードをドロー
        int createdCount = 0;
        for (int i = 0; i < handSize; i++)
        {
            CardDataSO drawnCard = playerDeck.DrawCard();
            
            if (drawnCard == null)
            {
                Debug.LogWarning($"CardManager: ドロー可能なカードがありません (手札{i + 1}枚目)");
                break;
            }
            
            Debug.Log($"CardManager: カードドロー - {drawnCard.cardName}");
            
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
        
        Debug.Log($"CardManager: デッキから手札をドロー完了 作成数: {createdCount}/{handSize}");
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
        
        // 既存のカードを削除
        int childCount = handArea.childCount;
        foreach (Transform child in handArea)
            Destroy(child.gameObject);
        
        Debug.Log($"CardManager: 既存カード削除完了 削除数: {childCount}");

        // 新しいカードを生成
        int createdCount = 0;
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
            createdCount++;
        }
        
        Debug.Log($"CardManager: カードプールから手札をドロー完了 作成数: {createdCount}/{handSize}");
    }

    private void OnCardClicked(CardDataSO card)
    {
        if (card == null)
        {
            Debug.LogError("CardManager: クリックされたカードがnullです");
            return;
        }
        
        Debug.Log($"CardManager: カードクリック {card.cardName} ({card.type})");
        
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
            // 攻撃カードは方向選択を開始（ターン終了は方向選択後に実行）
            Player.Instance.StartAttackSelection(card.GetEffectivePower());
        }
        else
        {
            // Healカードなどは即座に実行してターン終了
            Player.Instance.ExecuteCardEffect(card);
            
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
            else
            {
                Debug.LogError("CardManager: TurnManager.Instanceが見つかりません");
            }
        }
        
        Debug.Log($"CardManager: カード効果実行完了 {card.cardName}");
    }
    
    // カード強化メソッド（テスト用）
    public void EnhanceCard(CardDataSO card)
    {
        if (card == null) return;
        
        if (card.LevelUp())
        {
            Debug.Log($"CardManager: カード強化成功！{card.cardName} Lv.{card.level}");
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"カード強化！{card.cardName} Lv.{card.level}");
            }
            
            // 手札のUIを更新
            UpdateHandUI();
        }
        else
        {
            Debug.Log($"CardManager: カード強化失敗！{card.cardName} は最大レベルです");
            
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"{card.cardName} は最大レベルです");
            }
        }
    }
    
    // 手札のUIを更新
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
    
    // 手札にカードを追加
    public void AddCardToHand(CardDataSO card)
    {
        if (card == null || handArea == null || cardUIPrefab == null)
        {
            Debug.LogError("CardManager: AddCardToHand - 必要なコンポーネントが設定されていません");
            return;
        }
        
        // カードUIを生成
        GameObject cardObj = Instantiate(cardUIPrefab, handArea);
        CardUI cardUI = cardObj.GetComponent<CardUI>();
        
        if (cardUI != null)
        {
            cardUI.Setup(card, OnCardClicked);
            Debug.Log($"CardManager: 手札にカードを追加 - {card.cardName}");
            
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
    
    /// <summary>
    /// シーン遷移時のクリーンアップ処理
    /// </summary>
    public void CleanupForSceneTransition()
    {
        Debug.Log("CardManager: シーン遷移時のクリーンアップ開始");
        
        // 手札をクリア
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
        
        Debug.Log("CardManager: シーン遷移時のクリーンアップ完了");
    }
} 