using UnityEngine;
using System;
using System.Collections.Generic;

namespace PlayerDataSystem
{
    /// <summary>
    /// プレイヤーデータ管理用ScriptableObject
    /// 責務：プレイヤーデータの状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Player Data System/Player Data")]
    public class PlayerDataSO : ScriptableObject
    {
        [Header("Basic Info")]
        public string playerName = "Player";
        public int level = 1;
        public int maxLevel = 10;
        
        [Header("Experience")]
        public int experience = 0;
        public int experienceToNext = 10;
        
        [Header("Health")]
        public int currentHP = 20;
        public int maxHP = 20;
        
        [Header("Stats")]
        public int attack = 5;
        public int defense = 2;
        public int gold = 0;
        public int shards = 0; // 強化用シャード
        
        [Header("Card Collection")]
        public List<CardInstance> ownedCards = new List<CardInstance>();
        
        [Header("Settings")]
        public float levelUpMultiplier = 1.5f;
        public int hpGainPerLevel = 5;
        
        // イベント
        public event Action<PlayerDataSO> OnDataChanged;
        public event Action<int> OnLevelUp;
        public event Action<int> OnExpGained;
        public event Action<int, int> OnHPChanged;
        public event Action<CardInstance> OnCardAdded;
        public event Action<CardInstance> OnCardUpgraded;
        
        /// <summary>
        /// プレイヤーデータの初期化
        /// </summary>
        public void Initialize()
        {
            level = 1;
            experience = 0;
            experienceToNext = 10;
            currentHP = 20;
            maxHP = 20;
            attack = 5;
            defense = 2;
            gold = 0;
            shards = 0;
            ownedCards = new List<CardInstance>();
            
            Debug.Log("PlayerDataSO: プレイヤーデータを初期化しました");
        }
        
        /// <summary>
        /// カードインスタンスを取得
        /// </summary>
        public CardInstance GetCardInstance(string cardId)
        {
            return ownedCards.Find(card => card.cardId == cardId);
        }
        
        /// <summary>
        /// カードインスタンスを追加
        /// </summary>
        public void AddCardInstance(CardInstance cardInstance)
        {
            if (cardInstance == null) return;
            
            var existingCard = GetCardInstance(cardInstance.cardId);
            if (existingCard != null)
            {
                Debug.LogWarning($"PlayerDataSO: カード {cardInstance.cardId} は既に所持しています");
                return;
            }
            
            ownedCards.Add(cardInstance);
            OnCardAdded?.Invoke(cardInstance);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"PlayerDataSO: カード {cardInstance.cardId} を追加しました");
        }
        
