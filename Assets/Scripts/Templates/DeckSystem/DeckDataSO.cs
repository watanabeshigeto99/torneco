using UnityEngine;
using System;
using System.Collections.Generic;

namespace DeckSystem
{
    /// <summary>
    /// デッキデータ管理用ScriptableObject
    /// 責務：デッキデータの状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "DeckData", menuName = "Deck System/Deck Data")]
    public class DeckDataSO : ScriptableObject
    {
        [Header("Deck Settings")]
        public string deckName = "Default Deck";
        public int maxDeckSize = 30;
        public int maxHandSize = 7;
        public int startingHandSize = 5;
        
        [Header("Deck Contents")]
        public List<CardDataSO> deckCards = new List<CardDataSO>();
        public List<CardDataSO> handCards = new List<CardDataSO>();
        public List<CardDataSO> discardCards = new List<CardDataSO>();
        
        [Header("Deck Statistics")]
        public int totalCardsPlayed = 0;
        public int totalCardsDrawn = 0;
        public int totalCardsDiscarded = 0;
        public float averageCardCost = 0f;
        
        [Header("Deck State")]
        public bool isShuffled = false;
        public bool isInitialized = false;
        public int currentTurn = 0;
        
        // イベント
        public event Action<DeckDataSO> OnDataChanged;
        public event Action<CardDataSO> OnCardAdded;
        public event Action<CardDataSO> OnCardRemoved;
        public event Action<CardDataSO> OnCardDrawn;
        public event Action<CardDataSO> OnCardPlayed;
        public event Action OnDeckShuffled;
        public event Action OnDeckReset;
        
        /// <summary>
        /// デッキデータの初期化
        /// </summary>
        public void Initialize()
        {
            deckCards.Clear();
            handCards.Clear();
            discardCards.Clear();
            
            totalCardsPlayed = 0;
            totalCardsDrawn = 0;
            totalCardsDiscarded = 0;
            averageCardCost = 0f;
            
            isShuffled = false;
            isInitialized = true;
            currentTurn = 0;
            
            Debug.Log("DeckDataSO: デッキデータを初期化しました");
        }
        
        /// <summary>
        /// カードをデッキに追加
        /// </summary>
        public void AddCard(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (deckCards.Count >= maxDeckSize)
            {
                Debug.LogWarning($"DeckDataSO: デッキが最大サイズに達しています - {deckName}");
                return;
            }
            
            cardData.MoveToDeck();
            deckCards.Add(cardData);
            
            UpdateAverageCardCost();
            
            OnCardAdded?.Invoke(cardData);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"DeckDataSO: カードをデッキに追加しました - {cardData.cardName}");
        }
        
        /// <summary>
        /// カードをデッキから削除
        /// </summary>
        public void RemoveCard(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (deckCards.Remove(cardData))
            {
                cardData.ResetCard();
                UpdateAverageCardCost();
                
                OnCardRemoved?.Invoke(cardData);
                OnDataChanged?.Invoke(this);
                
                Debug.Log($"DeckDataSO: カードをデッキから削除しました - {cardData.cardName}");
            }
        }
        
