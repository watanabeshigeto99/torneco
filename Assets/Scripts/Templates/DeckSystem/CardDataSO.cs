using UnityEngine;
using System;

namespace DeckSystem
{
    /// <summary>
    /// カードデータ管理用ScriptableObject
    /// 責務：カードデータの状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "CardData", menuName = "Deck System/Card Data")]
    public class CardDataSO : ScriptableObject
    {
        [Header("Card Basic Info")]
        public string cardName = "Unknown Card";
        public string cardDescription = "No description";
        public Sprite cardImage;
        public CardType cardType = CardType.Attack;
        public int cardCost = 1;
        
        [Header("Card Stats")]
        public int attackPower = 0;
        public int defensePower = 0;
        public int healAmount = 0;
        public int drawAmount = 0;
        public int energyCost = 0;
        
        [Header("Card Effects")]
        public bool isUpgraded = false;
        public int upgradeLevel = 0;
        public int maxUpgradeLevel = 3;
        
        [Header("Card State")]
        public bool isInHand = false;
        public bool isInDeck = false;
        public bool isInDiscard = false;
        public bool isPlayed = false;
        
        // イベント
        public event Action<CardDataSO> OnCardStateChanged;
        public event Action<CardDataSO> OnCardUpgraded;
        public event Action<CardDataSO> OnCardPlayed;
        
        /// <summary>
        /// カードタイプの列挙
        /// </summary>
        public enum CardType
        {
            Attack,
            Defense,
            Heal,
            Draw,
            Utility,
            Special
        }
        
        /// <summary>
        /// カードデータの初期化
        /// </summary>
        public void Initialize()
        {
            isInHand = false;
            isInDeck = false;
            isInDiscard = false;
            isPlayed = false;
            upgradeLevel = 0;
            isUpgraded = false;
            
            Debug.Log($"CardDataSO: カードデータを初期化しました - {cardName}");
        }
        
        /// <summary>
        /// カードを手札に移動
        /// </summary>
        public void MoveToHand()
        {
            isInHand = true;
            isInDeck = false;
            isInDiscard = false;
            isPlayed = false;
            
            OnCardStateChanged?.Invoke(this);
            Debug.Log($"CardDataSO: カードを手札に移動しました - {cardName}");
        }
        
        /// <summary>
        /// カードをデッキに移動
        /// </summary>
        public void MoveToDeck()
        {
            isInHand = false;
            isInDeck = true;
            isInDiscard = false;
            isPlayed = false;
            
            OnCardStateChanged?.Invoke(this);
            Debug.Log($"CardDataSO: カードをデッキに移動しました - {cardName}");
        }
        
        /// <summary>
        /// カードを捨て札に移動
        /// </summary>
        public void MoveToDiscard()
        {
            isInHand = false;
            isInDeck = false;
            isInDiscard = true;
            isPlayed = false;
            
            OnCardStateChanged?.Invoke(this);
            Debug.Log($"CardDataSO: カードを捨て札に移動しました - {cardName}");
        }
        
        /// <summary>
        /// カードをプレイ済みに設定
        /// </summary>
        public void SetPlayed()
        {
            isPlayed = true;
            OnCardPlayed?.Invoke(this);
            Debug.Log($"CardDataSO: カードをプレイしました - {cardName}");
        }
        
        /// <summary>
        /// カードをアップグレード
        /// </summary>
        public bool UpgradeCard()
        {
            if (upgradeLevel >= maxUpgradeLevel)
            {
                Debug.LogWarning($"CardDataSO: カードは最大レベルです - {cardName}");
                return false;
            }
            
            upgradeLevel++;
            isUpgraded = true;
            
            // アップグレード効果を適用
            ApplyUpgradeEffects();
            
            OnCardUpgraded?.Invoke(this);
            OnCardStateChanged?.Invoke(this);
            
            Debug.Log($"CardDataSO: カードをアップグレードしました - {cardName} (レベル {upgradeLevel})");
            return true;
        }
        
