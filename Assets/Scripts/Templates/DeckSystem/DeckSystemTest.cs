using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DeckSystem
{
    /// <summary>
    /// デッキシステムテンプレートのテスト用コンポーネント
    /// 責務：デッキシステムのテストのみ
    /// </summary>
    public class DeckSystemTest : MonoBehaviour
    {
        [Header("Deck System Components")]
        public DeckManager deckManager;
        public DeckDataSO deckData;
        public DeckEventChannel deckEventChannel;
        
        [Header("Test UI")]
        public Button addCardButton;
        public Button removeCardButton;
        public Button drawCardButton;
        public Button shuffleDeckButton;
        public Button resetDeckButton;
        public Button upgradeCardButton;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI eventLogText;
        
        [Header("Test Cards")]
        public CardDataSO[] testCards;
        
        private string eventLog = "";
        private int maxEventLogLines = 10;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            if (addCardButton != null)
                addCardButton.onClick.AddListener(OnAddCardButtonClicked);
            
            if (removeCardButton != null)
                removeCardButton.onClick.AddListener(OnRemoveCardButtonClicked);
            
            if (drawCardButton != null)
                drawCardButton.onClick.AddListener(OnDrawCardButtonClicked);
            
            if (shuffleDeckButton != null)
                shuffleDeckButton.onClick.AddListener(OnShuffleDeckButtonClicked);
            
            if (resetDeckButton != null)
                resetDeckButton.onClick.AddListener(OnResetDeckButtonClicked);
            
            if (upgradeCardButton != null)
                upgradeCardButton.onClick.AddListener(OnUpgradeCardButtonClicked);
            
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (deckEventChannel != null)
            {
                deckEventChannel.OnDeckChanged.AddListener(OnDeckChanged);
                deckEventChannel.OnCardAdded.AddListener(OnCardAdded);
                deckEventChannel.OnCardRemoved.AddListener(OnCardRemoved);
                deckEventChannel.OnCardDrawn.AddListener(OnCardDrawn);
                deckEventChannel.OnCardPlayed.AddListener(OnCardPlayed);
                deckEventChannel.OnDeckShuffled.AddListener(OnDeckShuffled);
                deckEventChannel.OnDeckReset.AddListener(OnDeckReset);
                deckEventChannel.OnCardAddedToHand.AddListener(OnCardAddedToHand);
                deckEventChannel.OnCardRemovedFromHand.AddListener(OnCardRemovedFromHand);
                deckEventChannel.OnHandFull.AddListener(OnHandFull);
                deckEventChannel.OnHandEmpty.AddListener(OnHandEmpty);
                deckEventChannel.OnDeckSizeChanged.AddListener(OnDeckSizeChanged);
                deckEventChannel.OnHandSizeChanged.AddListener(OnHandSizeChanged);
                deckEventChannel.OnDiscardSizeChanged.AddListener(OnDiscardSizeChanged);
                deckEventChannel.OnAverageCardCostChanged.AddListener(OnAverageCardCostChanged);
                deckEventChannel.OnCardStateChanged.AddListener(OnCardStateChanged);
                deckEventChannel.OnCardUpgraded.AddListener(OnCardUpgraded);
                deckEventChannel.OnCardCostChanged.AddListener(OnCardCostChanged);
            }
        }
        
        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (deckEventChannel != null)
            {
                deckEventChannel.OnDeckChanged.RemoveListener(OnDeckChanged);
                deckEventChannel.OnCardAdded.RemoveListener(OnCardAdded);
                deckEventChannel.OnCardRemoved.RemoveListener(OnCardRemoved);
                deckEventChannel.OnCardDrawn.RemoveListener(OnCardDrawn);
                deckEventChannel.OnCardPlayed.RemoveListener(OnCardPlayed);
                deckEventChannel.OnDeckShuffled.RemoveListener(OnDeckShuffled);
                deckEventChannel.OnDeckReset.RemoveListener(OnDeckReset);
                deckEventChannel.OnCardAddedToHand.RemoveListener(OnCardAddedToHand);
                deckEventChannel.OnCardRemovedFromHand.RemoveListener(OnCardRemovedFromHand);
                deckEventChannel.OnHandFull.RemoveListener(OnHandFull);
                deckEventChannel.OnHandEmpty.RemoveListener(OnHandEmpty);
                deckEventChannel.OnDeckSizeChanged.RemoveListener(OnDeckSizeChanged);
                deckEventChannel.OnHandSizeChanged.RemoveListener(OnHandSizeChanged);
                deckEventChannel.OnDiscardSizeChanged.RemoveListener(OnDiscardSizeChanged);
                deckEventChannel.OnAverageCardCostChanged.RemoveListener(OnAverageCardCostChanged);
                deckEventChannel.OnCardStateChanged.RemoveListener(OnCardStateChanged);
                deckEventChannel.OnCardUpgraded.RemoveListener(OnCardUpgraded);
                deckEventChannel.OnCardCostChanged.RemoveListener(OnCardCostChanged);
            }
        }
        
        // UI Button Event Handlers
        
        /// <summary>
        /// カード追加ボタンクリック
        /// </summary>
        private void OnAddCardButtonClicked()
        {
            if (deckManager != null && testCards != null && testCards.Length > 0)
            {
                CardDataSO randomCard = testCards[Random.Range(0, testCards.Length)];
                deckManager.AddCardToDeck(randomCard);
                AddToEventLog($"カード追加ボタンクリック: {randomCard.cardName}");
            }
        }
        
        /// <summary>
        /// カード削除ボタンクリック
        /// </summary>
        private void OnRemoveCardButtonClicked()
        {
            if (deckManager != null && deckData != null && deckData.deckCards.Count > 0)
            {
                CardDataSO cardToRemove = deckData.deckCards[0];
                deckManager.RemoveCardFromDeck(cardToRemove);
                AddToEventLog($"カード削除ボタンクリック: {cardToRemove.cardName}");
            }
        }
        
        /// <summary>
        /// カードドローボタンクリック
        /// </summary>
        private void OnDrawCardButtonClicked()
        {
            if (deckManager != null)
            {
                CardDataSO drawnCard = deckManager.DrawCard();
                if (drawnCard != null)
                {
                    AddToEventLog($"カードドローボタンクリック: {drawnCard.cardName}");
                }
            }
        }
        
        /// <summary>
        /// デッキシャッフルボタンクリック
        /// </summary>
        private void OnShuffleDeckButtonClicked()
        {
            if (deckManager != null)
            {
                deckManager.ShuffleDeck();
                AddToEventLog("デッキシャッフルボタンクリック");
            }
        }
        
        /// <summary>
        /// デッキリセットボタンクリック
        /// </summary>
        private void OnResetDeckButtonClicked()
        {
            if (deckManager != null)
            {
                deckManager.ResetDeck();
                AddToEventLog("デッキリセットボタンクリック");
            }
        }
        
        /// <summary>
        /// カードアップグレードボタンクリック
        /// </summary>
        private void OnUpgradeCardButtonClicked()
        {
            if (deckData != null && deckData.handCards.Count > 0)
            {
                CardDataSO cardToUpgrade = deckData.handCards[0];
                bool upgraded = cardToUpgrade.UpgradeCard();
                if (upgraded)
                {
                    AddToEventLog($"カードアップグレードボタンクリック: {cardToUpgrade.cardName}");
                }
            }
        }
        
        // Event Handlers
        
        private void OnDeckChanged(DeckDataSO deckData)
        {
            AddToEventLog($"デッキ変更: {deckData?.deckName ?? "Unknown"}");
            UpdateStatusDisplay();
        }
        
        private void OnCardAdded(CardDataSO cardData)
        {
            AddToEventLog($"カード追加: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardRemoved(CardDataSO cardData)
        {
            AddToEventLog($"カード削除: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardDrawn(CardDataSO cardData)
        {
            AddToEventLog($"カードドロー: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardPlayed(CardDataSO cardData)
        {
            AddToEventLog($"カードプレイ: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnDeckShuffled()
        {
            AddToEventLog("デッキシャッフル");
        }
        
        private void OnDeckReset()
        {
            AddToEventLog("デッキリセット");
        }
        
        private void OnCardAddedToHand(CardDataSO cardData)
        {
            AddToEventLog($"手札追加: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardRemovedFromHand(CardDataSO cardData)
        {
            AddToEventLog($"手札削除: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnHandFull()
        {
            AddToEventLog("手札満杯");
        }
        
        private void OnHandEmpty()
        {
            AddToEventLog("手札空");
        }
        
        private void OnDeckSizeChanged(int newSize)
        {
            AddToEventLog($"デッキサイズ変更: {newSize}");
        }
        
        private void OnHandSizeChanged(int newSize)
        {
            AddToEventLog($"手札サイズ変更: {newSize}");
        }
        
        private void OnDiscardSizeChanged(int newSize)
        {
            AddToEventLog($"捨て札サイズ変更: {newSize}");
        }
        
        private void OnAverageCardCostChanged(float newCost)
        {
            AddToEventLog($"平均コスト変更: {newCost:F1}");
        }
        
        private void OnCardStateChanged(CardDataSO cardData)
        {
            AddToEventLog($"カード状態変更: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardUpgraded(CardDataSO cardData)
        {
            AddToEventLog($"カードアップグレード: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardCostChanged(CardDataSO cardData)
        {
            AddToEventLog($"カードコスト変更: {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// イベントログに追加
        /// </summary>
        private void AddToEventLog(string message)
        {
            eventLog = $"[{System.DateTime.Now:HH:mm:ss}] {message}\n{eventLog}";
            
            // 最大行数を制限
            string[] lines = eventLog.Split('\n');
            if (lines.Length > maxEventLogLines)
            {
                eventLog = string.Join("\n", lines, 0, maxEventLogLines);
            }
            
            if (eventLogText != null)
            {
                eventLogText.text = eventLog;
            }
        }
        
        /// <summary>
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText != null && deckData != null)
            {
                statusText.text = $"=== Deck System Test ===\n" +
                                 $"Deck: {deckData.GetDeckInfo()}\n" +
                                 $"Manager: {(deckManager != null ? "✓" : "✗")}\n" +
                                 $"EventChannel: {(deckEventChannel != null ? "✓" : "✗")}\n" +
                                 $"Test Cards: {(testCards != null ? testCards.Length : 0)}";
            }
        }
        
        /// <summary>
        /// デッキシステムテストの情報を取得
        /// </summary>
        public string GetDeckSystemTestInfo()
        {
            return $"DeckSystemTest - Manager: {(deckManager != null ? "✓" : "✗")}, " +
                   $"Data: {(deckData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(deckEventChannel != null ? "✓" : "✗")}, " +
                   $"TestCards: {(testCards != null ? testCards.Length : 0)}";
        }
    }
} 