using UnityEngine;
using System;

namespace BattleSystemTemplate
{
    /// <summary>
    /// 戦闘開始専用コンポーネント
    /// 責務：戦闘の開始、終了、状態管理のみ
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class BattleStarter : MonoBehaviour
    {
        public static BattleStarter Instance { get; private set; }
        
        [Header("Battle Settings")]
        public BattleStateSO battleState;
        public BattleEventChannel battleEventChannel;
        
        [Header("Battle Units")]
        public BattleUnit playerUnit;
        public BattleUnit enemyUnit;
        
        [Header("Battle UI")]
        public BattleUI battleUI;
        
        // 戦闘状態
        private bool isBattleActive = false;
        private bool isPlayerTurn = true;
        
        // 戦闘イベント
        public static event Action<BattleStateSO> OnBattleStarted;
        public static event Action<BattleStateSO> OnBattleEnded;
        public static event Action<BattleUnit> OnUnitDefeated;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeBattleSystem();
        }
        
        /// <summary>
        /// 戦闘システムの初期化
        /// </summary>
        private void InitializeBattleSystem()
        {
            if (battleState == null)
            {
                Debug.LogError("BattleStarter: battleStateが設定されていません");
                return;
            }
            
            if (battleEventChannel == null)
            {
                Debug.LogError("BattleStarter: battleEventChannelが設定されていません");
                return;
            }
            
            SubscribeToEvents();
            
            Debug.Log("BattleStarter: 戦闘システムを初期化しました");
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (battleEventChannel != null)
            {
                battleEventChannel.OnBattleStartRequested.AddListener(StartBattle);
                battleEventChannel.OnBattleEndRequested.AddListener(EndBattle);
            }
        }
        
        /// <summary>
        /// 戦闘を開始
        /// </summary>
        public void StartBattle()
        {
            if (isBattleActive) return;
            
            isBattleActive = true;
            isPlayerTurn = true;
            
            // 戦闘状態を初期化
            if (battleState != null)
            {
                battleState.InitializeBattle(playerUnit, enemyUnit);
            }
            
            // UIを初期化
            if (battleUI != null)
            {
                battleUI.InitializeBattleUI(battleState);
            }
            
            OnBattleStarted?.Invoke(battleState);
            battleEventChannel?.BattleStarted(battleState);
            
            Debug.Log("BattleStarter: 戦闘を開始しました");
        }
        
        /// <summary>
        /// 戦闘を終了
        /// </summary>
        public void EndBattle()
        {
            if (!isBattleActive) return;
            
            isBattleActive = false;
            
            OnBattleEnded?.Invoke(battleState);
            battleEventChannel?.BattleEnded(battleState);
            
            Debug.Log("BattleStarter: 戦闘を終了しました");
        }
        
        /// <summary>
        /// プレイヤーターンを開始
        /// </summary>
        public void StartPlayerTurn()
        {
            if (!isBattleActive || isPlayerTurn) return;
            
            isPlayerTurn = true;
            
            if (battleUI != null)
            {
                battleUI.StartPlayerTurn();
            }
            
            Debug.Log("BattleStarter: プレイヤーターンを開始しました");
        }
        
        /// <summary>
        /// 敵ターンを開始
        /// </summary>
        public void StartEnemyTurn()
        {
            if (!isBattleActive || !isPlayerTurn) return;
            
            isPlayerTurn = false;
            
            if (enemyUnit != null)
            {
                enemyUnit.ExecuteEnemyAction();
            }
            
            Debug.Log("BattleStarter: 敵ターンを開始しました");
        }
        
        /// <summary>
        /// ユニットが倒された時の処理
        /// </summary>
        /// <param name="defeatedUnit">倒されたユニット</param>
        public void HandleUnitDefeated(BattleUnit defeatedUnit)
        {
            OnUnitDefeated?.Invoke(defeatedUnit);
            battleEventChannel?.UnitDefeated(defeatedUnit);
            
            // 戦闘終了判定
            if (defeatedUnit == playerUnit)
            {
                battleState.battleResult = BattleResult.EnemyVictory;
                EndBattle();
            }
            else if (defeatedUnit == enemyUnit)
            {
                battleState.battleResult = BattleResult.PlayerVictory;
                EndBattle();
            }
            
            Debug.Log($"BattleStarter: ユニットが倒されました - {defeatedUnit.unitName}");
        }
        
        /// <summary>
        /// 戦闘システムの情報を取得
        /// </summary>
        /// <returns>戦闘システムの情報文字列</returns>
        public string GetBattleSystemInfo()
        {
            return $"BattleSystem - Active: {isBattleActive}, PlayerTurn: {isPlayerTurn}, " +
                   $"State: {(battleState != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(battleEventChannel != null ? "✓" : "✗")}";
        }
    }
} 