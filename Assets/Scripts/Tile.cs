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

    private bool isSelected = false;

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
} 