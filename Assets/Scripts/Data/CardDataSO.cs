using UnityEngine;

public enum CardType { Attack, Move, Heal }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Battle/Card Data")]
public class CardDataSO : ScriptableObject
{
    public string cardName;
    public CardType type;
    public Sprite icon;
    public int power;
    
    [Header("Move Settings")]
    public Vector2Int moveDirection = Vector2Int.down; // 移動方向
    public int moveDistance = 1; // 移動距離
    
    [Header("Heal Settings")]
    public int healAmount = 10; // 回復量
} 