using UnityEngine;

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
        
        // 視界範囲更新はGridManagerに任せる（初期化タイミングの問題を回避）
        Debug.Log($"敵を初期化: 位置{startPos}");
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
            Player player = FindObjectOfType<Player>();
            if (player != null && GridManager.Instance != null)
            {
                GridManager.Instance.UpdateTileVisibility(player.gridPosition);
            }
        }

        TryAttackPlayer();
    }

    private void TryAttackPlayer()
    {
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            Vector2Int playerPos = player.gridPosition;
            if (Vector2Int.Distance(playerPos, gridPosition) == 1)
            {
                player.TakeDamage(1); // 1ダメージ
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