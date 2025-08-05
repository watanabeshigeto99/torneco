using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleSystem
{
    /// <summary>
    /// 戦闘システムテンプレートの動作確認用テスト
    /// 責務：テンプレートの単体テストのみ
    /// </summary>
    public class BattleSystemTest : MonoBehaviour
    {
        [Header("Battle System Components")]
        public BattleStarter battleStarter;
        public BattleStateSO battleState;
        public BattleConfigSO battleConfig;
        public BattleEventChannel battleEventChannel;
        
        [Header("Test UI")]
        public Button startBattleButton;
        public Button endBattleButton;
        public Button damagePlayerButton;
        public Button healPlayerButton;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI logText;
        
        [Header("Test Units")]
        public BattleUnit testPlayerUnit;
        public BattleUnit testEnemyUnit;
        
        private void Start()
        {
            SetupTestUI();
            SubscribeToEvents();
            ValidateTestSetup();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// テストUIの設定
        /// </summary>
        private void SetupTestUI()
        {
            if (startBattleButton != null)
            {
                startBattleButton.onClick.AddListener(OnStartBattleClicked);
            }
            
            if (endBattleButton != null)
            {
                endBattleButton.onClick.AddListener(OnEndBattleClicked);
            }
            
            if (damagePlayerButton != null)
            {
                damagePlayerButton.onClick.AddListener(OnDamagePlayerClicked);
            }
            
            if (healPlayerButton != null)
            {
                healPlayerButton.onClick.AddListener(OnHealPlayerClicked);
            }
            
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (battleEventChannel != null)
            {
                battleEventChannel.OnBattleStarted.AddListener(OnBattleStarted);
                battleEventChannel.OnBattleEnded.AddListener(OnBattleEnded);
                battleEventChannel.OnUnitDamaged.AddListener(OnUnitDamaged);
                battleEventChannel.OnUnitHealed.AddListener(OnUnitHealed);
                battleEventChannel.OnUnitDied.AddListener(OnUnitDied);
            }
        }
        
        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (battleEventChannel != null)
            {
                battleEventChannel.OnBattleStarted.RemoveListener(OnBattleStarted);
                battleEventChannel.OnBattleEnded.RemoveListener(OnBattleEnded);
                battleEventChannel.OnUnitDamaged.RemoveListener(OnUnitDamaged);
                battleEventChannel.OnUnitHealed.RemoveListener(OnUnitHealed);
                battleEventChannel.OnUnitDied.RemoveListener(OnUnitDied);
            }
        }
        
        /// <summary>
        /// テスト設定の検証
        /// </summary>
        private void ValidateTestSetup()
        {
            bool isValid = true;
            
            if (battleStarter == null)
            {
                Debug.LogError("BattleSystemTest: battleStarterが設定されていません");
                isValid = false;
            }
            
            if (battleState == null)
            {
                Debug.LogError("BattleSystemTest: battleStateが設定されていません");
                isValid = false;
            }
            
            if (battleConfig == null)
            {
                Debug.LogError("BattleSystemTest: battleConfigが設定されていません");
                isValid = false;
            }
            
            if (battleEventChannel == null)
            {
                Debug.LogError("BattleSystemTest: battleEventChannelが設定されていません");
                isValid = false;
            }
            
            if (testPlayerUnit == null)
            {
                Debug.LogError("BattleSystemTest: testPlayerUnitが設定されていません");
                isValid = false;
            }
            
            if (testEnemyUnit == null)
            {
                Debug.LogError("BattleSystemTest: testEnemyUnitが設定されていません");
                isValid = false;
            }
            
            if (isValid)
            {
                Debug.Log("BattleSystemTest: テスト設定の検証が完了しました");
                AddLog("テスト設定の検証が完了しました");
            }
            else
            {
                Debug.LogError("BattleSystemTest: テスト設定に問題があります");
                AddLog("テスト設定に問題があります");
            }
        }
        
        /// <summary>
        /// 戦闘開始ボタンクリック
        /// </summary>
        private void OnStartBattleClicked()
        {
            if (battleStarter != null)
            {
                // テストユニットを設定
                if (battleState != null)
                {
                    battleState.SetPlayerUnit(testPlayerUnit);
                    battleState.SetEnemyUnits(new BattleUnit[] { testEnemyUnit });
                }
                
                battleStarter.StartBattle();
                AddLog("戦闘を開始しました");
            }
        }
        
        /// <summary>
        /// 戦闘終了ボタンクリック
        /// </summary>
        private void OnEndBattleClicked()
        {
            if (battleStarter != null)
            {
                battleStarter.EndBattle(BattleResult.PlayerVictory);
                AddLog("戦闘を終了しました（プレイヤー勝利）");
            }
        }
        
        /// <summary>
        /// プレイヤーダメージボタンクリック
        /// </summary>
        private void OnDamagePlayerClicked()
        {
            if (testPlayerUnit != null)
            {
                testPlayerUnit.TakeDamage(5);
                AddLog("プレイヤーに5ダメージを与えました");
            }
        }
        
        /// <summary>
        /// プレイヤー回復ボタンクリック
        /// </summary>
        private void OnHealPlayerClicked()
        {
            if (testPlayerUnit != null)
            {
                testPlayerUnit.Heal(3);
                AddLog("プレイヤーを3回復しました");
            }
        }
        
        /// <summary>
        /// 戦闘開始イベントハンドラー
        /// </summary>
        private void OnBattleStarted(BattleStateSO state)
        {
            AddLog($"戦闘開始イベントを受信 - フェーズ: {state.currentPhase}");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// 戦闘終了イベントハンドラー
        /// </summary>
        private void OnBattleEnded(BattleStateSO state)
        {
            AddLog($"戦闘終了イベントを受信 - 結果: {state.battleResult}");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// ユニットダメージイベントハンドラー
        /// </summary>
        private void OnUnitDamaged(BattleUnit unit)
        {
            AddLog($"{unit.unitName}がダメージを受けました - HP: {unit.currentHP}/{unit.maxHP}");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// ユニット回復イベントハンドラー
        /// </summary>
        private void OnUnitHealed(BattleUnit unit)
        {
            AddLog($"{unit.unitName}が回復しました - HP: {unit.currentHP}/{unit.maxHP}");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// ユニット死亡イベントハンドラー
        /// </summary>
        private void OnUnitDied(BattleUnit unit)
        {
            AddLog($"{unit.unitName}が死亡しました");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;
            
            string status = "=== 戦闘システムテスト ===\n";
            
            if (battleState != null)
            {
                status += $"戦闘フェーズ: {battleState.currentPhase}\n";
                status += $"戦闘結果: {battleState.battleResult}\n";
                status += $"ターン: {battleState.currentTurn}\n";
                status += $"プレイヤーターン: {battleState.isPlayerTurn}\n";
            }
            
            if (testPlayerUnit != null)
            {
                status += $"\nプレイヤー: {testPlayerUnit.GetUnitInfo()}\n";
            }
            
            if (testEnemyUnit != null)
            {
                status += $"敵: {testEnemyUnit.GetUnitInfo()}\n";
            }
            
            statusText.text = status;
        }
        
        /// <summary>
        /// ログの追加
        /// </summary>
        private void AddLog(string message)
        {
            if (logText == null) return;
            
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}\n";
            logText.text = logEntry + logText.text;
            
            // 最大行数を制限
            string[] lines = logText.text.Split('\n');
            if (lines.Length > 10)
            {
                logText.text = string.Join("\n", lines, 0, 10);
            }
        }
    }
} 