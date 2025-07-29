using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
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
            actionLog.text += message + "\n";
        }
    }

    public void ClearLog()
    {
        if (actionLog != null)
        {
            actionLog.text = "";
        }
    }
} 