        /// <summary>
        /// カードインスタンスを更新
        /// </summary>
        public void UpdateCardInstance(CardInstance cardInstance)
        {
            if (cardInstance == null) return;
            
            var existingCard = GetCardInstance(cardInstance.cardId);
            if (existingCard == null)
            {
                AddCardInstance(cardInstance);
                return;
            }
            
            int previousLevel = existingCard.level;
            existingCard.level = cardInstance.level;
            
            if (existingCard.level > previousLevel)
            {
                OnCardUpgraded?.Invoke(existingCard);
            }
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"PlayerDataSO: カード {cardInstance.cardId} を更新しました (Lv.{previousLevel} → Lv.{existingCard.level})");
        }
        
        /// <summary>
        /// カードインスタンスを削除
        /// </summary>
        public void RemoveCardInstance(string cardId)
        {
            var card = GetCardInstance(cardId);
            if (card != null)
            {
                ownedCards.Remove(card);
                OnDataChanged?.Invoke(this);
                
                Debug.Log($"PlayerDataSO: カード {cardId} を削除しました");
            }
        }
        
        /// <summary>
        /// ゴールドを追加
        /// </summary>
        public void AddGold(int amount)
        {
            gold += amount;
            OnDataChanged?.Invoke(this);
            Debug.Log($"PlayerDataSO: ゴールドを {amount} 追加しました (合計: {gold})");
        }
        
        /// <summary>
        /// ゴールドを消費
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                OnDataChanged?.Invoke(this);
                Debug.Log($"PlayerDataSO: ゴールドを {amount} 消費しました (残高: {gold})");
                return true;
            }
            
            Debug.LogWarning($"PlayerDataSO: ゴールドが不足しています (必要: {amount}, 所持: {gold})");
            return false;
        }
        
        /// <summary>
        /// シャードを追加
        /// </summary>
        public void AddShards(int amount)
        {
            shards += amount;
            OnDataChanged?.Invoke(this);
            Debug.Log($"PlayerDataSO: シャードを {amount} 追加しました (合計: {shards})");
        }
        
        /// <summary>
        /// シャードを消費
        /// </summary>
        public bool SpendShards(int amount)
        {
            if (shards >= amount)
            {
                shards -= amount;
                OnDataChanged?.Invoke(this);
                Debug.Log($"PlayerDataSO: シャードを {amount} 消費しました (残高: {shards})");
                return true;
            }
            
            Debug.LogWarning($"PlayerDataSO: シャードが不足しています (必要: {amount}, 所持: {shards})");
            return false;
        }
        
        /// <summary>
        /// 経験値の追加
        /// </summary>
        public void AddExperience(int amount)
        {
            if (level >= maxLevel) return;
            
            experience += amount;
            OnExpGained?.Invoke(amount);
            
            // レベルアップチェック
            while (experience >= experienceToNext && level < maxLevel)
            {
                LevelUp();
            }
            
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// レベルアップ処理
        /// </summary>
        private void LevelUp()
        {
            level++;
            experience -= experienceToNext;
            
            // ステータス上昇
            int oldMaxHP = maxHP;
            maxHP += hpGainPerLevel;
            currentHP = maxHP; // レベルアップ時は全回復
            
            // 次のレベルアップに必要な経験値を計算
            experienceToNext = Mathf.RoundToInt(experienceToNext * levelUpMultiplier);
            
            OnLevelUp?.Invoke(level);
            Debug.Log($"PlayerDataSO: レベルアップ！レベル {level}、HP {maxHP}");
        }
        
        /// <summary>
        /// HPの設定
        /// </summary>
        public void SetHP(int current, int max = -1)
        {
            int oldCurrent = currentHP;
            int oldMax = maxHP;
            
            currentHP = Mathf.Clamp(current, 0, maxHP);
            if (max > 0)
            {
                maxHP = max;
                currentHP = Mathf.Clamp(currentHP, 0, maxHP);
            }
            
            if (oldCurrent != currentHP || oldMax != maxHP)
            {
                OnHPChanged?.Invoke(currentHP, maxHP);
                OnDataChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0) return;
            
            int oldHP = currentHP;
            currentHP = Mathf.Max(0, currentHP - damage);
            
            if (oldHP != currentHP)
            {
                OnHPChanged?.Invoke(currentHP, maxHP);
                OnDataChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// 回復する
        /// </summary>
        public void Heal(int healAmount)
        {
            if (healAmount <= 0) return;
            
            int oldHP = currentHP;
            currentHP = Mathf.Min(maxHP, currentHP + healAmount);
            
            if (oldHP != currentHP)
            {
                OnHPChanged?.Invoke(currentHP, maxHP);
                OnDataChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// レベルを設定
        /// </summary>
        public void SetLevel(int newLevel)
        {
            if (newLevel < 1 || newLevel > maxLevel) return;
            
            if (level != newLevel)
            {
                level = newLevel;
                OnLevelUp?.Invoke(level);
                OnDataChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// 経験値を設定
        /// </summary>
        public void SetExperience(int exp)
        {
            experience = Mathf.Max(0, exp);
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// 攻撃力を設定
        /// </summary>
        public void SetAttack(int newAttack)
        {
            attack = Mathf.Max(0, newAttack);
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// 防御力を設定
        /// </summary>
        public void SetDefense(int newDefense)
        {
            defense = Mathf.Max(0, newDefense);
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// HPの割合を取得
        /// </summary>
        public float GetHPPercentage()
        {
            return maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
        
        /// <summary>
        /// 経験値の割合を取得
        /// </summary>
        public float GetExpPercentage()
        {
            return experienceToNext > 0 ? (float)experience / experienceToNext : 0f;
        }
        
        /// <summary>
        /// 生存しているかチェック
        /// </summary>
        public bool IsAlive()
        {
            return currentHP > 0;
        }
        
        /// <summary>
        /// 最大レベルかチェック
        /// </summary>
        public bool IsMaxLevel()
        {
            return level >= maxLevel;
        }
        
        /// <summary>
        /// プレイヤー情報を取得
        /// </summary>
        public string GetPlayerInfo()
        {
            return $"Lv.{level} {playerName} (HP: {currentHP}/{maxHP})";
        }
        
        /// <summary>
        /// 詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"Lv.{level} {playerName}\nHP: {currentHP}/{maxHP}\nExp: {experience}/{experienceToNext}\nGold: {gold}\nShards: {shards}\nCards: {ownedCards.Count}";
        }
    }
} 