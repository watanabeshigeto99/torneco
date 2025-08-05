using UnityEngine;
using System;

namespace BattleSystemTemplate
{
    /// <summary>
    /// 戦闘ユニットの基本クラス
    /// 責務：ユニットの基本属性と行動のみ
    /// </summary>
    public class BattleUnit : MonoBehaviour
    {
        [Header("Unit Data")]
        public string unitName = "Unit";
        public int maxHP = 20;
        public int currentHP = 20;
        public int attack = 5;
        public int defense = 2;
        
        [Header("Unit Type")]
        public bool isPlayer = false;
        public bool isEnemy = false;
        
        // ユニットイベント
        public static event Action<BattleUnit> OnUnitDefeated;
        public static event Action<BattleUnit> OnUnitAttacked;
        public static event Action<BattleUnit> OnUnitHealed;
        public static event Action<BattleUnit> OnUnitDamaged;
        
        /// <summary>
        /// ユニットを初期化
        /// </summary>
        /// <param name="name">ユニット名</param>
        /// <param name="hp">最大HP</param>
        /// <param name="atk">攻撃力</param>
        /// <param name="def">防御力</param>
        public void InitializeUnit(string name, int hp, int atk, int def)
        {
            unitName = name;
            maxHP = hp;
            currentHP = hp;
            attack = atk;
            defense = def;
            
            Debug.Log($"BattleUnit: ユニットを初期化しました - {unitName} (HP: {currentHP}/{maxHP}, ATK: {attack}, DEF: {defense})");
        }
        
        /// <summary>
        /// ダメージを受ける
        /// </summary>
        /// <param name="damage">受けるダメージ</param>
        public void TakeDamage(int damage)
        {
            if (currentHP <= 0) return;
            
            int actualDamage = Mathf.Max(1, damage - defense);
            currentHP = Mathf.Max(0, currentHP - actualDamage);
            
            OnUnitDamaged?.Invoke(this);
            
            Debug.Log($"{unitName}が{actualDamage}ダメージを受けました (HP: {currentHP}/{maxHP})");
            
            // HPが0になった場合
            if (currentHP <= 0)
            {
                OnUnitDefeated?.Invoke(this);
                Debug.Log($"{unitName}が倒されました");
            }
        }
        
        /// <summary>
        /// 攻撃する
        /// </summary>
        /// <param name="target">攻撃対象</param>
        public void Attack(BattleUnit target)
        {
            if (target == null || target.currentHP <= 0) return;
            
            OnUnitAttacked?.Invoke(this);
            target.TakeDamage(attack);
            
            Debug.Log($"{unitName}が{target.unitName}を攻撃しました");
        }
        
        /// <summary>
        /// 回復する
        /// </summary>
        /// <param name="healAmount">回復量</param>
        public void Heal(int healAmount)
        {
            if (currentHP <= 0) return;
            
            int oldHP = currentHP;
            currentHP = Mathf.Min(maxHP, currentHP + healAmount);
            int actualHeal = currentHP - oldHP;
            
            if (actualHeal > 0)
            {
                OnUnitHealed?.Invoke(this);
                Debug.Log($"{unitName}が{actualHeal}回復しました (HP: {currentHP}/{maxHP})");
            }
        }
        
        /// <summary>
        /// 敵の行動を実行（AI）
        /// </summary>
        public virtual void ExecuteEnemyAction()
        {
            if (!isEnemy) return;
            
            // 簡単なAI：ランダムに攻撃または回復
            float random = UnityEngine.Random.Range(0f, 1f);
            
            if (random < 0.7f) // 70%の確率で攻撃
            {
                // プレイヤーを探して攻撃
                BattleUnit player = FindObjectOfType<BattleUnit>();
                if (player != null && player.isPlayer)
                {
                    Attack(player);
                }
            }
            else // 30%の確率で回復
            {
                Heal(5);
            }
        }
        
        /// <summary>
        /// ユニットが生存しているかチェック
        /// </summary>
        /// <returns>生存している場合true</returns>
        public bool IsAlive()
        {
            return currentHP > 0;
        }
        
        /// <summary>
        /// ユニットの情報を取得
        /// </summary>
        /// <returns>ユニットの情報文字列</returns>
        public string GetUnitInfo()
        {
            return $"{unitName} - HP: {currentHP}/{maxHP}, ATK: {attack}, DEF: {defense}, Alive: {IsAlive()}";
        }
    }
} 