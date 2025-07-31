using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class PlayerDeck
{
    [Header("Deck Data")]
    public List<CardDataSO> selectedDeck = new List<CardDataSO>();
    public Queue<CardDataSO> drawPile = new Queue<CardDataSO>();
    public List<CardDataSO> discardPile = new List<CardDataSO>();
    
    [Header("Settings")]
    public int maxDeckSize = 10;
    public int minDeckSize = 5;
    
    /// <summary>
    /// デッキを初期化
    /// </summary>
    public void InitializeDeck(List<CardDataSO> cards)
    {
        selectedDeck = new List<CardDataSO>(cards);
        InitializeDrawPile();
        discardPile.Clear();
        
        Debug.Log($"PlayerDeck: デッキ初期化完了 - {selectedDeck.Count}枚");
    }
    
    /// <summary>
    /// ドローピールを初期化（シャッフル）
    /// </summary>
    public void InitializeDrawPile()
    {
        drawPile.Clear();
        
        // デッキをシャッフルしてドローピールに追加
        var shuffledDeck = selectedDeck.OrderBy(x => Random.Range(0f, 1f)).ToList();
        
        foreach (var card in shuffledDeck)
        {
            drawPile.Enqueue(card);
        }
        
        Debug.Log($"PlayerDeck: ドローピール初期化完了 - {drawPile.Count}枚");
    }
    
    /// <summary>
    /// カードをドロー
    /// </summary>
    public CardDataSO DrawCard()
    {
        if (drawPile.Count == 0)
        {
            // ドローピールが空の場合、ディスカードピールをシャッフルしてドローピールに追加
            ReshuffleDiscardPile();
            
            if (drawPile.Count == 0)
            {
                Debug.LogWarning("PlayerDeck: ドロー可能なカードがありません");
                return null;
            }
        }
        
        var drawnCard = drawPile.Dequeue();
        Debug.Log($"PlayerDeck: カードドロー - {drawnCard.cardName}");
        
        return drawnCard;
    }
    
    /// <summary>
    /// カードをディスカードピールに追加
    /// </summary>
    public void DiscardCard(CardDataSO card)
    {
        if (card != null)
        {
            discardPile.Add(card);
            Debug.Log($"PlayerDeck: カードディスカード - {card.cardName}");
        }
    }
    
    /// <summary>
    /// ディスカードピールをシャッフルしてドローピールに追加
    /// </summary>
    public void ReshuffleDiscardPile()
    {
        if (discardPile.Count == 0)
        {
            Debug.LogWarning("PlayerDeck: ディスカードピールが空です");
            return;
        }
        
        var shuffledDiscard = discardPile.OrderBy(x => Random.Range(0f, 1f)).ToList();
        
        foreach (var card in shuffledDiscard)
        {
            drawPile.Enqueue(card);
        }
        
        discardPile.Clear();
        
        Debug.Log($"PlayerDeck: ディスカードピールをリシャッフル - {drawPile.Count}枚");
    }
    
    /// <summary>
    /// デッキをシャッフル
    /// </summary>
    public void ShuffleDeck()
    {
        InitializeDrawPile();
        Debug.Log("PlayerDeck: デッキをシャッフルしました");
    }
    
    /// <summary>
    /// デッキにカードを追加
    /// </summary>
    public bool AddCardToDeck(CardDataSO card)
    {
        if (selectedDeck.Count >= maxDeckSize)
        {
            Debug.LogWarning($"PlayerDeck: デッキが最大枚数({maxDeckSize}枚)に達しています");
            return false;
        }
        
        if (card != null)
        {
            selectedDeck.Add(card);
            Debug.Log($"PlayerDeck: デッキにカード追加 - {card.cardName} (現在{selectedDeck.Count}枚)");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// デッキからカードを削除
    /// </summary>
    public bool RemoveCardFromDeck(CardDataSO card)
    {
        if (selectedDeck.Remove(card))
        {
            Debug.Log($"PlayerDeck: デッキからカード削除 - {card.cardName} (現在{selectedDeck.Count}枚)");
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// デッキが有効かチェック
    /// </summary>
    public bool IsValidDeck()
    {
        return selectedDeck.Count >= minDeckSize && selectedDeck.Count <= maxDeckSize;
    }
    
    /// <summary>
    /// デッキの統計情報を取得
    /// </summary>
    public DeckStatistics GetDeckStatistics()
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
    /// デッキの状態を取得
    /// </summary>
    public DeckStatus GetDeckStatus()
    {
        return new DeckStatus
        {
            deckSize = selectedDeck.Count,
            drawPileSize = drawPile.Count,
            discardPileSize = discardPile.Count,
            isValid = IsValidDeck()
        };
    }
    
    /// <summary>
    /// デッキをクリア
    /// </summary>
    public void ClearDeck()
    {
        selectedDeck.Clear();
        drawPile.Clear();
        discardPile.Clear();
        Debug.Log("PlayerDeck: デッキをクリアしました");
    }
}

/// <summary>
/// デッキの状態情報
/// </summary>
[System.Serializable]
public struct DeckStatus
{
    public int deckSize;
    public int drawPileSize;
    public int discardPileSize;
    public bool isValid;
} 