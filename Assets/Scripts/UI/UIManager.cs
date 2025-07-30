using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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