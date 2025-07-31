using UnityEngine;

public enum CardType { Attack, Move, Heal }

[CreateAssetMenu(fileName = "NewCard", menuName = "Card Battle/Card Data")]
public class CardDataSO : ScriptableObject
{
    public string cardName;
    public CardType type;
    public Sprite icon;
    public int power;
    
    [Header("Level System")]
    public int level = 1;
    public int maxLevel = 5; // 最大レベル
    
    [Header("Upgrade Data")]
    public CardUpgradeSO upgradeData;
    
    [Header("Move Settings")]
    public Vector2Int moveDirection = Vector2Int.down; // 移動方向
    public int moveDistance = 1; // 移動距離
    
    [Header("Heal Settings")]
    public int healAmount = 10; // 回復量
    
    // レベルに応じた効果的なパワーを取得
    public int GetEffectivePower()
    {
        if (upgradeData != null)
        {
            return power + upgradeData.GetPowerIncrease(level);
        }
        else
        {
            // 従来の計算方法（フォールバック）
            return power + (level - 1) * 5;
        }
    }
    
    // レベルに応じた効果的な回復量を取得
    public int GetEffectiveHealAmount()
    {
        if (upgradeData != null)
        {
            return healAmount + upgradeData.GetHealIncrease(level);
        }
        else
        {
            // 従来の計算方法（フォールバック）
            return healAmount + (level - 1) * 3;
        }
    }
    
    // レベルに応じた効果的な移動距離を取得
    public int GetEffectiveMoveDistance()
    {
        if (upgradeData != null)
        {
            return moveDistance + upgradeData.GetMoveIncrease(level);
        }
        else
        {
            // 従来の計算方法（フォールバック）
            return moveDistance + (level - 1) / 2; // 2レベルごとに1マス増加
        }
    }
    
    // カードを強化
    public bool LevelUp()
    {
        if (level < maxLevel)
        {
            level++;
            return true;
        }
        return false;
    }
} 