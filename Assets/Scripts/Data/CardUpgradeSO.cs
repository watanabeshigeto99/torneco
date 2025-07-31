using UnityEngine;

[CreateAssetMenu(fileName = "NewCardUpgrade", menuName = "Card Battle/Card Upgrade Data")]
public class CardUpgradeSO : ScriptableObject
{
    [Header("Card Info")]
    [Tooltip("対象カード")]
    public CardDataSO targetCard;
    
    [Header("Upgrade Settings")]
    [Tooltip("最大レベル")]
    public int maxLevel = 5;
    
    [Tooltip("レベルアップに必要な経験値")]
    public int expRequiredForLevelUp = 10;
    
    [Header("Power Upgrade")]
    [Tooltip("レベルごとの攻撃力増加")]
    public AnimationCurve powerGrowthCurve = AnimationCurve.Linear(1, 0, 5, 20);
    
    [Tooltip("基本攻撃力増加")]
    public int basePowerIncrease = 5;
    
    [Header("Heal Upgrade")]
    [Tooltip("レベルごとの回復量増加")]
    public AnimationCurve healGrowthCurve = AnimationCurve.Linear(1, 0, 5, 15);
    
    [Tooltip("基本回復量増加")]
    public int baseHealIncrease = 3;
    
    [Header("Move Upgrade")]
    [Tooltip("レベルごとの移動距離増加")]
    public AnimationCurve moveGrowthCurve = AnimationCurve.Linear(1, 0, 5, 2);
    
    [Tooltip("基本移動距離増加")]
    public int baseMoveIncrease = 1;
    
    [Header("Special Effects")]
    [Tooltip("レベルごとの特殊効果")]
    public SpecialEffectDefinition[] specialEffects;
    
    /// <summary>
    /// 指定レベルでの攻撃力増加を取得
    /// </summary>
    /// <param name="level">カードレベル</param>
    /// <returns>攻撃力増加量</returns>
    public int GetPowerIncrease(int level)
    {
        if (level <= 1) return 0;
        
        float curveValue = powerGrowthCurve.Evaluate(Mathf.Clamp(level, 1, maxLevel));
        return Mathf.RoundToInt(basePowerIncrease * curveValue);
    }
    
    /// <summary>
    /// 指定レベルでの回復量増加を取得
    /// </summary>
    /// <param name="level">カードレベル</param>
    /// <returns>回復量増加量</returns>
    public int GetHealIncrease(int level)
    {
        if (level <= 1) return 0;
        
        float curveValue = healGrowthCurve.Evaluate(Mathf.Clamp(level, 1, maxLevel));
        return Mathf.RoundToInt(baseHealIncrease * curveValue);
    }
    
    /// <summary>
    /// 指定レベルでの移動距離増加を取得
    /// </summary>
    /// <param name="level">カードレベル</param>
    /// <returns>移動距離増加量</returns>
    public int GetMoveIncrease(int level)
    {
        if (level <= 1) return 0;
        
        float curveValue = moveGrowthCurve.Evaluate(Mathf.Clamp(level, 1, maxLevel));
        return Mathf.RoundToInt(baseMoveIncrease * curveValue);
    }
    
    /// <summary>
    /// 指定レベルでの特殊効果を取得
    /// </summary>
    /// <param name="level">カードレベル</param>
    /// <returns>特殊効果</returns>
    public CardSpecialEffect GetSpecialEffect(int level)
    {
        if (specialEffects == null || specialEffects.Length == 0)
            return CardSpecialEffect.None;
        
        foreach (var effect in specialEffects)
        {
            if (effect.requiredLevel == level)
                return effect.effectType;
        }
        
        return CardSpecialEffect.None;
    }
    
    /// <summary>
    /// カードの強化データを取得
    /// </summary>
    /// <param name="level">カードレベル</param>
    /// <returns>強化データ</returns>
    public CardUpgradeData GetUpgradeData(int level)
    {
        return new CardUpgradeData
        {
            level = level,
            powerIncrease = GetPowerIncrease(level),
            healIncrease = GetHealIncrease(level),
            moveIncrease = GetMoveIncrease(level),
            specialEffect = GetSpecialEffect(level)
        };
    }
    
    /// <summary>
    /// 次のレベルアップに必要な経験値を取得
    /// </summary>
    /// <param name="currentLevel">現在のレベル</param>
    /// <returns>必要な経験値</returns>
    public int GetExpRequiredForNextLevel(int currentLevel)
    {
        if (currentLevel >= maxLevel) return 0;
        return expRequiredForLevelUp * currentLevel; // レベルに応じて必要経験値が増加
    }
}

/// <summary>
/// カードの強化データ
/// </summary>
[System.Serializable]
public struct CardUpgradeData
{
    public int level;
    public int powerIncrease;
    public int healIncrease;
    public int moveIncrease;
    public CardSpecialEffect specialEffect;
    
    public override string ToString()
    {
        return $"Level {level}: Power+{powerIncrease}, Heal+{healIncrease}, Move+{moveIncrease}, Effect={specialEffect}";
    }
}

/// <summary>
/// カードの特殊効果
/// </summary>
public enum CardSpecialEffect
{
    None,
    Critical,       // クリティカル攻撃
    AreaAttack,     // 範囲攻撃
    HealOverTime,   // 継続回復
    Teleport,       // テレポート移動
    Shield,         // シールド効果
    Poison,         // 毒効果
    Stun,           // スタン効果
    MultiHit        // 複数回攻撃
}

/// <summary>
/// 特殊効果の定義
/// </summary>
[System.Serializable]
public struct SpecialEffectDefinition
{
    public int requiredLevel;
    public CardSpecialEffect effectType;
    public float effectValue;
    public string description;
} 