using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SelectedCardUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image cardIcon;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardTypeText;
    public Button removeButton;
    
    [Header("Visual Feedback")]
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.red;
    
    [Header("Card Size Settings")]
    public Vector2 defaultCardSize = new Vector2(100, 150);
    public Vector2 largeCardSize = new Vector2(120, 180);
    public Vector2 smallCardSize = new Vector2(80, 120);
    
    private CardDataSO cardData;
    private Action<CardDataSO> onRemoveCallback;
    private RectTransform rectTransform;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    /// <summary>
    /// 選択されたカードをセットアップ
    /// </summary>
    public void Setup(CardDataSO card, Action<CardDataSO> onRemove)
    {
        cardData = card;
        onRemoveCallback = onRemove;
        
        UpdateDisplay();
        SetupButton();
    }
    
    /// <summary>
    /// カードサイズを変更
    /// </summary>
    public void SetCardSize(Vector2 size)
    {
        if (rectTransform != null)
        {
            rectTransform.sizeDelta = size;
        }
    }
    
    /// <summary>
    /// デフォルトサイズに設定
    /// </summary>
    public void SetDefaultSize()
    {
        SetCardSize(defaultCardSize);
    }
    
    /// <summary>
    /// 大きいサイズに設定
    /// </summary>
    public void SetLargeSize()
    {
        SetCardSize(largeCardSize);
    }
    
    /// <summary>
    /// 小さいサイズに設定
    /// </summary>
    public void SetSmallSize()
    {
        SetCardSize(smallCardSize);
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay()
    {
        if (cardData == null) return;
        
        // アイコンのみ表示（テキストは非表示）
        if (cardIcon != null && cardData.icon != null)
        {
            cardIcon.sprite = cardData.icon;
        }
        
        // テキスト要素は非表示にするため、更新しない
        // カード名とカードタイプのテキストは画像で表示するため無効化
        
        // 以下は画像表示のためコメントアウト
        /*
        // カード名
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }
        
        // カードタイプ
        if (cardTypeText != null)
        {
            cardTypeText.text = GetCardTypeString(cardData.type);
        }
        */
    }
    
    /// <summary>
    /// ボタンをセットアップ
    /// </summary>
    private void SetupButton()
    {
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveClicked);
        }
    }
    
    /// <summary>
    /// 削除ボタンがクリックされた
    /// </summary>
    private void OnRemoveClicked()
    {
        if (onRemoveCallback != null && cardData != null)
        {
            onRemoveCallback(cardData);
        }
    }
    
    /// <summary>
    /// カードタイプを文字列に変換
    /// </summary>
    private string GetCardTypeString(CardType type)
    {
        switch (type)
        {
            case CardType.Attack:
                return "攻撃";
            case CardType.Move:
                return "移動";
            case CardType.Heal:
                return "回復";
            default:
                return "不明";
        }
    }
    
    /// <summary>
    /// マウスオーバー時の処理
    /// </summary>
    public void OnPointerEnter()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
    }
    
    /// <summary>
    /// マウスアウト時の処理
    /// </summary>
    public void OnPointerExit()
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = normalColor;
        }
    }
    
    /// <summary>
    /// カードデータを取得
    /// </summary>
    public CardDataSO GetCardData()
    {
        return cardData;
    }
} 