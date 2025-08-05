using UnityEngine;
using System;

namespace BattleSystem
{
    /// <summary>
    /// 戦闘状態を管理するScriptableObject
    /// 責務：戦闘の状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "BattleState", menuName = "Battle System/Battle State")]
    public class BattleStateSO : ScriptableObject
    {
        [Header("Battle State")]
        public BattlePhase currentPhase = BattlePhase.None;
        public BattleResult battleResult = BattleResult.None;
        
        [Header("Turn Information")]
        public int currentTurn = 0;
        public bool isPlayerTurn = true;
        
        [Header("Battle Participants")]
        public BattleUnit playerUnit;
        public BattleUnit[] enemyUnits;
        
        // 状態変更イベント
        public event Action<BattlePhase> OnPhaseChanged;
        public event Action<BattleResult> OnBattleResultChanged;
        public event Action<int> OnTurnChanged;
        public event Action<bool> OnTurnOwnerChanged;
        
        /// <summary>
        /// 戦闘状態の初期化
        /// </summary>
        public void Initialize()
        {
            currentPhase = BattlePhase.None;
            battleResult = BattleResult.None;
            currentTurn = 0;
            isPlayerTurn = true;
            
            Debug.Log("BattleStateSO: 戦闘状態を初期化しました");
        }
        
        /// <summary>
        /// 戦闘フェーズの設定
        /// </summary>
        public void SetBattlePhase(BattlePhase newPhase)
        {
            if (currentPhase == newPhase) return;
            
            currentPhase = newPhase;
            OnPhaseChanged?.Invoke(newPhase);
            
            Debug.Log($"BattleStateSO: 戦闘フェーズを変更しました - {newPhase}");
        }
        
        /// <summary>
        /// 戦闘結果の設定
        /// </summary>
        public void SetBattleResult(BattleResult result)
        {
            battleResult = result;
            OnBattleResultChanged?.Invoke(result);
            
            Debug.Log($"BattleStateSO: 戦闘結果を設定しました - {result}");
        }
        
        /// <summary>
        /// ターンの進行
        /// </summary>
        public void AdvanceTurn()
        {
            currentTurn++;
            OnTurnChanged?.Invoke(currentTurn);
            
            Debug.Log($"BattleStateSO: ターンを進行しました - {currentTurn}");
        }
        
        /// <summary>
        /// ターン所有者の変更
        /// </summary>
        public void SetTurnOwner(bool isPlayer)
        {
            isPlayerTurn = isPlayer;
            OnTurnOwnerChanged?.Invoke(isPlayer);
            
            Debug.Log($"BattleStateSO: ターン所有者を変更しました - {(isPlayer ? "プレイヤー" : "敵")}");
        }
        
        /// <summary>
        /// プレイヤーユニットの設定
        /// </summary>
        public void SetPlayerUnit(BattleUnit unit)
        {
            playerUnit = unit;
            Debug.Log("BattleStateSO: プレイヤーユニットを設定しました");
        }
        
        /// <summary>
        /// 敵ユニットの設定
        /// </summary>
        public void SetEnemyUnits(BattleUnit[] units)
        {
            enemyUnits = units;
            Debug.Log($"BattleStateSO: 敵ユニットを設定しました - {units.Length}体");
        }
        
        /// <summary>
        /// 戦闘が終了しているかチェック
        /// </summary>
        public bool IsBattleEnded()
        {
            return currentPhase == BattlePhase.Ended;
        }
        
        /// <summary>
        /// プレイヤーの勝利条件チェック
        /// </summary>
        public bool IsPlayerVictory()
        {
            if (enemyUnits == null) return false;
            
            foreach (var enemy in enemyUnits)
            {
                if (enemy != null && enemy.IsAlive())
                {
                    return false;
                }
            }
            return true;
        }
        
        /// <summary>
        /// 敵の勝利条件チェック
        /// </summary>
        public bool IsEnemyVictory()
        {
            return playerUnit != null && !playerUnit.IsAlive();
        }
    }
    
    /// <summary>
    /// 戦闘フェーズ
    /// </summary>
    public enum BattlePhase
    {
        None,
        PlayerTurn,
        EnemyTurn,
        CardSelection,
        ActionExecution,
        Ended
    }
    
    /// <summary>
    /// 戦闘結果
    /// </summary>
    public enum BattleResult
    {
        None,
        PlayerVictory,
        EnemyVictory,
        Draw
    }
} 