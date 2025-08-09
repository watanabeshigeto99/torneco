using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CardUI : MonoBehaviour
{
    public Image iconImage;
    public Button button;
    
    [Header("Level Display")]
    public TextMeshProUGUI levelLabel; // レベル表示用
    public GameObject levelIcon; // レベルアイコン
    
    [Header("Enhancement Effects")]
    public ParticleSystem enhanceEffect; // 強化エフェクト
    public float enhanceAnimationDuration = 0.5f;
    
    [Header("Upgrade Button")]
    public Button upgradeButton; // 強化ボタン
    public GameObject upgradeButtonContainer; // 強化ボタンのコンテナ
    
    private CardDataSO cardData;
    private int previousLevel = 1;

    public void Setup(CardDataSO data, System.Action<CardDataSO> onClick)
    {
        cardData = data;
        previousLevel = data.level;
        iconImage.sprite = data.icon;
        
        // レベル表示を更新
        UpdateLevelDisplay();
        
        // 強化ボタンの設定
        SetupUpgradeButton();
        
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => {
            // 効果音再生
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("Select");
            }
            onClick?.Invoke(cardData);
        });
    }
    
    /// <summary>
    /// 強化ボタンの設定
    /// </summary>
    private void SetupUpgradeButton()
    {
        if (upgradeButton == null) return;
        
        // 強化機能が有効な場合のみ表示
        bool showUpgradeButton = CardUpgradeFeature.IsEnabled;
        
        if (upgradeButtonContainer != null)
        {
            upgradeButtonContainer.SetActive(showUpgradeButton);
        }
        else if (upgradeButton != null)
        {
            upgradeButton.gameObject.SetActive(showUpgradeButton);
        }
        
        if (showUpgradeButton)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
        }
    }
    
    /// <summary>
    /// 強化ボタンクリック
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (cardData == null) return;
        
        // 強化モーダルを表示
        var upgradeModal = FindObjectOfType<CardUpgradeModal>();
        if (upgradeModal != null)
        {
            upgradeModal.ShowModal(cardData);
        }
        else
        {
            Debug.LogWarning("CardUI: CardUpgradeModalが見つかりません");
        }
    }
    
    // レベル表示を更新
    public void UpdateLevelDisplay()
    {
        if (cardData == null) return;
        
        // 画像表示のため、テキスト表示は無効化
        // レベルラベルとレベルアイコンは非表示
        
        // 以下は画像表示のためコメントアウト
        /*
        // レベルラベルを更新
        if (levelLabel != null)
        {
            levelLabel.text = $"Lv.{cardData.level}";
            
            // レベルに応じて色を変更
            if (cardData.level >= 3)
            {
                levelLabel.color = Color.yellow; // 高レベルは黄色
            }
            else if (cardData.level >= 2)
            {
                levelLabel.color = Color.green; // 中レベルは緑色
            }
            else
            {
                levelLabel.color = Color.white; // 低レベルは白色
            }
        }
        
        // レベルアイコンを表示/非表示
        if (levelIcon != null)
        {
            levelIcon.SetActive(cardData.level > 1);
        }
        */
        
        // レベルアップ演出をチェック
        if (cardData.level > previousLevel)
        {
            StartCoroutine(AnimateEnhancement());
            previousLevel = cardData.level;
        }
    }
    
    // 強化演出
    private IEnumerator AnimateEnhancement()
    {
        // 強化エフェクトを再生
        if (enhanceEffect != null)
        {
            enhanceEffect.Play();
        }
        
        // カードを光らせる演出
        float elapsed = 0f;
        Vector3 originalScale = transform.localScale;
        Color originalColor = iconImage.color;
        
        while (elapsed < enhanceAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / enhanceAnimationDuration;
            
            // スケールアニメーション
            float scale = 1f + Mathf.Sin(t * Mathf.PI * 2) * 0.2f;
            transform.localScale = originalScale * scale;
            
            // 色アニメーション（金色に光らせる）
            Color targetColor = Color.Lerp(originalColor, Color.yellow, Mathf.Sin(t * Mathf.PI * 2));
            iconImage.color = targetColor;
            
            yield return null;
        }
        
        // 元の状態に戻す
        transform.localScale = originalScale;
        iconImage.color = originalColor;
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Enhance");
        }
    }

    public CardDataSO GetCardData()
    {
        return cardData;
    }
} 