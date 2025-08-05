using UnityEngine;
using System;
using System.Collections.Generic;

namespace DeckSystem
{
    /// <summary>
    /// デッキシステム管理専用コンポーネント
    /// 責務：デッキシステムの管理のみ
    /// </summary>
    [DefaultExecutionOrder(-250)]
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }
        
        [Header("Deck System Settings")]
        public DeckDataSO deckData;
        public DeckEventChannel deckEventChannel;
        
        // デッキシステム変更イベント
        public static event Action<DeckDataSO> OnDeckChanged;
        public static event Action<CardDataSO> OnCardAdded;
        public static event Action<CardDataSO> OnCardRemoved;
        public static event Action OnDeckShuffled;
        public static event Action OnDeckReset;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeDeckSystem();
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        /// <summary>
        /// デッキシステムの初期化
        /// </summary>
        private void InitializeDeckSystem()
        {
            if (deckData == null)
            {
                Debug.LogError("DeckManager: deckDataが設定されていません");
                return;
            }
            
            deckData.Initialize();
            Debug.Log("DeckManager: デッキシステムを初期化しました");
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (deckEventChannel != null)
            {
                deckEventChannel.OnDeckChanged.AddListener(OnDeckChangedHandler);
                deckEventChannel.OnCardAdded.AddListener(OnCardAddedHandler);
                deckEventChannel.OnCardRemoved.AddListener(OnCardRemovedHandler);
                deckEventChannel.OnDeckShuffled.AddListener(OnDeckShuffledHandler);
                deckEventChannel.OnDeckReset.AddListener(OnDeckResetHandler);
            }
        }
        
        /// <summary>
        /// デッキ変更ハンドラー
        /// </summary>
        private void OnDeckChangedHandler(DeckDataSO deckData)
        {
            OnDeckChanged?.Invoke(deckData);
            Debug.Log("DeckManager: デッキが変更されました");
        }
        
        /// <summary>
        /// カード追加ハンドラー
        /// </summary>
        private void OnCardAddedHandler(CardDataSO cardData)
        {
            OnCardAdded?.Invoke(cardData);
            Debug.Log($"DeckManager: カードが追加されました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カード削除ハンドラー
        /// </summary>
        private void OnCardRemovedHandler(CardDataSO cardData)
        {
            OnCardRemoved?.Invoke(cardData);
            Debug.Log($"DeckManager: カードが削除されました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// デッキシャッフルハンドラー
        /// </summary>
        private void OnDeckShuffledHandler()
        {
            OnDeckShuffled?.Invoke();
            Debug.Log("DeckManager: デッキがシャッフルされました");
        }
        
        /// <summary>
        /// デッキリセットハンドラー
        /// </summary>
        private void OnDeckResetHandler()
        {
            OnDeckReset?.Invoke();
            Debug.Log("DeckManager: デッキがリセットされました");
        }
        
        /// <summary>
        /// カードをデッキに追加
        /// </summary>
        public void AddCardToDeck(CardDataSO cardData)
        {
            if (deckData == null) return;
            
            deckData.AddCard(cardData);
            
            if (deckEventChannel != null)
            {
                deckEventChannel.RaiseCardAdded(cardData);
                deckEventChannel.RaiseDeckChanged(deckData);
            }
        }
        
        /// <summary>
        /// カードをデッキから削除
        /// </summary>
        public void RemoveCardFromDeck(CardDataSO cardData)
        {
            if (deckData == null) return;
            
            deckData.RemoveCard(cardData);
            
            if (deckEventChannel != null)
            {
                deckEventChannel.RaiseCardRemoved(cardData);
                deckEventChannel.RaiseDeckChanged(deckData);
            }
        }
        
        /// <summary>
        /// デッキをシャッフル
        /// </summary>
        public void ShuffleDeck()
        {
            if (deckData == null) return;
            
            deckData.ShuffleDeck();
            
            if (deckEventChannel != null)
            {
                deckEventChannel.RaiseDeckShuffled();
                deckEventChannel.RaiseDeckChanged(deckData);
            }
        }
        
        /// <summary>
        /// デッキをリセット
        /// </summary>
        public void ResetDeck()
        {
            if (deckData == null) return;
            
            deckData.ResetDeck();
            
            if (deckEventChannel != null)
            {
                deckEventChannel.RaiseDeckReset();
                deckEventChannel.RaiseDeckChanged(deckData);
            }
        }
        
        /// <summary>
        /// カードを引く
        /// </summary>
        public CardDataSO DrawCard()
        {
            if (deckData == null) return null;
            
            CardDataSO drawnCard = deckData.DrawCard();
            
            if (drawnCard != null && deckEventChannel != null)
            {
                deckEventChannel.RaiseCardDrawn(drawnCard);
                deckEventChannel.RaiseDeckChanged(deckData);
            }
            
            return drawnCard;
        }
        
        /// <summary>
        /// カードを手札に追加
        /// </summary>
        public void AddCardToHand(CardDataSO cardData)
        {
            if (deckData == null) return;
            
            deckData.AddCardToHand(cardData);
            
            if (deckEventChannel != null)
            {
                deckEventChannel.RaiseCardAddedToHand(cardData);
                deckEventChannel.RaiseDeckChanged(deckData);
            }
        }
        
        /// <summary>
        /// カードを手札から削除
        /// </summary>
        public void RemoveCardFromHand(CardDataSO cardData)
        {
            if (deckData == null) return;
            
            deckData.RemoveCardFromHand(cardData);
            
            if (deckEventChannel != null)
            {
                deckEventChannel.RaiseCardRemovedFromHand(cardData);
                deckEventChannel.RaiseDeckChanged(deckData);
            }
        }
        
        /// <summary>
        /// デッキデータを取得
        /// </summary>
        public DeckDataSO GetDeckData()
        {
            return deckData;
        }
        
        /// <summary>
        /// デッキサイズを取得
        /// </summary>
        public int GetDeckSize()
        {
            return deckData?.GetDeckSize() ?? 0;
        }
        
        /// <summary>
        /// 手札サイズを取得
        /// </summary>
        public int GetHandSize()
        {
            return deckData?.GetHandSize() ?? 0;
        }
        
        /// <summary>
        /// デッキが空かチェック
        /// </summary>
        public bool IsDeckEmpty()
        {
            return deckData?.IsDeckEmpty() ?? true;
        }
        
        /// <summary>
        /// 手札が空かチェック
        /// </summary>
        public bool IsHandEmpty()
        {
            return deckData?.IsHandEmpty() ?? true;
        }
        
        /// <summary>
        /// デッキシステムの情報を取得
        /// </summary>
        public string GetDeckSystemInfo()
        {
            return $"DeckManager - Data: {(deckData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(deckEventChannel != null ? "✓" : "✗")}";
        }
    }
} 