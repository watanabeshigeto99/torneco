using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-50)]
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HP UI")]
    public Slider hpSlider;
    public TextMeshProUGUI hpText;

    [Header("Turn UI")]
    public TextMeshProUGUI turnLabel;

    [Header("Action Log")]
    public TextMeshProUGUI actionLog;
    [Tooltip("表示するログの最大行数")]
    public int maxLogLines = 5;

    private List<string> logMessages = new List<string>();

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
        ClearLog();
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        Player.OnPlayerMoved += OnPlayerMoved;
        Player.OnPlayerAttacked += OnPlayerAttacked;
        Player.OnPlayerHealed += OnPlayerHealed;
        Player.OnPlayerDied += OnPlayerDied;
        
        GridManager.OnGridInitialized += OnGridInitialized;
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
    }
    
    private void UnsubscribeFromEvents()
    {
        Player.OnPlayerMoved -= OnPlayerMoved;
        Player.OnPlayerAttacked -= OnPlayerAttacked;
        Player.OnPlayerHealed -= OnPlayerHealed;
        Player.OnPlayerDied -= OnPlayerDied;
        
        GridManager.OnGridInitialized -= OnGridInitialized;
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
    }
    
    // イベントハンドラー
    private void OnPlayerMoved(Vector2Int newPosition)
    {
        AddLog($"プレイヤーが移動しました: {newPosition}");
    }
    
    private void OnPlayerAttacked(int damage)
    {
        AddLog($"攻撃しました！ダメージ: {damage}");
    }
    
    private void OnPlayerHealed(int healAmount)
    {
        AddLog($"回復しました！回復量: {healAmount}");
    }
    
    private void OnPlayerDied()
    {
        AddLog("プレイヤーが死亡しました...");
    }
    
    private void OnGridInitialized()
    {
        AddLog("ゲーム開始！");
    }
    
    private void OnAllObjectsInitialized()
    {
        AddLog("全てのオブジェクトが初期化完了しました");
        
        // プレイヤーのHPを初期表示
        if (Player.Instance != null)
        {
            UpdateHP(Player.Instance.currentHP, Player.Instance.maxHP);
        }
        
        // ターン表示を初期化
        SetTurnText(true);
    }

    public void UpdateHP(int current, int max)
    {
        if (hpSlider != null)
        {
            hpSlider.maxValue = max;
            hpSlider.value = current;
        }
        
        if (hpText != null)
        {
            hpText.text = $"HP: {current} / {max}";
        }
    }

    public void SetTurnText(bool isPlayerTurn)
    {
        if (turnLabel != null)
        {
            turnLabel.text = isPlayerTurn ? "プレイヤーのターン" : "敵のターン";
        }
    }

    public void AddLog(string message)
    {
        if (actionLog != null)
        {
            logMessages.Add(message);
            
            while (logMessages.Count > maxLogLines)
            {
                logMessages.RemoveAt(0);
            }
            
            UpdateLogDisplay();
        }
    }

    private void UpdateLogDisplay()
    {
        if (actionLog != null)
        {
            actionLog.text = string.Join("\n", logMessages);
        }
    }

    public void ClearLog()
    {
        if (actionLog != null)
        {
            logMessages.Clear();
            actionLog.text = "";
        }
    }
} 