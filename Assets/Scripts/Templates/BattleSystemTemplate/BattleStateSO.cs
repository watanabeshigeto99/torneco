using UnityEngine;
using System;

namespace BattleSystemTemplate
{
    /// <summary>
    /// 戦闘結果の列挙型
    /// </summary>
    public enum BattleResult
    {
        None,
        PlayerVictory,
        EnemyVictory,
        Draw
    }
    
    /// <summary>
    /// 戦闘状態を管理するScriptableObject
    /// 責務：戦闘の状態データのみ
    /// </summary>
    [CreateAssetMenu(fileName = "BattleState", menuName = "BattleSystem/BattleState")]
    public class BattleStateSO : ScriptableObject
    {
        [Header("Battle State")]
        public BattleResult battleResult = BattleResult.None;
        public bool isBattleActive = false;
        public bool isPlayerTurn = true;
        
        [Header("Battle Units")]
        public BattleUnit playerUnit;
        public BattleUnit enemyUnit;
        
        [Header("Battle Data")]
        public int turnCount = 0;
        public float battleStartTime = 0f;
        public float battleEndTime = 0f;
        
        // 戦闘状態変更イベント
        public static event Action<BattleStateSO> OnBattleStateChanged;
        public static event Action<BattleStateSO> OnBattleResultChanged;
        
        /// <summary>
        /// 戦闘を初期化
        /// </summary>
        /// <param name="player">プレイヤーユニット</param>
        /// <param name="enemy">敵ユニット</param>
        public void InitializeBattle(BattleUnit player, BattleUnit enemy)
        {
            playerUnit = player;
            enemyUnit = enemy;
            battleResult = BattleResult.None;
            isBattleActive = true;
            isPlayerTurn = true;
            turnCount = 0;
            battleStartTime = Time.time;
            battleEndTime = 0f;
            
            OnBattleStateChanged?.Invoke(this);
            
            Debug.Log("BattleStateSO: 戦闘を初期化しました");
        }
        
        /// <summary>
        /// 戦闘を終了
        /// </summary>
        /// <param name="result">戦闘結果</param>
        public void EndBattle(BattleResult result)
        {
            battleResult = result;
            isBattleActive = false;
            battleEndTime = Time.time;
            
            OnBattleResultChanged?.Invoke(this);
            OnBattleStateChanged?.Invoke(this);
            
            Debug.Log($"BattleStateSO: 戦闘を終了しました - {result}");
        }
        
        /// <summary>
        /// ターンを進行
        /// </summary>
        public void AdvanceTurn()
        {
            if (!isBattleActive) return;
            
            turnCount++;
            isPlayerTurn = !isPlayerTurn;
            
            OnBattleStateChanged?.Invoke(this);
            
            Debug.Log($"BattleStateSO: ターンを進行しました - ターン{turnCount}, プレイヤーターン: {isPlayerTurn}");
        }
        
        /// <summary>
        /// 戦闘時間を取得
        /// </summary>
        /// <returns>戦闘時間（秒）</returns>
        public float GetBattleTime()
        {
            if (!isBattleActive)
            {
                return battleEndTime - battleStartTime;
            }
            else
            {
                return Time.time - battleStartTime;
            }
        }
        
        /// <summary>
        /// 戦闘状態の情報を取得
        /// </summary>
        /// <returns>戦闘状態の情報文字列</returns>
        public string GetBattleStateInfo()
        {
            return $"BattleState - Result: {battleResult}, Active: {isBattleActive}, " +
                   $"PlayerTurn: {isPlayerTurn}, Turn: {turnCount}, " +
                   $"Time: {GetBattleTime():F1}s";
        }
    }
} 