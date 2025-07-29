using UnityEngine;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    public Image iconImage;
    public Button button;

    private CardDataSO cardData;

    public void Setup(CardDataSO data, System.Action<CardDataSO> onClick)
    {
        cardData = data;
        iconImage.sprite = data.icon;
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

    public CardDataSO GetCardData()
    {
        return cardData;
    }
} 