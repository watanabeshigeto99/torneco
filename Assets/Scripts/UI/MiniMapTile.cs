using UnityEngine;
using UnityEngine.UI;

public class MiniMapTile : MonoBehaviour
{
    private Vector2Int gridPos;
    public Image image;

    public void Initialize(Vector2Int pos)
    {
        gridPos = pos;
        name = $"MiniMapTile_{pos.x}_{pos.y}";
        
        // Imageコンポーネントの取得
        if (image == null)
            image = GetComponent<Image>();
            
        // ミニマップタイルのサイズを設定
        SetTileSize();
    }
    
    private void SetTileSize()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // GridLayoutGroupがサイズを制御するため、ここでは設定しない
            // GridLayoutGroupのcellSizeでサイズが決定される
            Debug.Log($"MiniMapTile: タイルサイズ設定 - {rectTransform.sizeDelta}");
        }
    }
    
    /// <summary>
    /// タイルの位置を取得
    /// </summary>
    public Vector2Int GetGridPosition()
    {
        return gridPos;
    }

    public void SetColor(Color c)
    {
        if (image != null)
            image.color = c;
    }
} 