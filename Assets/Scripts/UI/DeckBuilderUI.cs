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
    
    [Header("Floor Display")]
    public TextMeshProUGUI floorInfoText;
    
    [Header("Audio")]
    public string bgmName = "DeckBuilderBGM";
    public string buttonClickSE = "ButtonClick";
    public string cardSelectSE = "card_hover";
    public string cardAddSE = "アイテムを入手2";
    public string cardRemoveSE = "Error";
    
    private List<CardDataSO> availableCards = new List<CardDataSO>();
    private List<CardDataSO> selectedDeck = new List<CardDataSO>();
    private CardType currentFilter = CardType.Attack;
    
    private void Start()
    {
        // GameManagerを確実に初期化
        GameManager.GetOrCreateInstance();
        
        // 遷移後の初期化が完了するまで待機
        if (TransitionManager.Instance != null && TransitionManager.Instance.IsTransitioning)
        {
            Debug.Log("DeckBuilderUI: 遷移中なので初期化を遅延します");
            return;
        }
        
        // 通常の初期化を実行
        InitializeAfterTransition();
    }
    
    private void OnEnable()
    {
        // TransitionManagerのイベントを購読
        TransitionManager.OnSceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        // TransitionManagerのイベントを購読解除
        TransitionManager.OnSceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(string sceneName)
    {
        if (sceneName == "DeckBuilderScene")
        {
            Debug.Log("DeckBuilderUI: シーンロード完了イベントを受信");
            // 少し遅延してから初期化を実行
            StartCoroutine(DelayedInitialization());
        }
    }
    
    private System.Collections.IEnumerator DelayedInitialization()
    {
        // 2フレーム待機
        yield return null;
        yield return null;
        
        Debug.Log("DeckBuilderUI: 遅延初期化を実行");
        InitializeAfterTransition();
    }
    
    /// <summary>
    /// 遷移後の初期化処理
    /// </summary>
    public void InitializeAfterTransition()
    {
        Debug.Log("DeckBuilderUI: 遷移後の初期化開始");
        
        InitializeUI();
        LoadAvailableCards();
        LoadExistingDeck();
        UpdateUI();
        
        // BGMを再生
        PlayBGM();
        
        Debug.Log("DeckBuilderUI: 遷移後の初期化完了");
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
        

    }
    
    /// <summary>
    /// 利用可能なカードを読み込み
    /// </summary>
    private void LoadAvailableCards()
    {
        if (cardDatabase != null)
        {
            availableCards = cardDatabase.GetAllCards();
            Debug.Log($"DeckBuilderUI: 利用可能なカードを読み込み - {availableCards.Count}枚");
        }
        else
        {
            Debug.LogError("DeckBuilderUI: CardDatabaseが設定されていません");
        }
    }
    
    /// <summary>
    /// 既存のデッキを読み込み
    /// </summary>
    private void LoadExistingDeck()
    {
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null && gameManager.GetPlayerDeck() != null)
        {
            var existingDeck = gameManager.GetPlayerDeck();
            if (existingDeck.selectedDeck != null && existingDeck.selectedDeck.Count > 0)
            {
                selectedDeck = new List<CardDataSO>(existingDeck.selectedDeck);
                Debug.Log($"DeckBuilderUI: 既存のデッキを読み込み - {selectedDeck.Count}枚");
                
                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.AddLog($"既存のデッキを読み込みました ({selectedDeck.Count}枚)");
                }
            }
        }
    }
    
    /// <summary>
    /// カードリストを更新
    /// </summary>
    private void UpdateCardList()
    {
        if (cardListContent == null)
        {
            Debug.LogError("DeckBuilderUI: cardListContentが設定されていません");
            return;
        }
        
        if (cardListItemPrefab == null)
        {
            Debug.LogError("DeckBuilderUI: cardListItemPrefabが設定されていません");
            return;
        }
        
        // 既存のカードリストアイテムをクリア
        foreach (Transform child in cardListContent)
        {
            Destroy(child.gameObject);
        }
        
        // フィルタリングされたカードを取得
        var filteredCards = GetFilteredCards();
        Debug.Log($"DeckBuilderUI: カードリストを更新 - フィルタリングされたカード: {filteredCards.Count}枚");
        
        // カードリストアイテムを作成
        foreach (var card in filteredCards)
        {
            if (card == null)
            {
                Debug.LogWarning("DeckBuilderUI: カードデータがnullです");
                continue;
            }
            
            var cardItem = Instantiate(cardListItemPrefab, cardListContent);
            var cardItemUI = cardItem.GetComponent<CardListItemUI>();
            
            if (cardItemUI != null)
            {
                cardItemUI.Setup(card, OnCardListItemClicked);
                Debug.Log($"DeckBuilderUI: カードアイテムを作成 - {card.cardName}");
            }
            else
            {
                Debug.LogError("DeckBuilderUI: CardListItemUIコンポーネントが見つかりません");
            }
        }
        
        Debug.Log($"DeckBuilderUI: カードリスト更新完了 - 作成されたアイテム数: {cardListContent.childCount}");
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
        UpdateFloorInfo();
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
        Debug.Log($"DeckBuilderUI: フィルタリング実行 - 利用可能カード: {availableCards.Count}枚, 現在のフィルター: {currentFilter}");
        
        if (currentFilter == CardType.Attack && allCardsButton != null)
        {
            // 全カード表示の場合
            Debug.Log($"DeckBuilderUI: 全カード表示 - {availableCards.Count}枚");
            return availableCards;
        }
        
        var filteredCards = availableCards.Where(card => card.type == currentFilter).ToList();
        Debug.Log($"DeckBuilderUI: フィルタリング結果 - {filteredCards.Count}枚 ({currentFilter}タイプ)");
        return filteredCards;
    }
    
    /// <summary>
    /// カードフィルターを設定
    /// </summary>
    private void SetCardFilter(CardType filterType)
    {
        currentFilter = filterType;
        PlayButtonClickSE(); // ボタンクリックSE
        UpdateCardList();

    }
    
    /// <summary>
    /// カードリストアイテムがクリックされた
    /// </summary>
    private void OnCardListItemClicked(CardDataSO card)
    {
        if (selectedDeck.Count >= maxDeckSize)
        {
            Debug.LogWarning($"DeckBuilderUI: デッキが最大枚数({maxDeckSize}枚)に達しています");
            PlayCardRemoveSE(); // エラー音
            return;
        }
        
        selectedDeck.Add(card);
        PlayCardAddSE(); // カード追加SE
        UpdateUI();

    }
    
    /// <summary>
    /// 選択されたカードがクリックされた
    /// </summary>
    private void OnSelectedCardClicked(CardDataSO card)
    {
        selectedDeck.Remove(card);
        PlayCardRemoveSE(); // カード削除SE
        UpdateUI();

    }
    
    /// <summary>
    /// カード追加ボタンがクリックされた
    /// </summary>
    private void OnAddCardClicked()
    {
        // 現在選択されているカードを追加（実装はOnCardListItemClickedで処理）
    }
    
    /// <summary>
    /// カード削除ボタンがクリックされた
    /// </summary>
    private void OnRemoveCardClicked()
    {
        if (selectedDeck.Count > 0)
        {
            selectedDeck.RemoveAt(selectedDeck.Count - 1);
            PlayCardRemoveSE(); // カード削除SE
            UpdateUI();

        }
        PlayButtonClickSE(); // ボタンクリックSE
    }
    
    /// <summary>
    /// デッキクリアボタンがクリックされた
    /// </summary>
    private void OnClearDeckClicked()
    {
        selectedDeck.Clear();
        PlayCardRemoveSE(); // カード削除SE
        UpdateUI();
        PlayButtonClickSE(); // ボタンクリックSE

    }
    
    /// <summary>
    /// バトル開始ボタンがクリックされた
    /// </summary>
    private void OnStartBattleClicked()
    {
        if (selectedDeck.Count < minDeckSize)
        {
            Debug.LogWarning($"DeckBuilderUI: デッキが最小枚数({minDeckSize}枚)に達していません");
            PlayCardRemoveSE(); // エラー音
            return;
        }
        
        PlayButtonClickSE(); // ボタンクリックSE
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
        
        Debug.Log($"DeckBuilderUI: デッキを初期化 - {selectedDeck.Count}枚");
        
        // GameManagerにデッキを設定
        GameManager gameManager = GameManager.GetOrCreateInstance();
        if (gameManager != null)
        {
            gameManager.SetPlayerDeck(playerDeck);
            Debug.Log($"DeckBuilderUI: GameManagerにデッキを設定完了 - {playerDeck.selectedDeck.Count}枚");
        }
        else
        {
            Debug.LogError("DeckBuilderUI: GameManagerの作成に失敗しました");
        }
        
        // バトルシーンに遷移（TransitionManagerを使用）
        if (TransitionManager.Instance != null)
        {
            TransitionManager.Instance.LoadSceneWithFade("MainScene");
        }
        else
        {
            Debug.LogError("DeckBuilderUI: TransitionManagerが見つかりません");
            UnityEngine.SceneManagement.SceneManager.LoadScene(1); // フォールバック
        }
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
    
    /// <summary>
    /// 階層情報を更新
    /// </summary>
    private void UpdateFloorInfo()
    {
        if (floorInfoText != null)
        {
            GameManager gameManager = GameManager.GetOrCreateInstance();
            if (gameManager != null)
            {
                floorInfoText.text = $"階層 {gameManager.currentFloor}";
            }
            else
            {
                floorInfoText.text = "階層 1";
            }
        }
    }
    
    /// <summary>
    /// BGMを再生
    /// </summary>
    private void PlayBGM()
    {
        if (SoundManager.Instance != null)
        {
            // 現在のシーンに応じたBGMを再生
            SoundManager.Instance.PlayBGMForCurrentScene();
        }
    }
    
    /// <summary>
    /// ボタンクリックSEを再生
    /// </summary>
    private void PlayButtonClickSE()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(buttonClickSE);
        }
    }
    
    /// <summary>
    /// カード選択SEを再生
    /// </summary>
    private void PlayCardSelectSE()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(cardSelectSE);
        }
    }
    
    /// <summary>
    /// カード追加SEを再生
    /// </summary>
    private void PlayCardAddSE()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(cardAddSE);
        }
    }
    
    /// <summary>
    /// カード削除SEを再生
    /// </summary>
    private void PlayCardRemoveSE()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(cardRemoveSE);
        }
    }
    
} 