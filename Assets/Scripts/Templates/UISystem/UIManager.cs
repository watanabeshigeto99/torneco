using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

namespace UISystem
{
    /// <summary>
    /// UIシステム管理専用コンポーネント
    /// 責務：UIシステムの管理のみ
    /// </summary>
    [DefaultExecutionOrder(-300)]
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }
        
        [Header("UI System Settings")]
        public UIDataSO uiData;
        public UIEventChannel uiEventChannel;
        
        [Header("UI Components")]
        public Canvas mainCanvas;
        public CanvasGroup mainCanvasGroup;
        public GameObject loadingScreen;
        public GameObject pauseMenu;
        public GameObject gameOverScreen;
        public GameObject gameClearScreen;
        
        [Header("HUD Elements")]
        public TextMeshProUGUI hpText;
        public TextMeshProUGUI levelText;
        public TextMeshProUGUI scoreText;
        public TextMeshProUGUI floorText;
        public Slider hpSlider;
        public Slider expSlider;
        
        [Header("Log System")]
        public ScrollRect logScrollRect;
        public TextMeshProUGUI logText;
        public int maxLogLines = 50;
        
        [Header("Card UI")]
        public Transform cardContainer;
        public GameObject cardPrefab;
        public List<GameObject> activeCards = new List<GameObject>();
        
        // UIシステム変更イベント
        public static event Action<UIDataSO> OnUIDataChanged;
        public static event Action<string> OnLogAdded;
        public static event Action OnUIStateChanged;
        public static event Action OnScreenTransition;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeUISystem();
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        /// <summary>
        /// UIシステムの初期化
        /// </summary>
        private void InitializeUISystem()
        {
            if (uiData == null)
            {
                Debug.LogError("UIManager: uiDataが設定されていません");
                return;
            }
            
            uiData.Initialize();
            UpdateAllUIElements();
            Debug.Log("UIManager: UIシステムを初期化しました");
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (uiEventChannel != null)
            {
                uiEventChannel.OnUIDataChanged.AddListener(OnUIDataChangedHandler);
                uiEventChannel.OnLogAdded.AddListener(OnLogAddedHandler);
                uiEventChannel.OnUIStateChanged.AddListener(OnUIStateChangedHandler);
                uiEventChannel.OnScreenTransition.AddListener(OnScreenTransitionHandler);
                uiEventChannel.OnHPChanged.AddListener(OnHPChangedHandler);
                uiEventChannel.OnLevelChanged.AddListener(OnLevelChangedHandler);
                uiEventChannel.OnScoreChanged.AddListener(OnScoreChangedHandler);
                uiEventChannel.OnFloorChanged.AddListener(OnFloorChangedHandler);
            }
        }
        
        /// <summary>
        /// UIデータ変更ハンドラー
        /// </summary>
        private void OnUIDataChangedHandler(UIDataSO uiData)
        {
            OnUIDataChanged?.Invoke(uiData);
            UpdateAllUIElements();
            Debug.Log("UIManager: UIデータが変更されました");
        }
        
        /// <summary>
        /// ログ追加ハンドラー
        /// </summary>
        private void OnLogAddedHandler(string logMessage)
        {
            OnLogAdded?.Invoke(logMessage);
            AddLog(logMessage);
            Debug.Log($"UIManager: ログが追加されました - {logMessage}");
        }
        
        /// <summary>
        /// UI状態変更ハンドラー
        /// </summary>
        private void OnUIStateChangedHandler()
        {
            OnUIStateChanged?.Invoke();
            Debug.Log("UIManager: UI状態が変更されました");
        }
        
        /// <summary>
        /// 画面遷移ハンドラー
        /// </summary>
        private void OnScreenTransitionHandler()
        {
            OnScreenTransition?.Invoke();
            Debug.Log("UIManager: 画面遷移が発生しました");
        }
        
        /// <summary>
        /// HP変更ハンドラー
        /// </summary>
        private void OnHPChangedHandler(int currentHP, int maxHP)
        {
            UpdateHP(currentHP, maxHP);
        }
        
        /// <summary>
        /// レベル変更ハンドラー
        /// </summary>
        private void OnLevelChangedHandler(int level, int exp, int expToNext)
        {
            UpdateLevelDisplay(level, exp, expToNext);
        }
        
        /// <summary>
        /// スコア変更ハンドラー
        /// </summary>
        private void OnScoreChangedHandler(int score)
        {
            UpdateScore(score);
        }
        
        /// <summary>
        /// 階層変更ハンドラー
        /// </summary>
        private void OnFloorChangedHandler(int floor)
        {
            UpdateFloor(floor);
        }
        
        /// <summary>
        /// すべてのUI要素を更新
        /// </summary>
        private void UpdateAllUIElements()
        {
            if (uiData == null) return;
            
            UpdateHP(uiData.currentHP, uiData.maxHP);
            UpdateLevelDisplay(uiData.playerLevel, uiData.playerExp, uiData.playerExpToNext);
            UpdateScore(uiData.score);
            UpdateFloor(uiData.currentFloor);
        }
        
        /// <summary>
        /// HP表示を更新
        /// </summary>
        public void UpdateHP(int currentHP, int maxHP)
        {
            if (uiData != null)
            {
                uiData.SetHP(currentHP, maxHP);
            }
            
            if (hpText != null)
            {
                hpText.text = $"HP: {currentHP}/{maxHP}";
            }
            
            if (hpSlider != null)
            {
                hpSlider.value = maxHP > 0 ? (float)currentHP / maxHP : 0f;
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseHPChanged(currentHP, maxHP);
            }
        }
        
        /// <summary>
        /// レベル表示を更新
        /// </summary>
        public void UpdateLevelDisplay(int level, int exp, int expToNext)
        {
            if (uiData != null)
            {
                uiData.SetLevel(level, exp, expToNext);
            }
            
            if (levelText != null)
            {
                levelText.text = $"Level: {level}";
            }
            
            if (expSlider != null)
            {
                expSlider.value = expToNext > 0 ? (float)exp / expToNext : 0f;
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseLevelChanged(level, exp, expToNext);
            }
        }
        
        /// <summary>
        /// スコア表示を更新
        /// </summary>
        public void UpdateScore(int score)
        {
            if (uiData != null)
            {
                uiData.SetScore(score);
            }
            
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseScoreChanged(score);
            }
        }
        
        /// <summary>
        /// 階層表示を更新
        /// </summary>
        public void UpdateFloor(int floor)
        {
            if (uiData != null)
            {
                uiData.SetFloor(floor);
            }
            
            if (floorText != null)
            {
                floorText.text = $"Floor: {floor}";
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseFloorChanged(floor);
            }
        }
        
        /// <summary>
        /// ログを追加
        /// </summary>
        public void AddLog(string message)
        {
            if (uiData != null)
            {
                uiData.AddLog(message);
            }
            
            if (logText != null)
            {
                string timestamp = $"[{DateTime.Now:HH:mm:ss}] ";
                logText.text = timestamp + message + "\n" + logText.text;
                
                // 最大行数を制限
                string[] lines = logText.text.Split('\n');
                if (lines.Length > maxLogLines)
                {
                    logText.text = string.Join("\n", lines, 0, maxLogLines);
                }
                
                // スクロールを最下部に
                if (logScrollRect != null)
                {
                    Canvas.ForceUpdateCanvases();
                    logScrollRect.verticalNormalizedPosition = 0f;
                }
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseLogAdded(message);
            }
        }
        
        /// <summary>
        /// ログをクリア
        /// </summary>
        public void ClearLog()
        {
            if (logText != null)
            {
                logText.text = "";
            }
            
            if (uiData != null)
            {
                uiData.ClearLog();
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseLogCleared();
            }
        }
        
        /// <summary>
        /// ローディング画面を表示
        /// </summary>
        public void ShowLoadingScreen()
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(true);
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseLoadingScreenShown();
            }
        }
        
        /// <summary>
        /// ローディング画面を非表示
        /// </summary>
        public void HideLoadingScreen()
        {
            if (loadingScreen != null)
            {
                loadingScreen.SetActive(false);
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseLoadingScreenHidden();
            }
        }
        
        /// <summary>
        /// ポーズメニューを表示
        /// </summary>
        public void ShowPauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(true);
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaisePauseMenuShown();
            }
        }
        
        /// <summary>
        /// ポーズメニューを非表示
        /// </summary>
        public void HidePauseMenu()
        {
            if (pauseMenu != null)
            {
                pauseMenu.SetActive(false);
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaisePauseMenuHidden();
            }
        }
        
        /// <summary>
        /// ゲームオーバー画面を表示
        /// </summary>
        public void ShowGameOverScreen()
        {
            if (gameOverScreen != null)
            {
                gameOverScreen.SetActive(true);
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseGameOverScreenShown();
            }
        }
        
        /// <summary>
        /// ゲームクリア画面を表示
        /// </summary>
        public void ShowGameClearScreen()
        {
            if (gameClearScreen != null)
            {
                gameClearScreen.SetActive(true);
            }
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseGameClearScreenShown();
            }
        }
        
        /// <summary>
        /// カードUIを生成
        /// </summary>
        public GameObject CreateCardUI(CardDataSO cardData)
        {
            if (cardPrefab == null || cardContainer == null)
            {
                Debug.LogError("UIManager: カードUIの生成に必要なコンポーネントが設定されていません");
                return null;
            }
            
            GameObject cardUI = Instantiate(cardPrefab, cardContainer);
            CardUI cardUIComponent = cardUI.GetComponent<CardUI>();
            
            if (cardUIComponent != null)
            {
                // 既存のCardUIクラスのSetupメソッドを使用
                cardUIComponent.Setup(cardData, OnCardUIClicked);
            }
            
            activeCards.Add(cardUI);
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseCardUICreated(cardData);
            }
            
            return cardUI;
        }
        
        /// <summary>
        /// カードUIを削除
        /// </summary>
        public void RemoveCardUI(GameObject cardUI)
        {
            if (activeCards.Remove(cardUI))
            {
                Destroy(cardUI);
                
                if (uiEventChannel != null)
                {
                    uiEventChannel.RaiseCardUIRemoved();
                }
            }
        }
        
        /// <summary>
        /// すべてのカードUIをクリア
        /// </summary>
        public void ClearAllCardUI()
        {
            foreach (var card in activeCards)
            {
                if (card != null)
                {
                    Destroy(card);
                }
            }
            
            activeCards.Clear();
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseAllCardUICleared();
            }
        }
        
        /// <summary>
        /// UIデータを取得
        /// </summary>
        public UIDataSO GetUIData()
        {
            return uiData;
        }
        
        /// <summary>
        /// カードUIクリック時の処理
        /// </summary>
        private void OnCardUIClicked(CardDataSO cardData)
        {
            Debug.Log($"UIManager: カードUIがクリックされました - {cardData?.cardName ?? "Unknown"}");
            
            if (uiEventChannel != null)
            {
                uiEventChannel.RaiseCardUICreated(cardData);
            }
        }
        
        /// <summary>
        /// UIシステムの情報を取得
        /// </summary>
        public string GetUISystemInfo()
        {
            return $"UIManager - Data: {(uiData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(uiEventChannel != null ? "✓" : "✗")}, " +
                   $"ActiveCards: {activeCards.Count}";
        }
    }
} 