using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewLevelTable", menuName = "Card Battle/Level Table Data")]
public class LevelTableSO : ScriptableObject
{
    [Header("Level Settings")]
    [Tooltip("最大レベル")]
    public int maxLevel = 10;
    
    [Header("Experience Curve")]
    [Tooltip("レベルごとの必要経験値曲線")]
    public AnimationCurve expCurve = AnimationCurve.Linear(1, 10, 10, 100);
    
    [Tooltip("基本経験値")]
    public int baseExp = 10;
    
    [Tooltip("経験値倍率")]
    public float expMultiplier = 1.0f;
    
    [Header("HP Growth")]
    [Tooltip("レベルごとのHP増加曲線")]
    public AnimationCurve hpGrowthCurve = AnimationCurve.Linear(1, 0, 10, 50);
    
    [Tooltip("基本HP増加")]
    public int baseHPIncrease = 5;
    
    [Header("Rewards")]
    [Tooltip("レベルごとの報酬")]
    public LevelReward[] levelRewards;
    
    /// <summary>
    /// 指定レベルに必要な経験値を取得
    /// </summary>
    /// <param name="level">レベル</param>
    /// <returns>必要な経験値</returns>
    public int GetExpRequired(int level)
    {
        if (level <= 1) return 0;
        
        float curveValue = expCurve.Evaluate(Mathf.Clamp(level, 1, maxLevel));
        return Mathf.RoundToInt(baseExp * curveValue * expMultiplier);
    }
    
    /// <summary>
    /// 指定レベルでのHP増加を取得
    /// </summary>
    /// <param name="level">レベル</param>
    /// <returns>HP増加量</returns>
    public int GetHPIncrease(int level)
    {
        if (level <= 1) return 0;
        
        float curveValue = hpGrowthCurve.Evaluate(Mathf.Clamp(level, 1, maxLevel));
        return Mathf.RoundToInt(baseHPIncrease * curveValue);
    }
    
    /// <summary>
    /// 指定レベルでの報酬を取得
    /// </summary>
    /// <param name="level">レベル</param>
    /// <returns>報酬データ</returns>
    public LevelReward GetReward(int level)
    {
        if (levelRewards == null || levelRewards.Length == 0)
            return new LevelReward { level = level };
        
        foreach (var reward in levelRewards)
        {
            if (reward.level == level)
                return reward;
        }
        
        return new LevelReward { level = level };
    }
    
    /// <summary>
    /// レベルアップデータを取得
    /// </summary>
    /// <param name="level">レベル</param>
    /// <returns>レベルアップデータ</returns>
    public LevelUpgradeData GetLevelUpgradeData(int level)
    {
        return new LevelUpgradeData
        {
            level = level,
            expRequired = GetExpRequired(level),
            hpIncrease = GetHPIncrease(level),
            reward = GetReward(level)
        };
    }
    
    /// <summary>
    /// 全レベルのデータを取得
    /// </summary>
    /// <returns>全レベルデータ</returns>
    public List<LevelUpgradeData> GetAllLevelData()
    {
        var allData = new List<LevelUpgradeData>();
        
        for (int i = 1; i <= maxLevel; i++)
        {
            allData.Add(GetLevelUpgradeData(i));
        }
        
        return allData;
    }
}

/// <summary>
/// レベルアップデータ
/// </summary>
[System.Serializable]
public struct LevelUpgradeData
{
    public int level;
    public int expRequired;
    public int hpIncrease;
    public LevelReward reward;
    
    public override string ToString()
    {
        return $"Level {level}: EXP={expRequired}, HP+{hpIncrease}, Reward={reward}";
    }
}

/// <summary>
/// レベル報酬
/// </summary>
[System.Serializable]
public struct LevelReward
{
    public int level;
    public RewardType rewardType;
    public int rewardValue;
    public CardDataSO cardReward;
    public string description;
    
    public override string ToString()
    {
        if (rewardType == RewardType.Card && cardReward != null)
            return $"Level {level}: {rewardType} - {cardReward.cardName}";
        else
            return $"Level {level}: {rewardType} - {rewardValue}";
    }
}

/// <summary>
/// 報酬タイプ
/// </summary>
public enum RewardType
{
    None,
    Card,           // カード獲得
    Gold,           // ゴールド
    Item,           // アイテム
    Skill,          // スキル
    StatBoost       // ステータス強化
} 