using UnityEngine;
using UnityEngine.Events;

namespace PlayerDataSystem
{
    /// <summary>
    /// プレイヤーデータ専用イベントチャンネル
    /// 責務：プレイヤー関連のイベント管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerEventChannel", menuName = "Player Data System/Player Event Channel")]
    public class PlayerEventChannel : ScriptableObject
    {
        // プレイヤーデータ変更イベント
        [Header("Player Data Events")]
        public UnityEvent<PlayerDataSO> OnPlayerDataChanged;
        public UnityEvent<int> OnPlayerLevelUp;
        public UnityEvent<int> OnPlayerExpGained;
        public UnityEvent<int, int> OnPlayerHPChanged;
        
        // プレイヤーアクションイベント
        [Header("Player Action Events")]
        public UnityEvent<int> OnPlayerDamaged;
        public UnityEvent<int> OnPlayerHealed;
        public UnityEvent<int> OnPlayerGoldChanged;
        public UnityEvent OnPlayerDied;
        
        // プレイヤー統計イベント
        [Header("Player Stats Events")]
        public UnityEvent<int> OnPlayerAttackChanged;
        public UnityEvent<int> OnPlayerDefenseChanged;
        public UnityEvent<float> OnPlayerHPPercentageChanged;
        public UnityEvent<float> OnPlayerExpPercentageChanged;
        
        /// <summary>
        /// プレイヤーデータ変更イベントを発火
        /// </summary>
        public void RaisePlayerDataChanged(PlayerDataSO playerData)
        {
            OnPlayerDataChanged?.Invoke(playerData);
            Debug.Log($"[PlayerEventChannel] Player data changed: {playerData?.playerName ?? "Unknown"}");
        }
        
        /// <summary>
        /// プレイヤーレベルアップイベントを発火
        /// </summary>
        public void RaisePlayerLevelUp(int newLevel)
        {
            OnPlayerLevelUp?.Invoke(newLevel);
            Debug.Log($"[PlayerEventChannel] Player leveled up to level {newLevel}");
        }
        
        /// <summary>
        /// プレイヤー経験値獲得イベントを発火
        /// </summary>
        public void RaisePlayerExpGained(int expAmount)
        {
            OnPlayerExpGained?.Invoke(expAmount);
            Debug.Log($"[PlayerEventChannel] Player gained {expAmount} experience");
        }
        
        /// <summary>
        /// プレイヤーHP変更イベントを発火
        /// </summary>
        public void RaisePlayerHPChanged(int currentHP, int maxHP)
        {
            OnPlayerHPChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"[PlayerEventChannel] Player HP changed: {currentHP}/{maxHP}");
        }
        
        /// <summary>
        /// プレイヤーダメージイベントを発火
        /// </summary>
        public void RaisePlayerDamaged(int damage)
        {
            OnPlayerDamaged?.Invoke(damage);
            Debug.Log($"[PlayerEventChannel] Player took {damage} damage");
        }
        
        /// <summary>
        /// プレイヤー回復イベントを発火
        /// </summary>
        public void RaisePlayerHealed(int healAmount)
        {
            OnPlayerHealed?.Invoke(healAmount);
            Debug.Log($"[PlayerEventChannel] Player healed {healAmount} HP");
        }
        
        /// <summary>
        /// プレイヤーゴールド変更イベントを発火
        /// </summary>
        public void RaisePlayerGoldChanged(int newGold)
        {
            OnPlayerGoldChanged?.Invoke(newGold);
            Debug.Log($"[PlayerEventChannel] Player gold changed to {newGold}");
        }
        
        /// <summary>
        /// プレイヤー死亡イベントを発火
        /// </summary>
        public void RaisePlayerDied()
        {
            OnPlayerDied?.Invoke();
            Debug.Log("[PlayerEventChannel] Player died");
        }
        
        /// <summary>
        /// プレイヤー攻撃力変更イベントを発火
        /// </summary>
        public void RaisePlayerAttackChanged(int newAttack)
        {
            OnPlayerAttackChanged?.Invoke(newAttack);
            Debug.Log($"[PlayerEventChannel] Player attack changed to {newAttack}");
        }
        
        /// <summary>
        /// プレイヤー防御力変更イベントを発火
        /// </summary>
        public void RaisePlayerDefenseChanged(int newDefense)
        {
            OnPlayerDefenseChanged?.Invoke(newDefense);
            Debug.Log($"[PlayerEventChannel] Player defense changed to {newDefense}");
        }
        
        /// <summary>
        /// プレイヤーHP割合変更イベントを発火
        /// </summary>
        public void RaisePlayerHPPercentageChanged(float percentage)
        {
            OnPlayerHPPercentageChanged?.Invoke(percentage);
            Debug.Log($"[PlayerEventChannel] Player HP percentage changed to {percentage:P1}");
        }
        
        /// <summary>
        /// プレイヤー経験値割合変更イベントを発火
        /// </summary>
        public void RaisePlayerExpPercentageChanged(float percentage)
        {
            OnPlayerExpPercentageChanged?.Invoke(percentage);
            Debug.Log($"[PlayerEventChannel] Player experience percentage changed to {percentage:P1}");
        }
        
        /// <summary>
        /// イベントチャンネルの情報を取得
        /// </summary>
        public string GetEventChannelInfo()
        {
            return $"PlayerEventChannel - Events: {OnPlayerDataChanged?.GetPersistentEventCount() ?? 0} data, " +
                   $"{OnPlayerLevelUp?.GetPersistentEventCount() ?? 0} level, " +
                   $"{OnPlayerHPChanged?.GetPersistentEventCount() ?? 0} HP listeners";
        }
    }
} 