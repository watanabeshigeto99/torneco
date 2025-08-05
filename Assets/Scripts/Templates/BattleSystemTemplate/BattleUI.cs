using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BattleSystemTemplate
{
    /// <summary>
    /// 戦闘UI専用コンポーネント
    /// 責務：戦闘画面のUI表示のみ
    /// </summary>
    public class BattleUI : MonoBehaviour
    {
        [Header("Battle UI Elements")]
        public TextMeshProUGUI battleStatusText;
        public TextMeshProUGUI turnText;
        public TextMeshProUGUI playerHPText;
        public TextMeshProUGUI enemyHPText;
        public TextMeshProUGUI logText;
        
        [Header("Battle Buttons")]
        public Button attackButton;
        public Button healButton;
        public Button endTurnButton;
        
        [Header("Unit Displays")]
        public GameObject playerUnitDisplay;
        public GameObject enemyUnitDisplay;
        
        private BattleStateSO currentBattleState;
        private BattleUnit playerUnit;
        private BattleUnit enemyUnit;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            // ボタンの初期設定
            if (attackButton != null)
                attackButton.onClick.AddListener(OnAttackButtonClicked);
            if (healButton != null)
                healButton.onClick.AddListener(OnHealButtonClicked);
            if (endTurnButton != null)
                endTurnButton.onClick.AddListener(OnEndTurnButtonClicked);
            
            // 初期状態ではボタンを無効化
            SetButtonInteractable(false);
            
            Debug.Log("BattleUI: UIを初期化しました");
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            // 戦闘ユニットイベント
            BattleUnit.OnUnitDefeated += OnUnitDefeated;
            BattleUnit.OnUnitAttacked += OnUnitAttacked;
            BattleUnit.OnUnitHealed += OnUnitHealed;
        }
        
        /// <summary>
        /// 戦闘UIを初期化
        /// </summary>
        /// <param name="battleState">戦闘状態</param>
        public void InitializeBattleUI(BattleStateSO battleState)
        {
            currentBattleState = battleState;
            playerUnit = battleState.playerUnit;
            enemyUnit = battleState.enemyUnit;
            
            UpdateAllUI();
            SetButtonInteractable(true);
            
            AddLog("戦闘開始！");
            
            Debug.Log("BattleUI: 戦闘UIを初期化しました");
        }
        
        /// <summary>
        /// プレイヤーターンを開始
        /// </summary>
        public void StartPlayerTurn()
        {
            SetButtonInteractable(true);
            UpdateTurnText("プレイヤーターン");
            AddLog("プレイヤーのターンです");
        }
        
        /// <summary>
        /// 全UIを更新
        /// </summary>
        private void UpdateAllUI()
        {
            UpdateBattleStatus();
            UpdateUnitDisplays();
            UpdateTurnText();
        }
        
        /// <summary>
        /// 戦闘状態を更新
        /// </summary>
        private void UpdateBattleStatus()
        {
            if (battleStatusText != null && currentBattleState != null)
            {
                string status = $"戦闘中 - ターン: {currentBattleState.turnCount}";
                battleStatusText.text = status;
            }
        }
        
        /// <summary>
        /// ユニット表示を更新
        /// </summary>
        private void UpdateUnitDisplays()
        {
            if (playerHPText != null && playerUnit != null)
            {
                playerHPText.text = $"プレイヤー: {playerUnit.currentHP}/{playerUnit.maxHP}";
            }
            
            if (enemyHPText != null && enemyUnit != null)
            {
                enemyHPText.text = $"敵: {enemyUnit.currentHP}/{enemyUnit.maxHP}";
            }
        }
        
        /// <summary>
        /// ターン表示を更新
        /// </summary>
        /// <param name="customText">カスタムテキスト</param>
        private void UpdateTurnText(string customText = null)
        {
            if (turnText != null)
            {
                if (customText != null)
                {
                    turnText.text = customText;
                }
                else if (currentBattleState != null)
                {
                    turnText.text = currentBattleState.isPlayerTurn ? "プレイヤーターン" : "敵ターン";
                }
            }
        }
        
        /// <summary>
        /// ボタンの有効/無効を設定
        /// </summary>
        /// <param name="interactable">有効にする場合true</param>
        private void SetButtonInteractable(bool interactable)
        {
            if (attackButton != null)
                attackButton.interactable = interactable;
            if (healButton != null)
                healButton.interactable = interactable;
            if (endTurnButton != null)
                endTurnButton.interactable = interactable;
        }
        
        /// <summary>
        /// ログを追加
        /// </summary>
        /// <param name="message">メッセージ</param>
        private void AddLog(string message)
        {
            if (logText != null)
            {
                logText.text += $"\n{message}";
                
                // ログが長すぎる場合は古い部分を削除
                if (logText.text.Length > 1000)
                {
                    logText.text = logText.text.Substring(logText.text.Length - 500);
                }
            }
        }
        
        // ボタンイベントハンドラー
        
        /// <summary>
        /// 攻撃ボタンクリック時の処理
        /// </summary>
        private void OnAttackButtonClicked()
        {
            if (playerUnit != null && enemyUnit != null && currentBattleState.isPlayerTurn)
            {
                playerUnit.Attack(enemyUnit);
                SetButtonInteractable(false);
                AddLog($"{playerUnit.unitName}が{enemyUnit.unitName}を攻撃しました");
            }
        }
        
        /// <summary>
        /// 回復ボタンクリック時の処理
        /// </summary>
        private void OnHealButtonClicked()
        {
            if (playerUnit != null && currentBattleState.isPlayerTurn)
            {
                playerUnit.Heal(10);
                SetButtonInteractable(false);
                AddLog($"{playerUnit.unitName}が回復しました");
            }
        }
        
        /// <summary>
        /// ターン終了ボタンクリック時の処理
        /// </summary>
        private void OnEndTurnButtonClicked()
        {
            if (currentBattleState != null && currentBattleState.isPlayerTurn)
            {
                currentBattleState.AdvanceTurn();
                SetButtonInteractable(false);
                AddLog("プレイヤーターンを終了しました");
            }
        }
        
        // ユニットイベントハンドラー
        
        /// <summary>
        /// ユニット撃破時の処理
        /// </summary>
        /// <param name="unit">撃破されたユニット</param>
        private void OnUnitDefeated(BattleUnit unit)
        {
            AddLog($"{unit.unitName}が倒されました");
            UpdateUnitDisplays();
        }
        
        /// <summary>
        /// ユニット攻撃時の処理
        /// </summary>
        /// <param name="unit">攻撃したユニット</param>
        private void OnUnitAttacked(BattleUnit unit)
        {
            UpdateUnitDisplays();
        }
        
        /// <summary>
        /// ユニット回復時の処理
        /// </summary>
        /// <param name="unit">回復したユニット</param>
        private void OnUnitHealed(BattleUnit unit)
        {
            AddLog($"{unit.unitName}が回復しました");
            UpdateUnitDisplays();
        }
        
        private void OnDestroy()
        {
            // イベントの購読解除
            BattleUnit.OnUnitDefeated -= OnUnitDefeated;
            BattleUnit.OnUnitAttacked -= OnUnitAttacked;
            BattleUnit.OnUnitHealed -= OnUnitHealed;
        }
    }
} 