using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class DeckBuilderUI : MonoBehaviour
{
    [Header("Card Database")]
    public CardDatabase cardDatabase;
    
    [Header("Card List Area")]
    public Transform cardListContent;
    public GameObject cardListItemPrefab;
    public ScrollRect cardListScrollView;
    
    [Header("Selected Deck Area")]
    public Transform selectedDeckContent;
    public GameObject selectedCardPrefab;
    public TextMeshProUGUI deckSizeText;
    public TextMeshProUGUI deckStatisticsText;
    
    [Header("Control Buttons")]
    public Button addCardButton;
    public Button removeCardButton;
    public Button startBattleButton;
    public Button clearDeckButton;
    
    [Header("Filter Buttons")]
    public Button allCardsButton;
    public Button attackCardsButton;
    public Button moveCardsButton;
    public Button healCardsButton;
    
    [Header("Settings")]
    public int maxDeckSize = 10;
    public int minDeckSize = 5;
    
    private List<CardDataSO> availableCards = new List<CardDataSO>();
    private List<CardDataSO> selectedDeck = new List<CardDataSO>();
    private CardType currentFilter = CardType.Attack;
    
    private void Start()
    {
        InitializeUI();
        LoadAvailableCards();
        UpdateUI();
    }
    
    /// <summary>
    /// UIを初期化
    /// </summary>
    private void InitializeUI()
    {
        if (cardDatabase == null)
        {
            Debug.LogError("DeckBuilderUI: CardDatabaseが設定されていません");
            return;
        }
        
        // ボタンイベントを設定
        if (addCardButton != null)
            addCardButton.onClick.AddListener(OnAddCardClicked);
        
        if (removeCardButton != null)
            removeCardButton.onClick.AddListener(OnRemoveCardClicked);
        
        if (startBattleButton != null)
            startBattleButton.onClick.AddListener(OnStartBattleClicked);
        
        if (clearDeckButton != null)
            clearDeckButton.onClick.AddListener(OnClearDeckClicked);
        
        // フィルターボタンのイベントを設定
        if (allCardsButton != null)
            allCardsButton.onClick.AddListener(() => SetCardFilter(CardType.Attack)); // 全カード表示
        
        if (attackCardsButton != null)
            attackCardsButton.onClick.AddListener(() => SetCardFilter(CardType.Attack));
        
        if (moveCardsButton != null)
            moveCardsButton.onClick.AddListener(() => SetCardFilter(CardType.Move));
        
        if (healCardsButton != null)
            healCardsButton.onClick.AddListener(() => SetCardFilter(CardType.Heal));
        
        Debug.Log("DeckBuilderUI: UI初期化完了");
    }
    
    /// <summary>
    /// 利用可能なカードを読み込み
    /// </summary>
    private void LoadAvailableCards()
    {
        if (cardDatabase != null)
        {
            availableCards = cardDatabase.GetAllCards();
            Debug.Log($"DeckBuilderUI: 利用可能カード読み込み完了 - {availableCards.Count}枚");
        }
        else
        {
            Debug.LogError("DeckBuilderUI: CardDatabaseが設定されていません");
        }
    }
    
    /// <summary>
    /// カードリストを更新
    /// </summary>
    private void UpdateCardList()
    {
        if (cardListContent == null || cardListItemPrefab == null) return;
        
        // 既存のカードリストアイテムをクリア
        foreach (Transform child in cardListContent)
        {
            Destroy(child.gameObject);
        }
        
        // フィルタリングされたカードを取得
        var filteredCards = GetFilteredCards();
        
        // カードリストアイテムを作成
        foreach (var card in filteredCards)
        {
            var cardItem = Instantiate(cardListItemPrefab, cardListContent);
            var cardItemUI = cardItem.GetComponent<CardListItemUI>();
            
            if (cardItemUI != null)
            {
                cardItemUI.Setup(card, OnCardListItemClicked);
            }
        }
        
        Debug.Log($"DeckBuilderUI: カードリスト更新完了 - {filteredCards.Count}枚表示");
    }
    
    /// <summary>
    /// 選択されたデッキの表示を更新
    /// </summary>
    private void UpdateSelectedDeckDisplay()
    {
        if (selectedDeckContent == null || selectedCardPrefab == null) return;
        
        // 既存の選択カード表示をクリア
        foreach (Transform child in selectedDeckContent)
        {
            Destroy(child.gameObject);
        }
        
        // 選択されたカードを表示
        foreach (var card in selectedDeck)
        {
            var selectedCard = Instantiate(selectedCardPrefab, selectedDeckContent);
            var selectedCardUI = selectedCard.GetComponent<SelectedCardUI>();
            
            if (selectedCardUI != null)
            {
                selectedCardUI.Setup(card, OnSelectedCardClicked);
            }
        }
        
        // デッキサイズテキストを更新
        if (deckSizeText != null)
        {
            deckSizeText.text = $"デッキ: {selectedDeck.Count}/{maxDeckSize}";
        }
        
        // デッキ統計テキストを更新
        if (deckStatisticsText != null)
        {
            var stats = GetDeckStatistics();
            deckStatisticsText.text = $"攻撃: {stats.attackCardCount} 移動: {stats.moveCardCount} 回復: {stats.healCardCount}";
        }
    }
    
    /// <summary>
    /// UI全体を更新
    /// </summary>
    private void UpdateUI()
    {
        UpdateCardList();
        UpdateSelectedDeckDisplay();
        UpdateButtonStates();
    }
    
    /// <summary>
    /// ボタンの状態を更新
    /// </summary>
    private void UpdateButtonStates()
    {
        bool canAddCard = selectedDeck.Count < maxDeckSize;
        bool canRemoveCard = selectedDeck.Count > 0;
        bool canStartBattle = selectedDeck.Count >= minDeckSize;
        
        if (addCardButton != null)
            addCardButton.interactable = canAddCard;
        
        if (removeCardButton != null)
            removeCardButton.interactable = canRemoveCard;
        
        if (startBattleButton != null)
            startBattleButton.interactable = canStartBattle;
        
        if (clearDeckButton != null)
            clearDeckButton.interactable = selectedDeck.Count > 0;
    }
    
    /// <summary>
    /// フィルタリングされたカードを取得
    /// </summary>
    private List<CardDataSO> GetFilteredCards()
    {
        if (currentFilter == CardType.Attack && allCardsButton != null)
        {
            // 全カード表示の場合
            return availableCards;
        }
        
        return availableCards.Where(card => card.type == currentFilter).ToList();
    }
    
    /// <summary>
    /// カードフィルターを設定
    /// </summary>
    private void SetCardFilter(CardType filterType)
    {
        currentFilter = filterType;
        UpdateCardList();
        Debug.Log($"DeckBuilderUI: カードフィルター設定 - {filterType}");
    }
    
    /// <summary>
    /// カードリストアイテムがクリックされた
    /// </summary>
    private void OnCardListItemClicked(CardDataSO card)
    {
        if (selectedDeck.Count >= maxDeckSize)
        {
            Debug.LogWarning($"DeckBuilderUI: デッキが最大枚数({maxDeckSize}枚)に達しています");
            return;
        }
        
        selectedDeck.Add(card);
        UpdateUI();
        Debug.Log($"DeckBuilderUI: カードをデッキに追加 - {card.cardName}");
    }
    
    /// <summary>
    /// 選択されたカードがクリックされた
    /// </summary>
    private void OnSelectedCardClicked(CardDataSO card)
    {
        selectedDeck.Remove(card);
        UpdateUI();
        Debug.Log($"DeckBuilderUI: カードをデッキから削除 - {card.cardName}");
    }
    
    /// <summary>
    /// カード追加ボタンがクリックされた
    /// </summary>
    private void OnAddCardClicked()
    {
        // 現在選択されているカードを追加（実装はOnCardListItemClickedで処理）
        Debug.Log("DeckBuilderUI: カード追加ボタンがクリックされました");
    }
    
    /// <summary>
    /// カード削除ボタンがクリックされた
    /// </summary>
    private void OnRemoveCardClicked()
    {
        if (selectedDeck.Count > 0)
        {
            selectedDeck.RemoveAt(selectedDeck.Count - 1);
            UpdateUI();
            Debug.Log("DeckBuilderUI: 最後のカードを削除しました");
        }
    }
    
    /// <summary>
    /// デッキクリアボタンがクリックされた
    /// </summary>
    private void OnClearDeckClicked()
    {
        selectedDeck.Clear();
        UpdateUI();
        Debug.Log("DeckBuilderUI: デッキをクリアしました");
    }
    
    /// <summary>
    /// バトル開始ボタンがクリックされた
    /// </summary>
    private void OnStartBattleClicked()
    {
        if (selectedDeck.Count < minDeckSize)
        {
            Debug.LogWarning($"DeckBuilderUI: デッキが最小枚数({minDeckSize}枚)に達していません");
            return;
        }
        
        // デッキを保存してバトルシーンに遷移
        SaveDeckAndStartBattle();
    }
    
    /// <summary>
    /// デッキを保存してバトルを開始
    /// </summary>
    private void SaveDeckAndStartBattle()
    {
        // PlayerDeckにデッキを保存
        var playerDeck = new PlayerDeck();
        playerDeck.InitializeDeck(selectedDeck);
        
        // GameManagerにデッキを設定
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerDeck(playerDeck);
        }
        
        Debug.Log($"DeckBuilderUI: デッキを保存してバトル開始 - {selectedDeck.Count}枚");
        
        // バトルシーンに遷移
        UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
    }
    
    /// <summary>
    /// デッキの統計情報を取得
    /// </summary>
    private DeckStatistics GetDeckStatistics()
    {
        var stats = new DeckStatistics();
        
        foreach (var card in selectedDeck)
        {
            switch (card.type)
            {
                case CardType.Attack:
                    stats.attackCardCount++;
                    stats.totalAttackPower += card.GetEffectivePower();
                    break;
                case CardType.Move:
                    stats.moveCardCount++;
                    stats.totalMoveDistance += card.GetEffectiveMoveDistance();
                    break;
                case CardType.Heal:
                    stats.healCardCount++;
                    stats.totalHealAmount += card.GetEffectiveHealAmount();
                    break;
            }
        }
        
        stats.totalCards = selectedDeck.Count;
        return stats;
    }
} 