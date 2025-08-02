using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CardListItemUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image cardIcon;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardTypeText;
    public TextMeshProUGUI cardPowerText;
    public Button cardButton;
    
    [Header("Visual Feedback")]
    public Image backgroundImage;
    public Color normalColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color selectedColor = Color.green;
    
    private CardDataSO cardData;
    private Action<CardDataSO> onClickCallback;
    private bool isSelected = false;
    
    /// <summary>
    /// カードアイテムをセットアップ
    /// </summary>
    public void Setup(CardDataSO card, Action<CardDataSO> onClick)
    {
        cardData = card;
        onClickCallback = onClick;
        
        UpdateDisplay();
        SetupButton();
        
        Debug.Log($"CardListItemUI: カードアイテムセットアップ完了 - {card.cardName}");
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
            cardIcon.sprite = cardData.icon;
        }
        
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
        
        // カード効果
        if (cardPowerText != null)
        {
            cardPowerText.text = GetCardEffectString(cardData);
        }
    }
    
    /// <summary>
    /// ボタンをセットアップ
    /// </summary>
    private void SetupButton()
    {
        if (cardButton != null)
        {
            cardButton.onClick.RemoveAllListeners();
            cardButton.onClick.AddListener(OnCardClicked);
        }
    }
    
    /// <summary>
    /// カードがクリックされた
    /// </summary>
    private void OnCardClicked()
    {
        if (onClickCallback != null && cardData != null)
        {
            onClickCallback(cardData);
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
    /// カード効果を文字列に変換
    /// </summary>
    private string GetCardEffectString(CardDataSO card)
    {
        switch (card.type)
        {
            case CardType.Attack:
                return $"攻撃力: {card.GetEffectivePower()}";
            case CardType.Move:
                return $"移動距離: {card.GetEffectiveMoveDistance()}";
            case CardType.Heal:
                return $"回復量: {card.GetEffectiveHealAmount()}";
            default:
                return "";
        }
    }
    
    /// <summary>
    /// 選択状態を設定
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        UpdateVisualState();
    }
    
    /// <summary>
    /// 視覚的状態を更新
    /// </summary>
    private void UpdateVisualState()
    {
        if (backgroundImage == null) return;
        
        if (isSelected)
        {
            backgroundImage.color = selectedColor;
        }
        else
        {
            backgroundImage.color = normalColor;
        }
    }
    
    /// <summary>
    /// マウスオーバー時の処理
    /// </summary>
    public void OnPointerEnter()
    {
        if (!isSelected && backgroundImage != null)
        {
            backgroundImage.color = hoverColor;
        }
        
        // ホバー時のSEを再生
        PlayHoverSE();
    }
    
    /// <summary>
    /// マウスアウト時の処理
    /// </summary>
    public void OnPointerExit()
    {
        if (!isSelected && backgroundImage != null)
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
    
    /// <summary>
    /// ホバー時のSEを再生
    /// </summary>
    private void PlayHoverSE()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("card_hover");
        }
    }
} 