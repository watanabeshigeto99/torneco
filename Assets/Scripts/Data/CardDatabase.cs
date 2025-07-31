using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCardDatabase", menuName = "Card Battle/Card Database")]
public class CardDatabase : ScriptableObject
{
    [Header("Available Cards")]
    [Tooltip("デッキ構築で選択可能な全カード")]
    public List<CardDataSO> availableCards = new List<CardDataSO>();
    
    [Header("Card Categories")]
    [Tooltip("攻撃カード")]
    public List<CardDataSO> attackCards = new List<CardDataSO>();
    
    [Tooltip("移動カード")]
    public List<CardDataSO> moveCards = new List<CardDataSO>();
    
    [Tooltip("回復カード")]
    public List<CardDataSO> healCards = new List<CardDataSO>();
    
    [Header("Settings")]
    [Tooltip("デッキの最大枚数")]
    public int maxDeckSize = 10;
    
    [Tooltip("デッキの最小枚数")]
    public int minDeckSize = 5;
    
    /// <summary>
    /// 全カードを取得
    /// </summary>
    public List<CardDataSO> GetAllCards()
    {
        return new List<CardDataSO>(availableCards);
    }
    
    /// <summary>
    /// カードタイプ別にカードを取得
    /// </summary>
    public List<CardDataSO> GetCardsByType(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:
                return new List<CardDataSO>(attackCards);
            case CardType.Move:
                return new List<CardDataSO>(moveCards);
            case CardType.Heal:
                return new List<CardDataSO>(healCards);
            default:
                return new List<CardDataSO>();
        }
    }
    
    /// <summary>
    /// カード名でカードを検索
    /// </summary>
    public CardDataSO GetCardByName(string cardName)
    {
        return availableCards.Find(card => card.cardName == cardName);
    }
    
    /// <summary>
    /// デッキサイズが有効かチェック
    /// </summary>
    public bool IsValidDeckSize(int deckSize)
    {
        return deckSize >= minDeckSize && deckSize <= maxDeckSize;
    }
    
    /// <summary>
    /// デッキ構築時の制限チェック
    /// </summary>
    public bool CanAddCardToDeck(List<CardDataSO> currentDeck)
    {
        return currentDeck.Count < maxDeckSize;
    }
    
    /// <summary>
    /// デッキが有効かチェック（最小枚数以上）
    /// </summary>
    public bool IsValidDeck(List<CardDataSO> deck)
    {
        return deck.Count >= minDeckSize;
    }
    
    /// <summary>
    /// デッキの統計情報を取得
    /// </summary>
    public DeckStatistics GetDeckStatistics(List<CardDataSO> deck)
    {
        var stats = new DeckStatistics();
        
        foreach (var card in deck)
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
        
        stats.totalCards = deck.Count;
        return stats;
    }
}

/// <summary>
/// デッキの統計情報
/// </summary>
[System.Serializable]
public struct DeckStatistics
{
    public int totalCards;
    public int attackCardCount;
    public int moveCardCount;
    public int healCardCount;
    public int totalAttackPower;
    public int totalMoveDistance;
    public int totalHealAmount;
    
    public float GetAttackCardRatio()
    {
        return totalCards > 0 ? (float)attackCardCount / totalCards : 0f;
    }
    
    public float GetMoveCardRatio()
    {
        return totalCards > 0 ? (float)moveCardCount / totalCards : 0f;
    }
    
    public float GetHealCardRatio()
    {
        return totalCards > 0 ? (float)healCardCount / totalCards : 0f;
    }
} 