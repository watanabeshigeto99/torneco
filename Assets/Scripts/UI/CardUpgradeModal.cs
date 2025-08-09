using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// カード強化モーダル
/// </summary>
public class CardUpgradeModal : MonoBehaviour
{
    [Header("UI References")]
    public GameObject modalPanel;
    public Image cardIcon;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI nextLevelText;
    public TextMeshProUGUI currentValueText;
    public TextMeshProUGUI nextValueText;
    public TextMeshProUGUI successRateText;
    public TextMeshProUGUI requiredGoldText;
    public TextMeshProUGUI requiredShardsText;
    public Button upgradeButton;
    public Button closeButton;
    public TextMeshProUGUI upgradeButtonText;
    
    [Header("Loading")]
    public GameObject loadingIndicator;
    
    [Header("Result")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button resultCloseButton;
    
    private CardDataSO currentCardData;
    private UpgradePreview currentPreview;
    private bool isUpgrading = false;
    
    private void Awake()
    {
        // ボタンイベントを設定
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        
        if (resultCloseButton != null)
            resultCloseButton.onClick.AddListener(OnResultCloseButtonClicked);
    }
    
    /// <summary>
    /// モーダルを表示
    /// </summary>
    public void ShowModal(CardDataSO cardData)
    {
        if (!CardUpgradeFeature.IsEnabled)
        {
            Debug.LogWarning("CardUpgradeModal: 強化機能が無効です");
            return;
        }
        
        currentCardData = cardData;
        currentPreview = null;
        isUpgrading = false;
        
        // UIを初期化
        InitializeUI();
        
        // プレビューを取得
        LoadUpgradePreview();
        
        // モーダルを表示
        if (modalPanel != null)
            modalPanel.SetActive(true);
        
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
    
    /// <summary>
    /// UIを初期化
    /// </summary>
    private void InitializeUI()
    {
        if (currentCardData == null) return;
        
        // カード情報を表示
        if (cardIcon != null)
            cardIcon.sprite = currentCardData.icon;
        
        if (cardNameText != null)
            cardNameText.text = currentCardData.cardName;
        
        // ボタンを無効化
        if (upgradeButton != null)
            upgradeButton.interactable = false;
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
    }
    
    /// <summary>
    /// 強化プレビューを読み込み
    /// </summary>
    private void LoadUpgradePreview()
    {
        if (UpgradeService.Instance == null)
        {
            Debug.LogError("CardUpgradeModal: UpgradeServiceが見つかりません");
            return;
        }
        
        currentPreview = UpgradeService.Instance.GetPreview(currentCardData.cardName);
        UpdatePreviewUI();
    }
    
    /// <summary>
    /// プレビューUIを更新
    /// </summary>
    private void UpdatePreviewUI()
    {
        if (currentPreview == null) return;
        
        // レベル情報
        if (currentLevelText != null)
            currentLevelText.text = $"Lv.{currentPreview.currentLevel}";
        
        if (nextLevelText != null)
            nextLevelText.text = $"Lv.{currentPreview.nextLevel}";
        
        // 効果値情報
        if (currentValueText != null)
            currentValueText.text = currentPreview.currentValue.ToString();
        
        if (nextValueText != null)
            nextValueText.text = currentPreview.nextValue.ToString();
        
        // 成功率
        if (successRateText != null)
            successRateText.text = $"{(currentPreview.successRate * 100):F0}%";
        
        // 必要リソース
        if (requiredGoldText != null)
            requiredGoldText.text = currentPreview.requiredGold.ToString();
        
        if (requiredShardsText != null)
            requiredShardsText.text = currentPreview.requiredShards.ToString();
        
        // ボタン状態
        if (upgradeButton != null)
        {
            upgradeButton.interactable = !currentPreview.isMaxLevel && !isUpgrading;
            upgradeButtonText.text = currentPreview.isMaxLevel ? "最大レベル" : "強化";
        }
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
    }
    
    /// <summary>
    /// 強化ボタンクリック
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (isUpgrading || currentPreview == null || currentPreview.isMaxLevel)
            return;
        
        StartCoroutine(PerformUpgrade());
    }
    
    /// <summary>
    /// 強化を実行
    /// </summary>
    private IEnumerator PerformUpgrade()
    {
        isUpgrading = true;
        
        // ボタンを無効化
        if (upgradeButton != null)
            upgradeButton.interactable = false;
        
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
        
        // 強化を実行
        var result = UpgradeService.Instance.TryUpgrade(currentCardData.cardName);
        
        // 少し待機（演出のため）
        yield return new WaitForSeconds(0.5f);
        
        // 結果を表示
        ShowResult(result);
        
        isUpgrading = false;
    }
    
    /// <summary>
    /// 結果を表示
    /// </summary>
    private void ShowResult(UpgradeResult result)
    {
        if (resultPanel == null || resultText == null) return;
        
        // 結果テキストを設定
        string resultMessage = result.success ? 
            $"強化に成功しました！\nLv.{result.previousLevel} → Lv.{result.newLevel}" :
            $"強化に失敗しました\n{result.message}";
        
        resultText.text = resultMessage;
        
        // 結果パネルを表示
        resultPanel.SetActive(true);
        
        // プレビューを再読み込み
        LoadUpgradePreview();
    }
    
    /// <summary>
    /// 閉じるボタンクリック
    /// </summary>
    private void OnCloseButtonClicked()
    {
        if (modalPanel != null)
            modalPanel.SetActive(false);
    }
    
    /// <summary>
    /// 結果閉じるボタンクリック
    /// </summary>
    private void OnResultCloseButtonClicked()
    {
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
    
    /// <summary>
    /// モーダルを閉じる
    /// </summary>
    public void CloseModal()
    {
        if (modalPanel != null)
            modalPanel.SetActive(false);
        
        if (resultPanel != null)
            resultPanel.SetActive(false);
    }
}
