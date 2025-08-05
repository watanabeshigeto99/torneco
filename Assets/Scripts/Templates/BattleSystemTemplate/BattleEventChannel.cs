using UnityEngine;
using UnityEngine.Events;

namespace BattleSystemTemplate
{
    /// <summary>
    /// 戦闘システムのイベントチャンネル
    /// 責務：戦闘システム関連のイベント管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "BattleEventChannel", menuName = "BattleSystem/BattleEventChannel")]
    public class BattleEventChannel : ScriptableObject
    {
        [Header("Battle Control Events")]
        public UnityEvent OnBattleStartRequested = new UnityEvent();
        public UnityEvent OnBattleEndRequested = new UnityEvent();
        
        [Header("Battle State Events")]
        public UnityEvent<BattleStateSO> OnBattleStarted = new UnityEvent<BattleStateSO>();
        public UnityEvent<BattleStateSO> OnBattleEnded = new UnityEvent<BattleStateSO>();
        public UnityEvent<BattleStateSO> OnBattleStateChanged = new UnityEvent<BattleStateSO>();
        
        [Header("Unit Events")]
        public UnityEvent<BattleUnit> OnUnitDefeated = new UnityEvent<BattleUnit>();
        public UnityEvent<BattleUnit> OnUnitAttacked = new UnityEvent<BattleUnit>();
        public UnityEvent<BattleUnit> OnUnitHealed = new UnityEvent<BattleUnit>();
        
        [Header("Turn Events")]
        public UnityEvent OnPlayerTurnStarted = new UnityEvent();
        public UnityEvent OnEnemyTurnStarted = new UnityEvent();
        public UnityEvent OnTurnEnded = new UnityEvent();
        
        /// <summary>
        /// 戦闘開始要求を発火
        /// </summary>
        public void RequestBattleStart()
        {
            OnBattleStartRequested?.Invoke();
        }
        
        /// <summary>
        /// 戦闘終了要求を発火
        /// </summary>
        public void RequestBattleEnd()
        {
            OnBattleEndRequested?.Invoke();
        }
        
        /// <summary>
        /// 戦闘開始を発火
        /// </summary>
        /// <param name="battleState">戦闘状態</param>
        public void BattleStarted(BattleStateSO battleState)
        {
            OnBattleStarted?.Invoke(battleState);
        }
        
        /// <summary>
        /// 戦闘終了を発火
        /// </summary>
        /// <param name="battleState">戦闘状態</param>
        public void BattleEnded(BattleStateSO battleState)
        {
            OnBattleEnded?.Invoke(battleState);
        }
        
        /// <summary>
        /// 戦闘状態変更を発火
        /// </summary>
        /// <param name="battleState">戦闘状態</param>
        public void BattleStateChanged(BattleStateSO battleState)
        {
            OnBattleStateChanged?.Invoke(battleState);
        }
        
        /// <summary>
        /// ユニット撃破を発火
        /// </summary>
        /// <param name="unit">撃破されたユニット</param>
        public void UnitDefeated(BattleUnit unit)
        {
            OnUnitDefeated?.Invoke(unit);
        }
        
        /// <summary>
        /// ユニット攻撃を発火
        /// </summary>
        /// <param name="unit">攻撃したユニット</param>
        public void UnitAttacked(BattleUnit unit)
        {
            OnUnitAttacked?.Invoke(unit);
        }
        
        /// <summary>
        /// ユニット回復を発火
        /// </summary>
        /// <param name="unit">回復したユニット</param>
        public void UnitHealed(BattleUnit unit)
        {
            OnUnitHealed?.Invoke(unit);
        }
        
        /// <summary>
        /// プレイヤーターン開始を発火
        /// </summary>
        public void PlayerTurnStarted()
        {
            OnPlayerTurnStarted?.Invoke();
        }
        
        /// <summary>
        /// 敵ターン開始を発火
        /// </summary>
        public void EnemyTurnStarted()
        {
            OnEnemyTurnStarted?.Invoke();
        }
        
        /// <summary>
        /// ターン終了を発火
        /// </summary>
        public void TurnEnded()
        {
            OnTurnEnded?.Invoke();
        }
    }
} 