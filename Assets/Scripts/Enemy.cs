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
    }

    public void Initialize(Vector2Int startPos)
    {
        gridPosition = startPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPosition);
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