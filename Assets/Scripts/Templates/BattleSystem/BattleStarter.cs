using UnityEngine;
using System;

namespace BattleSystem
{
    /// <summary>
    /// 戦闘開始専用コンポーネント
    /// 責務：戦闘の開始処理のみ
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class BattleStarter : MonoBehaviour
    {
        public static BattleStarter Instance { get; private set; }
        
        [Header("Battle Settings")]
        public BattleStateSO battleState;
        public BattleConfigSO battleConfig;
        
        [Header("Event Channels")]
        public BattleEventChannel battleEventChannel;
        
        // 戦闘開始イベント
        public static event Action<BattleStateSO> OnBattleStarted;
        public static event Action<BattleStateSO> OnBattleEnded;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }
        
        private void Start()
        {
            InitializeBattleState();
        }
        
        /// <summary>
        /// 戦闘状態の初期化
        /// </summary>
        private void InitializeBattleState()
        {
            if (battleState == null)
            {
                Debug.LogError("BattleStarter: battleStateが設定されていません");
                return;
            }
            
            battleState.Initialize();
            Debug.Log("BattleStarter: 戦闘状態を初期化しました");
        }
        
        /// <summary>
        /// 戦闘開始
        /// </summary>
        public void StartBattle()
        {
            if (battleState == null)
            {
                Debug.LogError("BattleStarter: battleStateが設定されていません");
                return;
            }
            
            battleState.SetBattlePhase(BattlePhase.PlayerTurn);
            OnBattleStarted?.Invoke(battleState);
            
            if (battleEventChannel != null)
            {
                battleEventChannel.RaiseBattleStarted(battleState);
            }
            
            Debug.Log("BattleStarter: 戦闘を開始しました");
        }
        
        /// <summary>
        /// 戦闘終了
        /// </summary>
        public void EndBattle(BattleResult result)
        {
            if (battleState == null) return;
            
            battleState.SetBattleResult(result);
            battleState.SetBattlePhase(BattlePhase.Ended);
            
            OnBattleEnded?.Invoke(battleState);
            
            if (battleEventChannel != null)
            {
                battleEventChannel.RaiseBattleEnded(battleState);
            }
            
            Debug.Log($"BattleStarter: 戦闘を終了しました - 結果: {result}");
        }
        
        /// <summary>
        /// 戦闘設定の検証
        /// </summary>
        public bool ValidateBattleSetup()
        {
            if (battleState == null)
            {
                Debug.LogError("BattleStarter: battleStateが設定されていません");
                return false;
            }
            
            if (battleConfig == null)
            {
                Debug.LogError("BattleStarter: battleConfigが設定されていません");
                return false;
            }
            
            if (battleEventChannel == null)
            {
                Debug.LogError("BattleStarter: battleEventChannelが設定されていません");
                return false;
            }
            
            return true;
        }

        /// <summary>
        /// 敵撃破時の処理（GameManager統合用）
        /// </summary>
        public void HandleEnemyDefeated()
        {
            if (battleState == null) return;
            
            // スコア加算
            if (GameManager.Instance != null)
            {
                GameManager.Instance.score += 100;
                GameManager.Instance.AddPlayerExp(10);
            }
            
            // ログ出力
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"敵を倒した！スコア: {GameManager.Instance?.score ?? 0}");
            }
            
            // 戦闘システムイベント発火
            if (battleEventChannel != null)
            {
                battleEventChannel.RaiseUnitDefeated(battleState);
            }
            
            Debug.Log("BattleStarter: 敵撃破処理を実行しました");
        }

        /// <summary>
        /// プレイヤー勝利時の処理（GameManager統合用）
        /// </summary>
        public void HandlePlayerVictory()
        {
            EndBattle(BattleResult.PlayerVictory);
            
            // GameManagerの勝利処理を呼び出し
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameClear();
            }
        }

        /// <summary>
        /// プレイヤー敗北時の処理（GameManager統合用）
        /// </summary>
        public void HandlePlayerDefeat()
        {
            EndBattle(BattleResult.EnemyVictory);
            
            // GameManagerの敗北処理を呼び出し
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GameOver();
            }
        }

        /// <summary>
        /// 戦闘システムの情報を取得
        /// </summary>
        public string GetBattleSystemInfo()
        {
            return $"BattleStarter - State: {(battleState != null ? "✓" : "✗")}, " +
                   $"Config: {(battleConfig != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(battleEventChannel != null ? "✓" : "✗")}";
        }
    }
} 