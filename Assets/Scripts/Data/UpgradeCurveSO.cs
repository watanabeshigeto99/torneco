using UnityEngine;

/// <summary>
/// レア度
/// </summary>
public enum CardRarity
{
    Normal = 0,     // N
    Rare = 1,       // R
    SuperRare = 2,  // SR
    UltraRare = 3   // UR
}

/// <summary>
/// 強化曲線データ（レア度ごと）
/// </summary>
[CreateAssetMenu(fileName = "NewUpgradeCurve", menuName = "Card Battle/Upgrade Curve Data")]
public class UpgradeCurveSO : ScriptableObject
{
    [Header("Rarity Settings")]
    public CardRarity rarity = CardRarity.Normal;
    public int maxLevel = 8;
    
    [Header("Value Growth")]
    [Tooltip("レベルごとの倍率（乗算型）")]
    public float[] valuePerLevel = new float[15] 
    { 
        1.0f, 1.2f, 1.4f, 1.6f, 1.8f, 2.0f, 2.2f, 2.4f, 2.6f, 2.8f, 
        3.0f, 3.2f, 3.4f, 3.6f, 3.8f 
    };
    
    [Header("Upgrade Costs")]
    [Tooltip("レベルごとの必要ゴールド")]
    public int[] costGold = new int[15] 
    { 
        100, 200, 300, 400, 500, 600, 700, 800, 900, 1000,
        1100, 1200, 1300, 1400, 1500
    };
    
    [Tooltip("レベルごとの必要シャード")]
    public int[] costShards = new int[15] 
    { 
        10, 20, 30, 40, 50, 60, 70, 80, 90, 100,
        110, 120, 130, 140, 150
    };
    
    [Header("Success Rates")]
    [Tooltip("レベルごとの成功率（0.0-1.0）")]
    public float[] successRate = new float[15] 
    { 
        1.0f, 0.95f, 0.90f, 0.85f, 0.80f, 0.75f, 0.70f, 0.65f, 0.60f, 0.55f,
        0.50f, 0.45f, 0.40f, 0.35f, 0.30f
    };
    
    /// <summary>
    /// 指定レベルの倍率を取得
    /// </summary>
    public float GetValueMultiplier(int level)
    {
        if (level <= 0 || level > maxLevel) return 1.0f;
        int index = Mathf.Clamp(level - 1, 0, valuePerLevel.Length - 1);
        return valuePerLevel[index];
    }
    
    /// <summary>
    /// 指定レベルの必要ゴールドを取得
    /// </summary>
    public int GetRequiredGold(int level)
    {
        if (level <= 0 || level >= maxLevel) return 0;
        int index = Mathf.Clamp(level - 1, 0, costGold.Length - 1);
        return costGold[index];
    }
    
    /// <summary>
    /// 指定レベルの必要シャードを取得
    /// </summary>
    public int GetRequiredShards(int level)
    {
        if (level <= 0 || level >= maxLevel) return 0;
        int index = Mathf.Clamp(level - 1, 0, costShards.Length - 1);
        return costShards[index];
    }
    
    /// <summary>
    /// 指定レベルの成功率を取得
    /// </summary>
    public float GetSuccessRate(int level)
    {
        if (level <= 0 || level >= maxLevel) return 0.0f;
        int index = Mathf.Clamp(level - 1, 0, successRate.Length - 1);
        return successRate[index];
    }
    
    /// <summary>
    /// 最大レベルかどうかをチェック
    /// </summary>
    public bool IsMaxLevel(int level)
    {
        return level >= maxLevel;
    }
}
