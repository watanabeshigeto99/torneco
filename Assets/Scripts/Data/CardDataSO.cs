using UnityEngine;

public enum CardType { Attack, Move, Heal }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Battle/Card Data")]
public class CardDataSO : ScriptableObject
{
    public string cardName;
    public CardType type;
    public Sprite icon;
    public int power;
} 