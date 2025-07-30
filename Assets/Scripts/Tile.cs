using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour, IPointerClickHandler
{
    public int x;
    public int y;

    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color selectedColor = Color.blue;
    public Color transparentColor = new Color(1f, 1f, 1f, 0.5f); // 半透明色

    private bool isSelected = false;
    
    // 表示状態の列挙型
    public enum VisibilityState
    {
        Hidden,     // 非表示
        Transparent, // 半透明表示
        Visible     // 通常表示
    }
    
    private VisibilityState currentVisibility = VisibilityState.Visible;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        ResetColor();
    }

    public void Initialize(int xPos, int yPos)
    {
        x = xPos;
        y = yPos;
        name = $"Tile_{x}_{y}";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Tile clicked: ({x}, {y})");
        ToggleSelect();
    }

    public void ToggleSelect()
    {
        isSelected = !isSelected;
        spriteRenderer.color = isSelected ? selectedColor : normalColor;
    }

    public void ResetColor()
    {
        spriteRenderer.color = normalColor;
    }

    public void Highlight()
    {
        spriteRenderer.color = highlightColor;
    }
    
    // 表示状態を設定
    public void SetVisibility(VisibilityState state)
    {
        currentVisibility = state;
        
        switch (state)
        {
            case VisibilityState.Hidden:
                gameObject.SetActive(false);
                break;
            case VisibilityState.Transparent:
                gameObject.SetActive(true);
                spriteRenderer.color = transparentColor;
                break;
            case VisibilityState.Visible:
                gameObject.SetActive(true);
                spriteRenderer.color = normalColor;
                break;
        }
    }
    
    // 現在の表示状態を取得
    public VisibilityState GetVisibility()
    {
        return currentVisibility;
    }
    
    // 表示状態を更新（選択状態を保持）
    public void UpdateVisibility(VisibilityState state)
    {
        currentVisibility = state;
        
        switch (state)
        {
            case VisibilityState.Hidden:
                gameObject.SetActive(false);
                break;
            case VisibilityState.Transparent:
                gameObject.SetActive(true);
                spriteRenderer.color = isSelected ? selectedColor : transparentColor;
                break;
            case VisibilityState.Visible:
                gameObject.SetActive(true);
                spriteRenderer.color = isSelected ? selectedColor : normalColor;
                break;
        }
    }
} 