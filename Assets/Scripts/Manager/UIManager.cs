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
    public int maxLogLines = 5; // 最大5行まで表示

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
        // ゲーム開始時にログをクリア
        ClearLog();
        
        // イベントを購読
        SubscribeToEvents();
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        // プレイヤーの行動イベントを購読
        Player.OnPlayerMoved += OnPlayerMoved;
        Player.OnPlayerAttacked += OnPlayerAttacked;
        Player.OnPlayerHealed += OnPlayerHealed;
        Player.OnPlayerDied += OnPlayerDied;
        
        // グリッド初期化イベントを購読
        GridManager.OnGridInitialized += OnGridInitialized;
        
        // 全オブジェクト初期化完了イベントを購読
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
        
        Debug.Log("UIManager: イベント購読完了");
    }
    
    private void UnsubscribeFromEvents()
    {
        // イベントの購読を解除
        Player.OnPlayerMoved -= OnPlayerMoved;
        Player.OnPlayerAttacked -= OnPlayerAttacked;
        Player.OnPlayerHealed -= OnPlayerHealed;
        Player.OnPlayerDied -= OnPlayerDied;
        
        GridManager.OnGridInitialized -= OnGridInitialized;
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
        
        Debug.Log("UIManager: イベント購読解除完了");
    }
    
    // イベントハンドラー
    private void OnPlayerMoved(Vector2Int newPosition)
    {
        Debug.Log($"UIManager: プレイヤー移動イベント受信 位置: {newPosition}");
        AddLog($"プレイヤーが移動しました: {newPosition}");
    }
    
    private void OnPlayerAttacked(int damage)
    {
        Debug.Log($"UIManager: プレイヤー攻撃イベント受信 ダメージ: {damage}");
        AddLog($"攻撃しました！ダメージ: {damage}");
    }
    
    private void OnPlayerHealed(int healAmount)
    {
        Debug.Log($"UIManager: プレイヤー回復イベント受信 回復量: {healAmount}");
        AddLog($"回復しました！回復量: {healAmount}");
    }
    
    private void OnPlayerDied()
    {
        Debug.Log("UIManager: プレイヤー死亡イベント受信");
        AddLog("プレイヤーが死亡しました...");
    }
    
    private void OnGridInitialized()
    {
        Debug.Log("UIManager: グリッド初期化イベント受信");
        AddLog("ゲーム開始！");
    }
    
    private void OnAllObjectsInitialized()
    {
        Debug.Log("UIManager: 全オブジェクト初期化完了イベント受信");
        AddLog("全てのオブジェクトが初期化完了しました");
        
        // プレイヤーのHPを初期表示
        if (Player.Instance != null)
        {
            UpdateHP(Player.Instance.currentHP, Player.Instance.maxHP);
        }
        
        // ターン表示を初期化
        SetTurnText(true); // プレイヤーターンから開始
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
            // 新しいログメッセージを追加
            logMessages.Add(message);
            
            // 最大行数を超えた場合、古いログを削除
            while (logMessages.Count > maxLogLines)
            {
                logMessages.RemoveAt(0);
            }
            
            // ログテキストを更新
            UpdateLogDisplay();
        }
    }

    private void UpdateLogDisplay()
    {
        if (actionLog != null)
        {
            // 全てのログメッセージを結合
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