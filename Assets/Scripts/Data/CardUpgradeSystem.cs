using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// カード強化システムのフィーチャーフラグ
/// </summary>
public static class CardUpgradeFeature
{
    /// <summary>
    /// カード強化機能の有効/無効フラグ
    /// </summary>
    public static bool IsEnabled => UPGRADE_FEATURE;
    
    /// <summary>
    /// フィーチャーフラグ（開発中はtrue、本番リリース前はfalse）
    /// </summary>
    private const bool UPGRADE_FEATURE = true;
}

/// <summary>
/// カードインスタンス（所持データ）
/// </summary>
[System.Serializable]
public class CardInstance
{
    public string cardId;
    public int level = 1;
    
    public CardInstance(string cardId, int level = 1)
    {
        this.cardId = cardId;
        this.level = level;
    }
}

/// <summary>
/// 強化プレビュー情報
/// </summary>
[System.Serializable]
public class UpgradePreview
{
    public string cardId;
    public int currentLevel;
    public int nextLevel;
    public int currentValue;
    public int nextValue;
    public int requiredGold;
    public int requiredShards;
    public float successRate;
    public bool isMaxLevel;
    
    public UpgradePreview(string cardId, int currentLevel, int nextLevel, int currentValue, int nextValue, 
                         int requiredGold, int requiredShards, float successRate, bool isMaxLevel)
    {
        this.cardId = cardId;
        this.currentLevel = currentLevel;
        this.nextLevel = nextLevel;
        this.currentValue = currentValue;
        this.nextValue = nextValue;
        this.requiredGold = requiredGold;
        this.requiredShards = requiredShards;
        this.successRate = successRate;
        this.isMaxLevel = isMaxLevel;
    }
}

/// <summary>
/// 強化結果
/// </summary>
[System.Serializable]
public class UpgradeResult
{
    public bool success;
    public string cardId;
    public int previousLevel;
    public int newLevel;
    public int usedGold;
    public int usedShards;
    public string message;
    
    public UpgradeResult(bool success, string cardId, int previousLevel, int newLevel, 
                        int usedGold, int usedShards, string message)
    {
        this.success = success;
        this.cardId = cardId;
        this.previousLevel = previousLevel;
        this.newLevel = newLevel;
        this.usedGold = usedGold;
        this.usedShards = usedShards;
        this.message = message;
    }
}
