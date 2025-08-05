using UnityEngine;
using System;

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
        
        [Header("Settings")]
        public float levelUpMultiplier = 1.5f;
        public int hpGainPerLevel = 5;
        
        // イベント
        public event Action<PlayerDataSO> OnDataChanged;
        public event Action<int> OnLevelUp;
        public event Action<int> OnExpGained;
        public event Action<int, int> OnHPChanged;
        
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
            
            Debug.Log("PlayerDataSO: プレイヤーデータを初期化しました");
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
            }
            
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public void TakeDamage(int damage)
        {
            int actualDamage = Mathf.Max(0, damage - defense);
            SetHP(currentHP - actualDamage);
            
            Debug.Log($"PlayerDataSO: {actualDamage}ダメージを受けました (防御力: {defense})");
        }
        
        /// <summary>
        /// 回復する
        /// </summary>
        public void Heal(int healAmount)
        {
            SetHP(currentHP + healAmount);
            
            Debug.Log($"PlayerDataSO: {healAmount}回復しました");
        }
        
        /// <summary>
        /// レベルの設定
        /// </summary>
        public void SetLevel(int newLevel)
        {
            if (newLevel != level)
            {
                level = Mathf.Clamp(newLevel, 1, maxLevel);
                OnLevelUp?.Invoke(level);
                OnDataChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// 経験値の設定
        /// </summary>
        public void SetExperience(int exp)
        {
            if (exp != experience)
            {
                experience = Mathf.Max(0, exp);
                OnExpGained?.Invoke(0);
                OnDataChanged?.Invoke(this);
            }
        }
        
        /// <summary>
        /// 攻撃力の設定
        /// </summary>
        public void SetAttack(int newAttack)
        {
            attack = Mathf.Max(0, newAttack);
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// 防御力の設定
        /// </summary>
        public void SetDefense(int newDefense)
        {
            defense = Mathf.Max(0, newDefense);
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// ゴールドの追加
        /// </summary>
        public void AddGold(int amount)
        {
            gold += amount;
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// ゴールドの消費
        /// </summary>
        public bool SpendGold(int amount)
        {
            if (gold >= amount)
            {
                gold -= amount;
                OnDataChanged?.Invoke(this);
                return true;
            }
            return false;
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
        /// 最大レベルに達しているかチェック
        /// </summary>
        public bool IsMaxLevel()
        {
            return level >= maxLevel;
        }
        
        /// <summary>
        /// プレイヤーデータの情報を取得
        /// </summary>
        public string GetPlayerInfo()
        {
            return $"Player: {playerName}, Level: {level}/{maxLevel}, HP: {currentHP}/{maxHP}, " +
                   $"Exp: {experience}/{experienceToNext}, Attack: {attack}, Defense: {defense}, Gold: {gold}";
        }
        
        /// <summary>
        /// プレイヤーデータの詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== Player Data ===\n" +
                   $"Name: {playerName}\n" +
                   $"Level: {level}/{maxLevel}\n" +
                   $"Experience: {experience}/{experienceToNext} ({GetExpPercentage():P1})\n" +
                   $"HP: {currentHP}/{maxHP} ({GetHPPercentage():P1})\n" +
                   $"Attack: {attack}\n" +
                   $"Defense: {defense}\n" +
                   $"Gold: {gold}\n" +
                   $"Alive: {IsAlive()}\n" +
                   $"Max Level: {IsMaxLevel()}";
        }
    }
} 