        /// <summary>
        /// カードを引く
        /// </summary>
        public CardDataSO DrawCard()
        {
            if (deckCards.Count == 0)
            {
                Debug.LogWarning("DeckDataSO: デッキが空です");
                return null;
            }
            
            if (handCards.Count >= maxHandSize)
            {
                Debug.LogWarning("DeckDataSO: 手札が最大サイズに達しています");
                return null;
            }
            
            CardDataSO drawnCard = deckCards[0];
            deckCards.RemoveAt(0);
            
            drawnCard.MoveToHand();
            handCards.Add(drawnCard);
            
            totalCardsDrawn++;
            
            OnCardDrawn?.Invoke(drawnCard);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"DeckDataSO: カードを引きました - {drawnCard.cardName}");
            return drawnCard;
        }
        
        /// <summary>
        /// カードを手札に追加
        /// </summary>
        public void AddCardToHand(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (handCards.Count >= maxHandSize)
            {
                Debug.LogWarning("DeckDataSO: 手札が最大サイズに達しています");
                return;
            }
            
            cardData.MoveToHand();
            handCards.Add(cardData);
            
            OnCardAdded?.Invoke(cardData);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"DeckDataSO: カードを手札に追加しました - {cardData.cardName}");
        }
        
        /// <summary>
        /// カードを手札から削除
        /// </summary>
        public void RemoveCardFromHand(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (handCards.Remove(cardData))
            {
                cardData.MoveToDiscard();
                discardCards.Add(cardData);
                
                totalCardsDiscarded++;
                
                OnCardRemoved?.Invoke(cardData);
                OnDataChanged?.Invoke(this);
                
                Debug.Log($"DeckDataSO: カードを手札から削除しました - {cardData.cardName}");
            }
        }
        
        /// <summary>
        /// カードをプレイ
        /// </summary>
        public void PlayCard(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (handCards.Remove(cardData))
            {
                cardData.SetPlayed();
                discardCards.Add(cardData);
                
                totalCardsPlayed++;
                
                OnCardPlayed?.Invoke(cardData);
                OnDataChanged?.Invoke(this);
                
                Debug.Log($"DeckDataSO: カードをプレイしました - {cardData.cardName}");
            }
        }
        
        /// <summary>
        /// デッキをシャッフル
        /// </summary>
        public void ShuffleDeck()
        {
            if (deckCards.Count == 0) return;
            
            // Fisher-Yates シャッフル
            for (int i = deckCards.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                CardDataSO temp = deckCards[i];
                deckCards[i] = deckCards[j];
                deckCards[j] = temp;
            }
            
            isShuffled = true;
            
            OnDeckShuffled?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log("DeckDataSO: デッキをシャッフルしました");
        }
        
        /// <summary>
        /// デッキをリセット
        /// </summary>
        public void ResetDeck()
        {
            // すべてのカードをデッキに戻す
            foreach (var card in handCards)
            {
                card.MoveToDeck();
                deckCards.Add(card);
            }
            
            foreach (var card in discardCards)
            {
                card.MoveToDeck();
                deckCards.Add(card);
            }
            
            handCards.Clear();
            discardCards.Clear();
            
            isShuffled = false;
            currentTurn = 0;
            
            UpdateAverageCardCost();
            
            OnDeckReset?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log("DeckDataSO: デッキをリセットしました");
        }
        
        /// <summary>
        /// 平均カードコストを更新
        /// </summary>
        private void UpdateAverageCardCost()
        {
            if (deckCards.Count == 0)
            {
                averageCardCost = 0f;
                return;
            }
            
            int totalCost = 0;
            foreach (var card in deckCards)
            {
                totalCost += card.cardCost;
            }
            
            averageCardCost = (float)totalCost / deckCards.Count;
        }
        
        /// <summary>
        /// デッキサイズを取得
        /// </summary>
        public int GetDeckSize()
        {
            return deckCards.Count;
        }
        
        /// <summary>
        /// 手札サイズを取得
        /// </summary>
        public int GetHandSize()
        {
            return handCards.Count;
        }
        
        /// <summary>
        /// 捨て札サイズを取得
        /// </summary>
        public int GetDiscardSize()
        {
            return discardCards.Count;
        }
        
        /// <summary>
        /// デッキが空かチェック
        /// </summary>
        public bool IsDeckEmpty()
        {
            return deckCards.Count == 0;
        }
        
        /// <summary>
        /// 手札が空かチェック
        /// </summary>
        public bool IsHandEmpty()
        {
            return handCards.Count == 0;
        }
        
        /// <summary>
        /// 手札が最大サイズかチェック
        /// </summary>
        public bool IsHandFull()
        {
            return handCards.Count >= maxHandSize;
        }
        
        /// <summary>
        /// デッキが最大サイズかチェック
        /// </summary>
        public bool IsDeckFull()
        {
            return deckCards.Count >= maxDeckSize;
        }
        
        /// <summary>
        /// デッキの情報を取得
        /// </summary>
        public string GetDeckInfo()
        {
            return $"Deck: {deckName}, Size: {GetDeckSize()}/{maxDeckSize}, " +
                   $"Hand: {GetHandSize()}/{maxHandSize}, Discard: {GetDiscardSize()}, " +
                   $"Shuffled: {isShuffled}";
        }
        
        /// <summary>
        /// デッキの詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== Deck Data ===\n" +
                   $"Name: {deckName}\n" +
                   $"Max Deck Size: {maxDeckSize}\n" +
                   $"Max Hand Size: {maxHandSize}\n" +
                   $"Starting Hand Size: {startingHandSize}\n" +
                   $"Deck Size: {GetDeckSize()}/{maxDeckSize}\n" +
                   $"Hand Size: {GetHandSize()}/{maxHandSize}\n" +
                   $"Discard Size: {GetDiscardSize()}\n" +
                   $"Total Cards Played: {totalCardsPlayed}\n" +
                   $"Total Cards Drawn: {totalCardsDrawn}\n" +
                   $"Total Cards Discarded: {totalCardsDiscarded}\n" +
                   $"Average Card Cost: {averageCardCost:F1}\n" +
                   $"Is Shuffled: {isShuffled}\n" +
                   $"Is Initialized: {isInitialized}\n" +
                   $"Current Turn: {currentTurn}";
        }
    }
} 