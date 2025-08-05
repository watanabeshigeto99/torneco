using UnityEngine;
using System;

namespace BattleSystem
{
    /// <summary>
    /// 戦闘ユニットの基底クラス
    /// 責務：ユニットの状態管理のみ
    /// </summary>
    public abstract class BattleUnit : MonoBehaviour
    {
        [Header("Unit Stats")]
        public string unitName = "Unit";
        public int maxHP = 20;
        public int currentHP = 20;
        public int attack = 5;
        public int defense = 2;
        
        [Header("Battle State")]
        public bool isAlive = true;
        public bool isStunned = false;
        public int stunTurns = 0;
        
        // ユニット状態変更イベント
        public event Action<BattleUnit> OnUnitDamaged;
        public event Action<BattleUnit> OnUnitHealed;
        public event Action<BattleUnit> OnUnitDied;
        public event Action<BattleUnit> OnUnitStunned;
        public event Action<BattleUnit> OnUnitRecovered;
        
        /// <summary>
        /// ユニットの初期化
        /// </summary>
        protected virtual void Awake()
        {
            InitializeUnit();
        }
        
        /// <summary>
        /// ユニットの初期化処理
        /// </summary>
        protected virtual void InitializeUnit()
        {
            currentHP = maxHP;
            isAlive = true;
            isStunned = false;
            stunTurns = 0;
            
            Debug.Log($"BattleUnit: {unitName}を初期化しました");
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (!isAlive) return;
            
            int actualDamage = Mathf.Max(1, damage - defense);
            currentHP = Mathf.Max(0, currentHP - actualDamage);
            
            OnUnitDamaged?.Invoke(this);
            
            Debug.Log($"{unitName}が{actualDamage}ダメージを受けました (HP: {currentHP}/{maxHP})");
            
            if (currentHP <= 0)
            {
                Die();
            }
        }
        
        /// <summary>
        /// 回復する
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (!isAlive) return;
            
            int oldHP = currentHP;
            currentHP = Mathf.Min(maxHP, currentHP + amount);
            int healedAmount = currentHP - oldHP;
            
            if (healedAmount > 0)
            {
                OnUnitHealed?.Invoke(this);
                Debug.Log($"{unitName}が{healedAmount}回復しました (HP: {currentHP}/{maxHP})");
            }
        }
        
        /// <summary>
        /// 死亡処理
        /// </summary>
        protected virtual void Die()
        {
            isAlive = false;
            currentHP = 0;
            OnUnitDied?.Invoke(this);
            
            Debug.Log($"{unitName}が死亡しました");
        }
        
        /// <summary>
        /// スタン状態にする
        /// </summary>
        public virtual void Stun(int turns)
        {
            if (!isAlive) return;
            
            isStunned = true;
            stunTurns = turns;
            OnUnitStunned?.Invoke(this);
            
            Debug.Log($"{unitName}が{turns}ターンスタンしました");
        }
        
        /// <summary>
        /// スタン状態を回復
        /// </summary>
        public virtual void RecoverFromStun()
        {
            if (!isStunned) return;
            
            stunTurns--;
            if (stunTurns <= 0)
            {
                isStunned = false;
                stunTurns = 0;
                OnUnitRecovered?.Invoke(this);
                
                Debug.Log($"{unitName}のスタンが回復しました");
            }
        }
        
        /// <summary>
        /// 生存チェック
        /// </summary>
        public virtual bool IsAlive()
        {
            return isAlive && currentHP > 0;
        }
        
        /// <summary>
        /// スタン状態チェック
        /// </summary>
        public virtual bool IsStunned()
        {
            return isStunned && stunTurns > 0;
        }
        
        /// <summary>
        /// 行動可能かチェック
        /// </summary>
        public virtual bool CanAct()
        {
            return IsAlive() && !IsStunned();
        }
        
        /// <summary>
        /// HPの割合を取得
        /// </summary>
        public virtual float GetHPPercentage()
        {
            return (float)currentHP / maxHP;
        }
        
        /// <summary>
        /// 最大HPの設定
        /// </summary>
        public virtual void SetMaxHP(int newMaxHP)
        {
            maxHP = newMaxHP;
            currentHP = Mathf.Min(currentHP, maxHP);
        }
        
        /// <summary>
        /// 攻撃力の設定
        /// </summary>
        public virtual void SetAttack(int newAttack)
        {
            attack = newAttack;
        }
        
        /// <summary>
        /// 防御力の設定
        /// </summary>
        public virtual void SetDefense(int newDefense)
        {
            defense = newDefense;
        }
        
        /// <summary>
        /// 完全回復
        /// </summary>
        public virtual void FullHeal()
        {
            currentHP = maxHP;
            OnUnitHealed?.Invoke(this);
            
            Debug.Log($"{unitName}が完全回復しました");
        }
        
        /// <summary>
        /// ユニット情報の取得
        /// </summary>
        public virtual string GetUnitInfo()
        {
            return $"{unitName} - HP: {currentHP}/{maxHP}, ATK: {attack}, DEF: {defense}";
        }
    }
} 