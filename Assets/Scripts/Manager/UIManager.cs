using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-50)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    
    public static UIManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("UIManager");
            Instance = go.AddComponent<UIManager>();
        }
        return Instance;
    }

    [Header("HP Display")]
    public UnityEngine.UI.Slider hpSlider;
    public TextMeshProUGUI hpText;
    
    [Header("Turn Display")]
    public TextMeshProUGUI turnLabel;
    
    [Header("Floor Display")]
    public TextMeshProUGUI floorLabel; // 階層表示用
    
    [Header("Level Display")]
    public TextMeshProUGUI levelLabel; // レベル表示用
    public TextMeshProUGUI expLabel; // 経験値表示用
    
    [Header("Log Display")]
    public TextMeshProUGUI logText;
    public int maxLogLines = 5;
    
    [Header("Card Enhancement")]
    public Button enhanceButton; // カード強化ボタン（テスト用）

    private void Awake()
    {
        Debug.Log("UIManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("UIManager: 重複するUIManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化（シーン間で保持）
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("UIManager: Awake完了");
    }

    private void Start()
    {
        Debug.Log("UIManager: Start開始");
        
        // イベントを購読
        SubscribeToEvents();
        
        // 初期表示を設定
        UpdateFloorDisplay(1);
        
        Debug.Log("UIManager: Start完了");
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        // 全オブジェクト初期化完了イベントを購読
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
        
        // 階層システムイベントを購読（準備段階）
        GameManager.OnFloorChanged += OnFloorChanged;
        GameManager.OnGameClear += OnGameClear;
        GameManager.OnGameOver += OnGameOver;
        
        // プレイヤーレベルアップイベントを購読
        Player.OnPlayerLevelUp += OnPlayerLevelUp;
        
        Debug.Log("UIManager: イベント購読完了");
    }
    
    private void UnsubscribeFromEvents()
    {
        // イベントの購読を解除
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
        
        // 階層システムイベントの購読を解除
        GameManager.OnFloorChanged -= OnFloorChanged;
        GameManager.OnGameClear -= OnGameClear;
        GameManager.OnGameOver -= OnGameOver;
        
        // プレイヤーレベルアップイベントの購読を解除
        Player.OnPlayerLevelUp -= OnPlayerLevelUp;
    }
    
    private void OnAllObjectsInitialized()
    {
        Debug.Log("UIManager: 全オブジェクト初期化完了イベントを受信");
        
        // 初期表示を更新
        if (Player.Instance != null)
        {
            UpdateHP(Player.Instance.currentHP, Player.Instance.maxHP);
            UpdateLevelDisplay(Player.Instance.level, Player.Instance.exp, Player.Instance.expToNext);
        }
        
        // 階層表示を更新（準備段階）
        UpdateFloorDisplay(GameManager.Instance.currentFloor);
        
        // 強化ボタンの設定
        SetupEnhanceButton();
    }
    
    // 強化ボタンの設定
    private void SetupEnhanceButton()
    {
        if (enhanceButton != null)
        {
            enhanceButton.onClick.RemoveAllListeners();
            enhanceButton.onClick.AddListener(OnEnhanceButtonClicked);
        }
    }
    
    // 強化ボタンクリック時の処理
    private void OnEnhanceButtonClicked()
    {
        if (CardManager.Instance != null)
        {
            // ランダムにカードを強化（テスト用）
            var cardPool = CardManager.Instance.cardPool;
            if (cardPool != null && cardPool.Length > 0)
            {
                CardDataSO randomCard = cardPool[UnityEngine.Random.Range(0, cardPool.Length)];
                CardManager.Instance.EnhanceCard(randomCard);
            }
        }
    }
    
    // 階層システム準備メソッド
    private void OnFloorChanged(int newFloor)
    {
        Debug.Log($"UIManager: 階層変更イベントを受信 - 新しい階層: {newFloor}");
        UpdateFloorDisplay(newFloor);
    }
    
    private void OnGameClear()
    {
        Debug.Log("UIManager: ゲームクリアイベントを受信");
        // TODO: ゲームクリア時のUI更新
    }
    
    private void OnGameOver()
    {
        Debug.Log("UIManager: ゲームオーバーイベントを受信");
        // TODO: ゲームオーバー時のUI更新
    }
    
    public void UpdateHP(int currentHP, int maxHP)
    {
        if (hpSlider != null)
        {
            hpSlider.value = (float)currentHP / maxHP;
        }
        
        if (hpText != null)
        {
            hpText.text = $"HP: {currentHP}/{maxHP}";
        }
    }
    
    public void UpdateTurnLabel(string turnText)
    {
        if (turnLabel != null)
        {
            turnLabel.text = turnText;
        }
    }
    
    public void UpdateFloorDisplay(int floor)
    {
        if (floorLabel != null)
        {
            floorLabel.text = $"階層: {floor}";
        }
    }
    
    // レベルアップイベントハンドラー
    private void OnPlayerLevelUp(int newLevel)
    {
        if (Player.Instance != null)
        {
            UpdateLevelDisplay(Player.Instance.level, Player.Instance.exp, Player.Instance.expToNext);
        }
    }
    
    // レベル表示を更新
    public void UpdateLevelDisplay(int level, int exp, int expToNext)
    {
        if (levelLabel != null)
        {
            levelLabel.text = $"Lv.{level}";
        }
        
        if (expLabel != null)
        {
            expLabel.text = $"EXP: {exp}/{expToNext}";
        }
    }
    
    public void AddLog(string message)
    {
        if (logText != null)
        {
            // 新しいログを追加
            string newLog = $"[{System.DateTime.Now:HH:mm:ss}] {message}\n";
            logText.text = newLog + logText.text;
            
            // 最大行数を制限
            string[] lines = logText.text.Split('\n');
            if (lines.Length > maxLogLines)
            {
                logText.text = string.Join("\n", lines, 0, maxLogLines);
            }
        }
    }
    
    /// <summary>
    /// シーン遷移時のクリーンアップ処理
    /// </summary>
    public void CleanupForSceneTransition()
    {
        Debug.Log("UIManager: シーン遷移時のクリーンアップ開始");
        
        // イベントの購読を解除
        UnsubscribeFromEvents();
        
        // UI要素のクリーンアップ
        if (logText != null)
        {
            logText.text = "";
        }
        
        Debug.Log("UIManager: シーン遷移時のクリーンアップ完了");
    }
    
    /// <summary>
    /// メインシーン用の初期化処理
    /// </summary>
    public void InitializeForMainScene()
    {
        Debug.Log("UIManager: メインシーン初期化開始");
        
        // イベントを再購読
        SubscribeToEvents();
        
        // 初期表示を設定
        UpdateFloorDisplay(GameManager.Instance?.currentFloor ?? 1);
        
        Debug.Log("UIManager: メインシーン初期化完了");
    }
    
    /// <summary>
    /// デッキビルダーシーン用の初期化処理
    /// </summary>
    public void InitializeForDeckBuilderScene()
    {
        Debug.Log("UIManager: デッキビルダーシーン初期化開始");
        
        // イベントを再購読
        SubscribeToEvents();
        
        // ログをクリア
        if (logText != null)
        {
            logText.text = "";
        }
        
        Debug.Log("UIManager: デッキビルダーシーン初期化完了");
    }
} 