using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// カード強化サービス
/// </summary>
public class UpgradeService : MonoBehaviour
{
    public static UpgradeService Instance { get; private set; }
    
    [Header("Upgrade Curves")]
    public UpgradeCurveSO[] upgradeCurves;
    
    [Header("Default Settings")]
    public UpgradeCurveSO defaultUpgradeCurve;
    
    private Dictionary<CardRarity, UpgradeCurveSO> upgradeCurveDict;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeUpgradeCurves();
    }
    
    /// <summary>
    /// 強化曲線の初期化
    /// </summary>
    private void InitializeUpgradeCurves()
    {
        upgradeCurveDict = new Dictionary<CardRarity, UpgradeCurveSO>();
        
        if (upgradeCurves != null)
        {
            foreach (var curve in upgradeCurves)
            {
                if (curve != null)
                {
                    upgradeCurveDict[curve.rarity] = curve;
                }
            }
        }
        
        // デフォルト曲線を設定
        if (defaultUpgradeCurve != null)
        {
            upgradeCurveDict[CardRarity.Normal] = defaultUpgradeCurve;
        }
    }
    
    /// <summary>
    /// 強化プレビューを取得
    /// </summary>
    public UpgradePreview GetPreview(string cardId)
    {
        if (!CardUpgradeFeature.IsEnabled)
        {
            return null;
        }
        
        // カードデータを取得
        var cardData = GetCardData(cardId);
        if (cardData == null)
        {
            Debug.LogWarning($"UpgradeService: カードデータが見つかりません: {cardId}");
            return null;
        }
        
        // カードインスタンスを取得
        var cardInstance = GetCardInstance(cardId);
        int currentLevel = cardInstance?.level ?? 1;
        
        // 強化曲線を取得
        var upgradeCurve = GetUpgradeCurve(cardData);
        if (upgradeCurve == null)
        {
            Debug.LogWarning($"UpgradeService: 強化曲線が見つかりません: {cardId}");
            return null;
        }
        
        // 最大レベルチェック
        if (upgradeCurve.IsMaxLevel(currentLevel))
        {
            return new UpgradePreview(cardId, currentLevel, currentLevel, 
                GetCardValue(cardData, currentLevel), GetCardValue(cardData, currentLevel),
                0, 0, 0.0f, true);
        }
        
        int nextLevel = currentLevel + 1;
        int currentValue = GetCardValue(cardData, currentLevel);
        int nextValue = GetCardValue(cardData, nextLevel);
        int requiredGold = upgradeCurve.GetRequiredGold(currentLevel);
        int requiredShards = upgradeCurve.GetRequiredShards(currentLevel);
        float successRate = upgradeCurve.GetSuccessRate(currentLevel);
        
        return new UpgradePreview(cardId, currentLevel, nextLevel, currentValue, nextValue,
            requiredGold, requiredShards, successRate, false);
    }
    
    /// <summary>
    /// 強化を実行
    /// </summary>
    public UpgradeResult TryUpgrade(string cardId, bool useProtector = false)
    {
        if (!CardUpgradeFeature.IsEnabled)
        {
            return new UpgradeResult(false, cardId, 0, 0, 0, 0, "強化機能が無効です");
        }
        
        // プレビューを取得
        var preview = GetPreview(cardId);
        if (preview == null)
        {
            return new UpgradeResult(false, cardId, 0, 0, 0, 0, "強化プレビューの取得に失敗しました");
        }
        
        if (preview.isMaxLevel)
        {
            return new UpgradeResult(false, cardId, preview.currentLevel, preview.currentLevel, 
                0, 0, "最大レベルに達しています");
        }
        
        // リソースチェック
        if (!HasEnoughResources(preview.requiredGold, preview.requiredShards))
        {
            return new UpgradeResult(false, cardId, preview.currentLevel, preview.currentLevel,
                0, 0, "リソースが不足しています");
        }
        
        // リソース消費
        ConsumeResources(preview.requiredGold, preview.requiredShards);
        
        // 成功判定
        bool success = Random.Range(0f, 1f) <= preview.successRate;
        
        if (success)
        {
            // 強化成功
            var cardInstance = GetCardInstance(cardId);
            if (cardInstance == null)
            {
                cardInstance = new CardInstance(cardId, 1);
                AddCardInstance(cardInstance);
            }
            
            int previousLevel = cardInstance.level;
            cardInstance.level = preview.nextLevel;
            
            UpdateCardInstance(cardInstance); // インスタンスを更新
            
            return new UpgradeResult(true, cardId, previousLevel, cardInstance.level,
                preview.requiredGold, preview.requiredShards, "強化に成功しました！");
        }
        else
        {
            // 強化失敗（リソースは消費されるがレベルは変わらない）
            return new UpgradeResult(false, cardId, preview.currentLevel, preview.currentLevel,
                preview.requiredGold, preview.requiredShards, "強化に失敗しました");
        }
    }
    
    /// <summary>
    /// カードデータを取得
    /// </summary>
    private CardDataSO GetCardData(string cardId)
    {
        // CardManagerからカードプールを取得
        if (CardManager.Instance != null && CardManager.Instance.cardPool != null)
        {
            return CardManager.Instance.cardPool.FirstOrDefault(card => card.cardName == cardId);
        }
        
        // リソースから読み込み
        var cardData = Resources.Load<CardDataSO>($"SO/Card/{cardId}");
        return cardData;
    }
    
    /// <summary>
    /// カードインスタンスを取得
    /// </summary>
    private CardInstance GetCardInstance(string cardId)
    {
        // PlayerDataManagerから取得
        if (PlayerDataSystem.PlayerDataManager.Instance != null && 
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData() != null)
        {
            return PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData().GetCardInstance(cardId);
        }
        
        return null;
    }
    
    /// <summary>
    /// カードインスタンスを追加
    /// </summary>
    private void AddCardInstance(CardInstance cardInstance)
    {
        if (PlayerDataSystem.PlayerDataManager.Instance != null && 
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData() != null)
        {
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData().AddCardInstance(cardInstance);
        }
        else
        {
            Debug.LogWarning($"UpgradeService: PlayerDataManagerが見つかりません");
        }
    }
    
    /// <summary>
    /// カードインスタンスを更新
    /// </summary>
    private void UpdateCardInstance(CardInstance cardInstance)
    {
        if (PlayerDataSystem.PlayerDataManager.Instance != null && 
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData() != null)
        {
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData().UpdateCardInstance(cardInstance);
        }
        else
        {
            Debug.LogWarning($"UpgradeService: PlayerDataManagerが見つかりません");
        }
    }
    
    /// <summary>
    /// 強化曲線を取得
    /// </summary>
    private UpgradeCurveSO GetUpgradeCurve(CardDataSO cardData)
    {
        // カードデータからレア度を取得
        CardRarity rarity = cardData.rarity;
        
        if (upgradeCurveDict.TryGetValue(rarity, out var curve))
        {
            return curve;
        }
        
        return defaultUpgradeCurve;
    }
    
    /// <summary>
    /// カードの効果値を取得
    /// </summary>
    private int GetCardValue(CardDataSO cardData, int level)
    {
        if (cardData == null) return 0;
        
        // 既存のGetEffectivePowerメソッドを使用
        if (cardData.type == CardType.Attack)
        {
            return cardData.GetEffectivePower();
        }
        else if (cardData.type == CardType.Heal)
        {
            return cardData.GetEffectiveHealAmount();
        }
        else if (cardData.type == CardType.Move)
        {
            return cardData.GetEffectiveMoveDistance();
        }
        
        return cardData.power;
    }
    
    /// <summary>
    /// リソースが十分かチェック
    /// </summary>
    private bool HasEnoughResources(int requiredGold, int requiredShards)
    {
        if (PlayerDataSystem.PlayerDataManager.Instance == null || 
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData() == null)
        {
            Debug.LogWarning("UpgradeService: PlayerDataManagerが見つかりません");
            return false;
        }
        
        var playerData = PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData();
        return playerData.gold >= requiredGold && playerData.shards >= requiredShards;
    }
    
    /// <summary>
    /// リソースを消費
    /// </summary>
    private void ConsumeResources(int gold, int shards)
    {
        if (PlayerDataSystem.PlayerDataManager.Instance == null || 
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData() == null)
        {
            Debug.LogWarning("UpgradeService: PlayerDataManagerが見つかりません");
            return;
        }
        
        var playerData = PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData();
        
        if (gold > 0)
        {
            playerData.SpendGold(gold);
        }
        
        if (shards > 0)
        {
            playerData.SpendShards(shards);
        }
    }
}
