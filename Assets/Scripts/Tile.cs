using UnityEngine;

[DefaultExecutionOrder(-10)]
public class Tile : MonoBehaviour
{
    public int x;
    public int y;

    public SpriteRenderer spriteRenderer;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;
    public Color selectedColor = Color.blue;
    public Color attackableColor = Color.red; // 攻撃可能タイル用の色
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
    
    // 現在の可視状態を取得するプロパティ
    public VisibilityState CurrentVisibility => currentVisibility;
    
    // 現在の透明度を取得するプロパティ
    public float CurrentAlpha
    {
        get
        {
            switch (currentVisibility)
            {
                case VisibilityState.Hidden:
                    return 0f;
                case VisibilityState.Transparent:
                    return 0.4f; // 半透明時の透明度
                case VisibilityState.Visible:
                    return 1f;
                default:
                    return 1f;
            }
        }
    }
    


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

    // マウスクリックで移動処理を実行
    private void OnMouseDown()
    {
        Debug.Log($"Tile: クリックされました 位置: ({x}, {y})");
        
        // この位置に敵がいるかチェック
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemies();
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.gridPosition == new Vector2Int(x, y))
                {
                    Debug.Log($"Tile: この位置に敵がいます。敵のクリック処理を優先します");
                    // 敵のクリック処理を優先
                    if (Player.Instance != null)
                    {
                        Player.Instance.OnEnemyClicked(new Vector2Int(x, y));
                    }
                    return; // タイルのクリック処理をスキップ
                }
            }
        }
        
        // 敵がいない場合は通常のタイルクリック処理
        if (Player.Instance != null)
        {
            Player.Instance.OnTileClicked(new Vector2Int(x, y));
        }
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
    
    // 攻撃可能タイルをハイライト
    public void HighlightAttackable()
    {
        spriteRenderer.color = attackableColor;
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