        /// <summary>
        /// アップグレード効果を適用
        /// </summary>
        private void ApplyUpgradeEffects()
        {
            switch (cardType)
            {
                case CardType.Attack:
                    attackPower += 2;
                    break;
                case CardType.Defense:
                    defensePower += 2;
                    break;
                case CardType.Heal:
                    healAmount += 3;
                    break;
                case CardType.Draw:
                    drawAmount += 1;
                    break;
                case CardType.Utility:
                    energyCost = Mathf.Max(0, energyCost - 1);
                    break;
                case CardType.Special:
                    // 特殊カードのアップグレード効果
                    break;
            }
        }
        
        /// <summary>
        /// カードをリセット
        /// </summary>
        public void ResetCard()
        {
            isInHand = false;
            isInDeck = false;
            isInDiscard = false;
            isPlayed = false;
            upgradeLevel = 0;
            isUpgraded = false;
            
            OnCardStateChanged?.Invoke(this);
            Debug.Log($"CardDataSO: カードをリセットしました - {cardName}");
        }
        
        /// <summary>
        /// カードが手札にあるかチェック
        /// </summary>
        public bool IsInHand()
        {
            return isInHand;
        }
        
        /// <summary>
        /// カードがデッキにあるかチェック
        /// </summary>
        public bool IsInDeck()
        {
            return isInDeck;
        }
        
        /// <summary>
        /// カードが捨て札にあるかチェック
        /// </summary>
        public bool IsInDiscard()
        {
            return isInDiscard;
        }
        
        /// <summary>
        /// カードがプレイ済みかチェック
        /// </summary>
        public bool IsPlayed()
        {
            return isPlayed;
        }
        
        /// <summary>
        /// カードがアップグレード済みかチェック
        /// </summary>
        public bool IsUpgraded()
        {
            return isUpgraded;
        }
        
        /// <summary>
        /// カードが最大レベルかチェック
        /// </summary>
        public bool IsMaxLevel()
        {
            return upgradeLevel >= maxUpgradeLevel;
        }
        
        /// <summary>
        /// カードの効果を取得
        /// </summary>
        public string GetCardEffect()
        {
            switch (cardType)
            {
                case CardType.Attack:
                    return $"攻撃力: {attackPower}";
                case CardType.Defense:
                    return $"防御力: {defensePower}";
                case CardType.Heal:
                    return $"回復量: {healAmount}";
                case CardType.Draw:
                    return $"ドロー数: {drawAmount}";
                case CardType.Utility:
                    return $"エネルギー消費: {energyCost}";
                case CardType.Special:
                    return "特殊効果";
                default:
                    return "効果なし";
            }
        }
        
        /// <summary>
        /// カードの情報を取得
        /// </summary>
        public string GetCardInfo()
        {
            return $"Card: {cardName}, Type: {cardType}, Cost: {cardCost}, " +
                   $"Level: {upgradeLevel}/{maxUpgradeLevel}, State: {GetCardState()}";
        }
        
        /// <summary>
        /// カードの状態を取得
        /// </summary>
        public string GetCardState()
        {
            if (isInHand) return "Hand";
            if (isInDeck) return "Deck";
            if (isInDiscard) return "Discard";
            if (isPlayed) return "Played";
            return "None";
        }
        
        /// <summary>
        /// カードの詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== Card Data ===\n" +
                   $"Name: {cardName}\n" +
                   $"Description: {cardDescription}\n" +
                   $"Type: {cardType}\n" +
                   $"Cost: {cardCost}\n" +
                   $"Attack: {attackPower}\n" +
                   $"Defense: {defensePower}\n" +
                   $"Heal: {healAmount}\n" +
                   $"Draw: {drawAmount}\n" +
                   $"Energy: {energyCost}\n" +
                   $"Upgrade Level: {upgradeLevel}/{maxUpgradeLevel}\n" +
                   $"Is Upgraded: {isUpgraded}\n" +
                   $"State: {GetCardState()}\n" +
                   $"Effect: {GetCardEffect()}";
        }
    }
} 