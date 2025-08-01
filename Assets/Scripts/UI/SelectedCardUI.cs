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
    
    private CardDataSO cardData;
    private Action<CardDataSO> onRemoveCallback;
    
    /// <summary>
    /// 選択されたカードをセットアップ
    /// </summary>
    public void Setup(CardDataSO card, Action<CardDataSO> onRemove)
    {
        if (card == null)
        {
            Debug.LogError("SelectedCardUI: カードデータがnullです");
            return;
        }
        
        cardData = card;
        onRemoveCallback = onRemove;
        
        Debug.Log($"SelectedCardUI: カードセットアップ開始 - {card.cardName} (アイコン: {(card.icon != null ? card.icon.name : "null")})");
        
        UpdateDisplay();
        SetupButton();
        
        Debug.Log($"SelectedCardUI: 選択カードセットアップ完了 - {card.cardName}");
    }
    
    /// <summary>
    /// 表示を更新
    /// </summary>
    private void UpdateDisplay()
    {
        if (cardData == null) return;
        
        // アイコン
        if (cardIcon != null)
        {
            if (cardData.icon != null)
            {
                cardIcon.sprite = cardData.icon;
                cardIcon.enabled = true;
                Debug.Log($"SelectedCardUI: アイコン設定完了 - {cardData.cardName} -> {cardData.icon.name}");
            }
            else
            {
                cardIcon.enabled = false;
                Debug.LogWarning($"SelectedCardUI: {cardData.cardName}のアイコンが設定されていません");
            }
        }
        else
        {
            Debug.LogError("SelectedCardUI: cardIconが設定されていません");
        }
        
        // カード名
        if (cardNameText != null)
        {
            cardNameText.text = cardData.cardName;
        }
        else
        {
            Debug.LogError("SelectedCardUI: cardNameTextが設定されていません");
        }
        
        // カードタイプ
        if (cardTypeText != null)
        {
            cardTypeText.text = GetCardTypeString(cardData.type);
        }
        else
        {
            Debug.LogError("SelectedCardUI: cardTypeTextが設定されていません");
        }
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