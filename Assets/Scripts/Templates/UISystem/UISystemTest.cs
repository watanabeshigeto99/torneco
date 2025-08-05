using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UISystem
{
    /// <summary>
    /// UIシステムテンプレートのテスト用コンポーネント
    /// 責務：UIシステムのテストのみ
    /// </summary>
    public class UISystemTest : MonoBehaviour
    {
        [Header("UI System Components")]
        public UIManager uiManager;
        public UIDataSO uiData;
        public UIEventChannel uiEventChannel;
        
        [Header("Test UI")]
        public Button updateHPButton;
        public Button updateLevelButton;
        public Button updateScoreButton;
        public Button updateFloorButton;
        public Button addLogButton;
        public Button clearLogButton;
        public Button showPauseMenuButton;
        public Button hidePauseMenuButton;
        public Button showLoadingScreenButton;
        public Button hideLoadingScreenButton;
        public Button createCardUIButton;
        public Button clearCardUIButton;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI eventLogText;
        
        [Header("Test Cards")]
        public CardDataSO[] testCards;
        
        private string eventLog = "";
        private int maxEventLogLines = 15;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            if (updateHPButton != null)
                updateHPButton.onClick.AddListener(OnUpdateHPButtonClicked);
            
            if (updateLevelButton != null)
                updateLevelButton.onClick.AddListener(OnUpdateLevelButtonClicked);
            
            if (updateScoreButton != null)
                updateScoreButton.onClick.AddListener(OnUpdateScoreButtonClicked);
            
            if (updateFloorButton != null)
                updateFloorButton.onClick.AddListener(OnUpdateFloorButtonClicked);
            
            if (addLogButton != null)
                addLogButton.onClick.AddListener(OnAddLogButtonClicked);
            
            if (clearLogButton != null)
                clearLogButton.onClick.AddListener(OnClearLogButtonClicked);
            
            if (showPauseMenuButton != null)
                showPauseMenuButton.onClick.AddListener(OnShowPauseMenuButtonClicked);
            
            if (hidePauseMenuButton != null)
                hidePauseMenuButton.onClick.AddListener(OnHidePauseMenuButtonClicked);
            
            if (showLoadingScreenButton != null)
                showLoadingScreenButton.onClick.AddListener(OnShowLoadingScreenButtonClicked);
            
            if (hideLoadingScreenButton != null)
                hideLoadingScreenButton.onClick.AddListener(OnHideLoadingScreenButtonClicked);
            
            if (createCardUIButton != null)
                createCardUIButton.onClick.AddListener(OnCreateCardUIButtonClicked);
            
            if (clearCardUIButton != null)
                clearCardUIButton.onClick.AddListener(OnClearCardUIButtonClicked);
            
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (uiEventChannel != null)
            {
                uiEventChannel.OnUIDataChanged.AddListener(OnUIDataChanged);
                uiEventChannel.OnLogAdded.AddListener(OnLogAdded);
                uiEventChannel.OnLogCleared.AddListener(OnLogCleared);
                uiEventChannel.OnUIStateChanged.AddListener(OnUIStateChanged);
                uiEventChannel.OnScreenTransition.AddListener(OnScreenTransition);
                uiEventChannel.OnHPChanged.AddListener(OnHPChanged);
                uiEventChannel.OnLevelChanged.AddListener(OnLevelChanged);
                uiEventChannel.OnScoreChanged.AddListener(OnScoreChanged);
                uiEventChannel.OnFloorChanged.AddListener(OnFloorChanged);
                uiEventChannel.OnPauseStateChanged.AddListener(OnPauseStateChanged);
                uiEventChannel.OnLoadingStateChanged.AddListener(OnLoadingStateChanged);
                uiEventChannel.OnGameOverStateChanged.AddListener(OnGameOverStateChanged);
                uiEventChannel.OnGameClearStateChanged.AddListener(OnGameClearStateChanged);
                uiEventChannel.OnMenuStateChanged.AddListener(OnMenuStateChanged);
                uiEventChannel.OnLoadingScreenShown.AddListener(OnLoadingScreenShown);
                uiEventChannel.OnLoadingScreenHidden.AddListener(OnLoadingScreenHidden);
                uiEventChannel.OnPauseMenuShown.AddListener(OnPauseMenuShown);
                uiEventChannel.OnPauseMenuHidden.AddListener(OnPauseMenuHidden);
                uiEventChannel.OnGameOverScreenShown.AddListener(OnGameOverScreenShown);
                uiEventChannel.OnGameClearScreenShown.AddListener(OnGameClearScreenShown);
                uiEventChannel.OnCardUICreated.AddListener(OnCardUICreated);
                uiEventChannel.OnCardUIRemoved.AddListener(OnCardUIRemoved);
                uiEventChannel.OnAllCardUICleared.AddListener(OnAllCardUICleared);
                uiEventChannel.OnCardCountChanged.AddListener(OnCardCountChanged);
                uiEventChannel.OnUIScaleChanged.AddListener(OnUIScaleChanged);
                uiEventChannel.OnAnimationEnabledChanged.AddListener(OnAnimationEnabledChanged);
                uiEventChannel.OnHUDVisibilityChanged.AddListener(OnHUDVisibilityChanged);
                uiEventChannel.OnLogVisibilityChanged.AddListener(OnLogVisibilityChanged);
                uiEventChannel.OnCardVisibilityChanged.AddListener(OnCardVisibilityChanged);
            }
        }
        
        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (uiEventChannel != null)
            {
                uiEventChannel.OnUIDataChanged.RemoveListener(OnUIDataChanged);
                uiEventChannel.OnLogAdded.RemoveListener(OnLogAdded);
                uiEventChannel.OnLogCleared.RemoveListener(OnLogCleared);
                uiEventChannel.OnUIStateChanged.RemoveListener(OnUIStateChanged);
                uiEventChannel.OnScreenTransition.RemoveListener(OnScreenTransition);
                uiEventChannel.OnHPChanged.RemoveListener(OnHPChanged);
                uiEventChannel.OnLevelChanged.RemoveListener(OnLevelChanged);
                uiEventChannel.OnScoreChanged.RemoveListener(OnScoreChanged);
                uiEventChannel.OnFloorChanged.RemoveListener(OnFloorChanged);
                uiEventChannel.OnPauseStateChanged.RemoveListener(OnPauseStateChanged);
                uiEventChannel.OnLoadingStateChanged.RemoveListener(OnLoadingStateChanged);
                uiEventChannel.OnGameOverStateChanged.RemoveListener(OnGameOverStateChanged);
                uiEventChannel.OnGameClearStateChanged.RemoveListener(OnGameClearStateChanged);
                uiEventChannel.OnMenuStateChanged.RemoveListener(OnMenuStateChanged);
                uiEventChannel.OnLoadingScreenShown.RemoveListener(OnLoadingScreenShown);
                uiEventChannel.OnLoadingScreenHidden.RemoveListener(OnLoadingScreenHidden);
                uiEventChannel.OnPauseMenuShown.RemoveListener(OnPauseMenuShown);
                uiEventChannel.OnPauseMenuHidden.RemoveListener(OnPauseMenuHidden);
                uiEventChannel.OnGameOverScreenShown.RemoveListener(OnGameOverScreenShown);
                uiEventChannel.OnGameClearScreenShown.RemoveListener(OnGameClearScreenShown);
                uiEventChannel.OnCardUICreated.RemoveListener(OnCardUICreated);
                uiEventChannel.OnCardUIRemoved.RemoveListener(OnCardUIRemoved);
                uiEventChannel.OnAllCardUICleared.RemoveListener(OnAllCardUICleared);
                uiEventChannel.OnCardCountChanged.RemoveListener(OnCardCountChanged);
                uiEventChannel.OnUIScaleChanged.RemoveListener(OnUIScaleChanged);
                uiEventChannel.OnAnimationEnabledChanged.RemoveListener(OnAnimationEnabledChanged);
                uiEventChannel.OnHUDVisibilityChanged.RemoveListener(OnHUDVisibilityChanged);
                uiEventChannel.OnLogVisibilityChanged.RemoveListener(OnLogVisibilityChanged);
                uiEventChannel.OnCardVisibilityChanged.RemoveListener(OnCardVisibilityChanged);
            }
        }
        
        // UI Button Event Handlers
        
        /// <summary>
        /// HP更新ボタンクリック
        /// </summary>
        private void OnUpdateHPButtonClicked()
        {
            if (uiManager != null)
            {
                int newHP = Random.Range(1, 100);
                int newMaxHP = Random.Range(newHP, 100);
                uiManager.UpdateHP(newHP, newMaxHP);
                AddToEventLog($"HP更新ボタンクリック: {newHP}/{newMaxHP}");
            }
        }
        
        /// <summary>
        /// レベル更新ボタンクリック
        /// </summary>
        private void OnUpdateLevelButtonClicked()
        {
            if (uiManager != null)
            {
                int newLevel = Random.Range(1, 20);
                int newExp = Random.Range(0, 100);
                int newExpToNext = Random.Range(50, 200);
                uiManager.UpdateLevelDisplay(newLevel, newExp, newExpToNext);
                AddToEventLog($"レベル更新ボタンクリック: {newLevel}, Exp: {newExp}/{newExpToNext}");
            }
        }
        
        /// <summary>
        /// スコア更新ボタンクリック
        /// </summary>
        private void OnUpdateScoreButtonClicked()
        {
            if (uiManager != null)
            {
                int newScore = Random.Range(0, 10000);
                uiManager.UpdateScore(newScore);
                AddToEventLog($"スコア更新ボタンクリック: {newScore}");
            }
        }
        
        /// <summary>
        /// 階層更新ボタンクリック
        /// </summary>
        private void OnUpdateFloorButtonClicked()
        {
            if (uiManager != null)
            {
                int newFloor = Random.Range(1, 50);
                uiManager.UpdateFloor(newFloor);
                AddToEventLog($"階層更新ボタンクリック: {newFloor}");
            }
        }
        
        /// <summary>
        /// ログ追加ボタンクリック
        /// </summary>
        private void OnAddLogButtonClicked()
        {
            if (uiManager != null)
            {
                string[] testMessages = {
                    "テストメッセージ1",
                    "敵を倒しました！",
                    "レベルアップ！",
                    "アイテムを獲得しました",
                    "新しいカードを入手しました"
                };
                string randomMessage = testMessages[Random.Range(0, testMessages.Length)];
                uiManager.AddLog(randomMessage);
                AddToEventLog($"ログ追加ボタンクリック: {randomMessage}");
            }
        }
        
        /// <summary>
        /// ログクリアボタンクリック
        /// </summary>
        private void OnClearLogButtonClicked()
        {
            if (uiManager != null)
            {
                uiManager.ClearLog();
                AddToEventLog("ログクリアボタンクリック");
            }
        }
        
        /// <summary>
        /// ポーズメニュー表示ボタンクリック
        /// </summary>
        private void OnShowPauseMenuButtonClicked()
        {
            if (uiManager != null)
            {
                uiManager.ShowPauseMenu();
                AddToEventLog("ポーズメニュー表示ボタンクリック");
            }
        }
        
        /// <summary>
        /// ポーズメニュー非表示ボタンクリック
        /// </summary>
        private void OnHidePauseMenuButtonClicked()
        {
            if (uiManager != null)
            {
                uiManager.HidePauseMenu();
                AddToEventLog("ポーズメニュー非表示ボタンクリック");
            }
        }
        
        /// <summary>
        /// ローディング画面表示ボタンクリック
        /// </summary>
        private void OnShowLoadingScreenButtonClicked()
        {
            if (uiManager != null)
            {
                uiManager.ShowLoadingScreen();
                AddToEventLog("ローディング画面表示ボタンクリック");
            }
        }
        
        /// <summary>
        /// ローディング画面非表示ボタンクリック
        /// </summary>
        private void OnHideLoadingScreenButtonClicked()
        {
            if (uiManager != null)
            {
                uiManager.HideLoadingScreen();
                AddToEventLog("ローディング画面非表示ボタンクリック");
            }
        }
        
        /// <summary>
        /// カードUI作成ボタンクリック
        /// </summary>
        private void OnCreateCardUIButtonClicked()
        {
            if (uiManager != null && testCards != null && testCards.Length > 0)
            {
                CardDataSO randomCard = testCards[Random.Range(0, testCards.Length)];
                GameObject cardUI = uiManager.CreateCardUI(randomCard);
                if (cardUI != null)
                {
                    AddToEventLog($"カードUI作成ボタンクリック: {randomCard.cardName}");
                }
            }
        }
        
        /// <summary>
        /// カードUIクリアボタンクリック
        /// </summary>
        private void OnClearCardUIButtonClicked()
        {
            if (uiManager != null)
            {
                uiManager.ClearAllCardUI();
                AddToEventLog("カードUIクリアボタンクリック");
            }
        }
        
        // Event Handlers
        
        private void OnUIDataChanged(UIDataSO uiData)
        {
            AddToEventLog($"UIデータ変更: {uiData?.GetUIInfo() ?? "Unknown"}");
            UpdateStatusDisplay();
        }
        
        private void OnLogAdded(string message)
        {
            AddToEventLog($"ログ追加: {message}");
        }
        
        private void OnLogCleared()
        {
            AddToEventLog("ログクリア");
        }
        
        private void OnUIStateChanged()
        {
            AddToEventLog("UI状態変更");
        }
        
        private void OnScreenTransition()
        {
            AddToEventLog("画面遷移");
        }
        
        private void OnHPChanged(int currentHP, int maxHP)
        {
            AddToEventLog($"HP変更: {currentHP}/{maxHP}");
        }
        
        private void OnLevelChanged(int level, int exp, int expToNext)
        {
            AddToEventLog($"レベル変更: {level}, Exp: {exp}/{expToNext}");
        }
        
        private void OnScoreChanged(int score)
        {
            AddToEventLog($"スコア変更: {score}");
        }
        
        private void OnFloorChanged(int floor)
        {
            AddToEventLog($"階層変更: {floor}");
        }
        
        private void OnPauseStateChanged(bool isPaused)
        {
            AddToEventLog($"ポーズ状態変更: {isPaused}");
        }
        
        private void OnLoadingStateChanged(bool isLoading)
        {
            AddToEventLog($"ローディング状態変更: {isLoading}");
        }
        
        private void OnGameOverStateChanged(bool isGameOver)
        {
            AddToEventLog($"ゲームオーバー状態変更: {isGameOver}");
        }
        
        private void OnGameClearStateChanged(bool isGameClear)
        {
            AddToEventLog($"ゲームクリア状態変更: {isGameClear}");
        }
        
        private void OnMenuStateChanged(bool isMenuOpen)
        {
            AddToEventLog($"メニュー状態変更: {isMenuOpen}");
        }
        
        private void OnLoadingScreenShown()
        {
            AddToEventLog("ローディング画面表示");
        }
        
        private void OnLoadingScreenHidden()
        {
            AddToEventLog("ローディング画面非表示");
        }
        
        private void OnPauseMenuShown()
        {
            AddToEventLog("ポーズメニュー表示");
        }
        
        private void OnPauseMenuHidden()
        {
            AddToEventLog("ポーズメニュー非表示");
        }
        
        private void OnGameOverScreenShown()
        {
            AddToEventLog("ゲームオーバー画面表示");
        }
        
        private void OnGameClearScreenShown()
        {
            AddToEventLog("ゲームクリア画面表示");
        }
        
        private void OnCardUICreated(CardDataSO cardData)
        {
            AddToEventLog($"カードUI作成: {cardData?.cardName ?? "Unknown"}");
        }
        
        private void OnCardUIRemoved()
        {
            AddToEventLog("カードUI削除");
        }
        
        private void OnAllCardUICleared()
        {
            AddToEventLog("すべてのカードUIクリア");
        }
        
        private void OnCardCountChanged(int cardCount)
        {
            AddToEventLog($"カード数変更: {cardCount}");
        }
        
        private void OnUIScaleChanged(float scale)
        {
            AddToEventLog($"UIスケール変更: {scale}");
        }
        
        private void OnAnimationEnabledChanged(bool enabled)
        {
            AddToEventLog($"アニメーション有効化変更: {enabled}");
        }
        
        private void OnHUDVisibilityChanged(bool visible)
        {
            AddToEventLog($"HUD表示変更: {visible}");
        }
        
        private void OnLogVisibilityChanged(bool visible)
        {
            AddToEventLog($"ログ表示変更: {visible}");
        }
        
        private void OnCardVisibilityChanged(bool visible)
        {
            AddToEventLog($"カード表示変更: {visible}");
        }
        
        /// <summary>
        /// イベントログに追加
        /// </summary>
        private void AddToEventLog(string message)
        {
            eventLog = $"[{System.DateTime.Now:HH:mm:ss}] {message}\n{eventLog}";
            
            // 最大行数を制限
            string[] lines = eventLog.Split('\n');
            if (lines.Length > maxEventLogLines)
            {
                eventLog = string.Join("\n", lines, 0, maxEventLogLines);
            }
            
            if (eventLogText != null)
            {
                eventLogText.text = eventLog;
            }
        }
        
        /// <summary>
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText != null && uiData != null)
            {
                statusText.text = $"=== UI System Test ===\n" +
                                 $"UI: {uiData.GetUIInfo()}\n" +
                                 $"Manager: {(uiManager != null ? "✓" : "✗")}\n" +
                                 $"EventChannel: {(uiEventChannel != null ? "✓" : "✗")}\n" +
                                 $"Test Cards: {(testCards != null ? testCards.Length : 0)}";
            }
        }
        
        /// <summary>
        /// UIシステムテストの情報を取得
        /// </summary>
        public string GetUISystemTestInfo()
        {
            return $"UISystemTest - Manager: {(uiManager != null ? "✓" : "✗")}, " +
                   $"Data: {(uiData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(uiEventChannel != null ? "✓" : "✗")}, " +
                   $"TestCards: {(testCards != null ? testCards.Length : 0)}";
        }
    }
} 