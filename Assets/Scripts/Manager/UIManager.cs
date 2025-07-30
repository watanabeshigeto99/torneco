using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-50)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HP Display")]
    public UnityEngine.UI.Slider hpSlider;
    public TextMeshProUGUI hpText;
    
    [Header("Turn Display")]
    public TextMeshProUGUI turnLabel;
    
    [Header("Floor Display")]
    public TextMeshProUGUI floorLabel; // 階層表示用
    
    [Header("Log Display")]
    public TextMeshProUGUI logText;
    public int maxLogLines = 5;

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
    }
    
    private void OnAllObjectsInitialized()
    {
        Debug.Log("UIManager: 全オブジェクト初期化完了イベントを受信");
        
        // 初期表示を更新
        if (Player.Instance != null)
        {
            UpdateHP(Player.Instance.currentHP, Player.Instance.maxHP);
        }
        
        // 階層表示を更新（準備段階）
        UpdateFloorDisplay(GameManager.Instance.currentFloor);
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
} 