using UnityEngine;

[DefaultExecutionOrder(-20)]
public class Enemy : Unit
{
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
        // 敵のクリックを確実にするための設定
        SetupForClicking();
    }
    
    // クリック可能にするための設定
    private void SetupForClicking()
    {
        // Collider2Dの設定を確認・調整
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true; // トリガーとして設定
            collider.size = new Vector2(1f, 1f); // サイズを確実に設定
        }
        else
        {
            // Collider2Dがない場合は追加
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1f, 1f);
        }
        
        // SpriteRendererのSortingOrderをタイルより高く設定
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = 10; // タイル（通常0-5）より高い値
        }
        
        Debug.Log("Enemy: クリック設定完了");
    }

    public void Initialize(Vector2Int startPos)
    {
        gridPosition = startPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPosition);
    }
    
    // マウスクリックで攻撃対象として選択
    private void OnMouseDown()
    {
        Debug.Log($"Enemy: クリックされました 位置: {gridPosition}");
        
        if (Player.Instance != null)
        {
            // プレイヤーが攻撃選択中の場合、敵をクリックしたらその位置を攻撃対象として処理
            Player.Instance.OnEnemyClicked(gridPosition);
        }
    }

    public void Act()
    {
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        Vector2Int dir = directions[Random.Range(0, directions.Length)];
        Vector2Int newPos = gridPosition + dir;

        if (GridManager.Instance.IsInsideGrid(newPos) && !GridManager.Instance.IsOccupied(newPos))
        {
            gridPosition = newPos;
            transform.position = GridManager.Instance.GetWorldPosition(gridPosition);
            
            // 敵の移動後に視界範囲を更新
            if (Player.Instance != null && GridManager.Instance != null)
            {
                GridManager.Instance.UpdateTileVisibility(Player.Instance.gridPosition);
            }
        }

        TryAttackPlayer();
    }

    private void TryAttackPlayer()
    {
        if (Player.Instance != null)
        {
            Vector2Int playerPos = Player.Instance.gridPosition;
            if (Vector2Int.Distance(playerPos, gridPosition) == 1)
            {
                Player.Instance.TakeDamage(1); // 1ダメージ
                Debug.Log("敵の攻撃！");
            }
        }
    }

    public void TakeDamage(int damage)
    {
        int oldHP = currentHP;
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        int actualDamage = oldHP - currentHP;
        
        Debug.Log($"敵が{actualDamage}ダメージを受けた！HP: {currentHP}/{maxHP}");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"敵が{actualDamage}ダメージを受けた！HP: {currentHP}/{maxHP}");
        }
        
        if (IsDead)
        {
            Die();
        }
    }



    protected override void Die()
    {
        Debug.Log("敵を倒した！");
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Death");
        }
        
        // スコア加算
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }
        
        Destroy(gameObject);
    }
    
}