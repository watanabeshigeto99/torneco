using UnityEngine;
using UnityEngine.Events;
using System;

namespace BattleSystem
{
    /// <summary>
    /// 戦闘システム専用イベントチャンネル
    /// 責務：戦闘関連のイベント管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "BattleEventChannel", menuName = "Battle System/Battle Event Channel")]
    public class BattleEventChannel : ScriptableObject
    {
        // 戦闘開始・終了イベント
        [Header("Battle Lifecycle Events")]
        public UnityEvent<BattleStateSO> OnBattleStarted;
        public UnityEvent<BattleStateSO> OnBattleEnded;
        
        // ターン管理イベント
        [Header("Turn Management Events")]
        public UnityEvent<int> OnTurnChanged;
        public UnityEvent<bool> OnTurnOwnerChanged;
        public UnityEvent<BattlePhase> OnPhaseChanged;
        
        // ユニット状態イベント
        [Header("Unit State Events")]
        public UnityEvent<BattleUnit> OnUnitDamaged;
        public UnityEvent<BattleUnit> OnUnitHealed;
        public UnityEvent<BattleUnit> OnUnitDied;
        public UnityEvent<BattleUnit> OnUnitStunned;
        public UnityEvent<BattleUnit> OnUnitRecovered;
        public UnityEvent<BattleStateSO> OnUnitDefeated;
        
        // カード関連イベント
        [Header("Card Events")]
        public UnityEvent<CardDataSO> OnCardPlayed;
        public UnityEvent<CardDataSO> OnCardDrawn;
        public UnityEvent<CardDataSO> OnCardDiscarded;
        
        // アクション実行イベント
        [Header("Action Events")]
        public UnityEvent<BattleUnit, BattleUnit, int> OnAttackExecuted;
        public UnityEvent<BattleUnit, int> OnHealExecuted;
        public UnityEvent<BattleUnit, int> OnStunExecuted;
        
        /// <summary>
        /// 戦闘開始イベントの発火
        /// </summary>
        public void RaiseBattleStarted(BattleStateSO battleState)
        {
            OnBattleStarted?.Invoke(battleState);
            Debug.Log("BattleEventChannel: 戦闘開始イベントを発火しました");
        }
        
        /// <summary>
        /// 戦闘終了イベントの発火
        /// </summary>
        public void RaiseBattleEnded(BattleStateSO battleState)
        {
            OnBattleEnded?.Invoke(battleState);
            Debug.Log("BattleEventChannel: 戦闘終了イベントを発火しました");
        }
        
        /// <summary>
        /// ターン変更イベントの発火
        /// </summary>
        public void RaiseTurnChanged(int turnNumber)
        {
            OnTurnChanged?.Invoke(turnNumber);
            Debug.Log($"BattleEventChannel: ターン変更イベントを発火しました - ターン{turnNumber}");
        }
        
        /// <summary>
        /// ターン所有者変更イベントの発火
        /// </summary>
        public void RaiseTurnOwnerChanged(bool isPlayerTurn)
        {
            OnTurnOwnerChanged?.Invoke(isPlayerTurn);
            Debug.Log($"BattleEventChannel: ターン所有者変更イベントを発火しました - {(isPlayerTurn ? "プレイヤー" : "敵")}");
        }
        
        /// <summary>
        /// フェーズ変更イベントの発火
        /// </summary>
        public void RaisePhaseChanged(BattlePhase newPhase)
        {
            OnPhaseChanged?.Invoke(newPhase);
            Debug.Log($"BattleEventChannel: フェーズ変更イベントを発火しました - {newPhase}");
        }
        
        /// <summary>
        /// ユニットダメージイベントの発火
        /// </summary>
        public void RaiseUnitDamaged(BattleUnit unit)
        {
            OnUnitDamaged?.Invoke(unit);
            Debug.Log($"BattleEventChannel: ユニットダメージイベントを発火しました - {unit.unitName}");
        }
        
        /// <summary>
        /// ユニット回復イベントの発火
        /// </summary>
        public void RaiseUnitHealed(BattleUnit unit)
        {
            OnUnitHealed?.Invoke(unit);
            Debug.Log($"BattleEventChannel: ユニット回復イベントを発火しました - {unit.unitName}");
        }
        
        /// <summary>
        /// ユニット死亡イベントの発火
        /// </summary>
        public void RaiseUnitDied(BattleUnit unit)
        {
            OnUnitDied?.Invoke(unit);
            Debug.Log($"BattleEventChannel: ユニット死亡イベントを発火しました - {unit.unitName}");
        }
        
        /// <summary>
        /// ユニットスタンイベントの発火
        /// </summary>
        public void RaiseUnitStunned(BattleUnit unit)
        {
            OnUnitStunned?.Invoke(unit);
            Debug.Log($"BattleEventChannel: ユニットスタンイベントを発火しました - {unit.unitName}");
        }
        
        /// <summary>
        /// ユニット回復イベントの発火
        /// </summary>
        public void RaiseUnitRecovered(BattleUnit unit)
        {
            OnUnitRecovered?.Invoke(unit);
            Debug.Log($"BattleEventChannel: ユニット回復イベントを発火しました - {unit.unitName}");
        }
        
        /// <summary>
        /// ユニット撃破イベントの発火
        /// </summary>
        public void RaiseUnitDefeated(BattleStateSO battleState)
        {
            OnUnitDefeated?.Invoke(battleState);
            Debug.Log("BattleEventChannel: ユニット撃破イベントを発火しました");
        }
        
        /// <summary>
        /// カード使用イベントの発火
        /// </summary>
        public void RaiseCardPlayed(CardDataSO card)
        {
            OnCardPlayed?.Invoke(card);
            Debug.Log($"BattleEventChannel: カード使用イベントを発火しました - {card.cardName}");
        }
        
        /// <summary>
        /// カードドローイベントの発火
        /// </summary>
        public void RaiseCardDrawn(CardDataSO card)
        {
            OnCardDrawn?.Invoke(card);
            Debug.Log($"BattleEventChannel: カードドローイベントを発火しました - {card.cardName}");
        }
        
        /// <summary>
        /// カード破棄イベントの発火
        /// </summary>
        public void RaiseCardDiscarded(CardDataSO card)
        {
            OnCardDiscarded?.Invoke(card);
            Debug.Log($"BattleEventChannel: カード破棄イベントを発火しました - {card.cardName}");
        }
        
        /// <summary>
        /// 攻撃実行イベントの発火
        /// </summary>
        public void RaiseAttackExecuted(BattleUnit attacker, BattleUnit target, int damage)
        {
            OnAttackExecuted?.Invoke(attacker, target, damage);
            Debug.Log($"BattleEventChannel: 攻撃実行イベントを発火しました - {attacker.unitName} → {target.unitName} ({damage}ダメージ)");
        }
        
        /// <summary>
        /// 回復実行イベントの発火
        /// </summary>
        public void RaiseHealExecuted(BattleUnit target, int healAmount)
        {
            OnHealExecuted?.Invoke(target, healAmount);
            Debug.Log($"BattleEventChannel: 回復実行イベントを発火しました - {target.unitName} ({healAmount}回復)");
        }
        
        /// <summary>
        /// スタン実行イベントの発火
        /// </summary>
        public void RaiseStunExecuted(BattleUnit target, int stunTurns)
        {
            OnStunExecuted?.Invoke(target, stunTurns);
            Debug.Log($"BattleEventChannel: スタン実行イベントを発火しました - {target.unitName} ({stunTurns}ターン)");
        }
    }
} 