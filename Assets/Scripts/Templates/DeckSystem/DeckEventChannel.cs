using UnityEngine;
using UnityEngine.Events;

namespace DeckSystem
{
    /// <summary>
    /// デッキシステムイベント管理用ScriptableObject
    /// 責務：デッキシステムのイベント管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "DeckEventChannel", menuName = "Deck System/Deck Event Channel")]
    public class DeckEventChannel : ScriptableObject
    {
        [Header("Deck Events")]
        public UnityEvent<DeckDataSO> OnDeckChanged;
        public UnityEvent<CardDataSO> OnCardAdded;
        public UnityEvent<CardDataSO> OnCardRemoved;
        public UnityEvent<CardDataSO> OnCardDrawn;
        public UnityEvent<CardDataSO> OnCardPlayed;
        public UnityEvent OnDeckShuffled;
        public UnityEvent OnDeckReset;
        
        [Header("Hand Events")]
        public UnityEvent<CardDataSO> OnCardAddedToHand;
        public UnityEvent<CardDataSO> OnCardRemovedFromHand;
        public UnityEvent OnHandFull;
        public UnityEvent OnHandEmpty;
        
        [Header("Deck Statistics Events")]
        public UnityEvent<int> OnDeckSizeChanged;
        public UnityEvent<int> OnHandSizeChanged;
        public UnityEvent<int> OnDiscardSizeChanged;
        public UnityEvent<float> OnAverageCardCostChanged;
        
        [Header("Card State Events")]
        public UnityEvent<CardDataSO> OnCardStateChanged;
        public UnityEvent<CardDataSO> OnCardUpgraded;
        public UnityEvent<CardDataSO> OnCardCostChanged;
        
        /// <summary>
        /// デッキ変更イベントを発生
        /// </summary>
        public void RaiseDeckChanged(DeckDataSO deckData)
        {
            OnDeckChanged?.Invoke(deckData);
            Debug.Log($"DeckEventChannel: デッキ変更イベントを発生しました - {deckData?.deckName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カード追加イベントを発生
        /// </summary>
        public void RaiseCardAdded(CardDataSO cardData)
        {
            OnCardAdded?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カード追加イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カード削除イベントを発生
        /// </summary>
        public void RaiseCardRemoved(CardDataSO cardData)
        {
            OnCardRemoved?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カード削除イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カードドローイベントを発生
        /// </summary>
        public void RaiseCardDrawn(CardDataSO cardData)
        {
            OnCardDrawn?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カードドローイベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カードプレイイベントを発生
        /// </summary>
        public void RaiseCardPlayed(CardDataSO cardData)
        {
            OnCardPlayed?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カードプレイイベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// デッキシャッフルイベントを発生
        /// </summary>
        public void RaiseDeckShuffled()
        {
            OnDeckShuffled?.Invoke();
            Debug.Log("DeckEventChannel: デッキシャッフルイベントを発生しました");
        }
        
        /// <summary>
        /// デッキリセットイベントを発生
        /// </summary>
        public void RaiseDeckReset()
        {
            OnDeckReset?.Invoke();
            Debug.Log("DeckEventChannel: デッキリセットイベントを発生しました");
        }
        
        /// <summary>
        /// カード手札追加イベントを発生
        /// </summary>
        public void RaiseCardAddedToHand(CardDataSO cardData)
        {
            OnCardAddedToHand?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カード手札追加イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カード手札削除イベントを発生
        /// </summary>
        public void RaiseCardRemovedFromHand(CardDataSO cardData)
        {
            OnCardRemovedFromHand?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カード手札削除イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// 手札満杯イベントを発生
        /// </summary>
        public void RaiseHandFull()
        {
            OnHandFull?.Invoke();
            Debug.Log("DeckEventChannel: 手札満杯イベントを発生しました");
        }
        
        /// <summary>
        /// 手札空イベントを発生
        /// </summary>
        public void RaiseHandEmpty()
        {
            OnHandEmpty?.Invoke();
            Debug.Log("DeckEventChannel: 手札空イベントを発生しました");
        }
        
        /// <summary>
        /// デッキサイズ変更イベントを発生
        /// </summary>
        public void RaiseDeckSizeChanged(int newSize)
        {
            OnDeckSizeChanged?.Invoke(newSize);
            Debug.Log($"DeckEventChannel: デッキサイズ変更イベントを発生しました - {newSize}");
        }
        
        /// <summary>
        /// 手札サイズ変更イベントを発生
        /// </summary>
        public void RaiseHandSizeChanged(int newSize)
        {
            OnHandSizeChanged?.Invoke(newSize);
            Debug.Log($"DeckEventChannel: 手札サイズ変更イベントを発生しました - {newSize}");
        }
        
        /// <summary>
        /// 捨て札サイズ変更イベントを発生
        /// </summary>
        public void RaiseDiscardSizeChanged(int newSize)
        {
            OnDiscardSizeChanged?.Invoke(newSize);
            Debug.Log($"DeckEventChannel: 捨て札サイズ変更イベントを発生しました - {newSize}");
        }
        
        /// <summary>
        /// 平均カードコスト変更イベントを発生
        /// </summary>
        public void RaiseAverageCardCostChanged(float newCost)
        {
            OnAverageCardCostChanged?.Invoke(newCost);
            Debug.Log($"DeckEventChannel: 平均カードコスト変更イベントを発生しました - {newCost:F1}");
        }
        
        /// <summary>
        /// カード状態変更イベントを発生
        /// </summary>
        public void RaiseCardStateChanged(CardDataSO cardData)
        {
            OnCardStateChanged?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カード状態変更イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カードアップグレードイベントを発生
        /// </summary>
        public void RaiseCardUpgraded(CardDataSO cardData)
        {
            OnCardUpgraded?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カードアップグレードイベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カードコスト変更イベントを発生
        /// </summary>
        public void RaiseCardCostChanged(CardDataSO cardData)
        {
            OnCardCostChanged?.Invoke(cardData);
            Debug.Log($"DeckEventChannel: カードコスト変更イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// デッキシステムの情報を取得
        /// </summary>
        public string GetDeckEventChannelInfo()
        {
            return $"DeckEventChannel - DeckChanged: {(OnDeckChanged != null ? "✓" : "✗")}, " +
                   $"CardAdded: {(OnCardAdded != null ? "✓" : "✗")}, " +
                   $"CardRemoved: {(OnCardRemoved != null ? "✓" : "✗")}, " +
                   $"CardDrawn: {(OnCardDrawn != null ? "✓" : "✗")}, " +
                   $"CardPlayed: {(OnCardPlayed != null ? "✓" : "✗")}, " +
                   $"DeckShuffled: {(OnDeckShuffled != null ? "✓" : "✗")}, " +
                   $"DeckReset: {(OnDeckReset != null ? "✓" : "✗")}";
        }
    }
} 