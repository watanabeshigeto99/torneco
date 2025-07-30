using UnityEngine;

[DefaultExecutionOrder(-20)]
public class Enemy : Unit
{
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;
    
    // SOデータ
    [Header("Enemy Data")]
    public EnemyDataSO enemyData;
    
    // 動的な状態
    private bool hasMovedThisTurn = false;
    private Vector2Int lastPosition;

    protected override void Awake()
    {
        base.Awake();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
        SetupForClicking();
    }
    
    // 初期化（SOデータを使用）
    public void Initialize(Vector2Int startPos, EnemyDataSO data = null)
    {
        gridPosition = startPos;
        lastPosition = startPos;
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // SOデータを設定
        if (data != null)
        {
            enemyData = data;
            ApplyEnemyData();
        }
        else if (enemyData != null)
        {
            ApplyEnemyData();
        }
    }
    
    // SOデータを適用
    private void ApplyEnemyData()
    {
        if (enemyData == null) return;
        
        // HP設定
        maxHP = enemyData.maxHP;
        currentHP = maxHP;
        
        // スプライト設定
        if (spriteRenderer != null)
        {
            if (enemyData.sprite != null)
            {
                spriteRenderer.sprite = enemyData.sprite;
            }
            spriteRenderer.color = enemyData.spriteColor;
            spriteRenderer.sortingOrder = enemyData.sortingOrder;
        }
        
        // Collider2D設定
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = enemyData.colliderSize;
        }
    }
    
    // クリック可能にするための設定
    private void SetupForClicking()
    {
        // Collider2Dの設定を確認・調整
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            if (enemyData != null)
            {
                collider.size = enemyData.colliderSize;
            }
            else
            {
                collider.size = new Vector2(1f, 1f);
            }
        }
        else
        {
            // Collider2Dがない場合は追加
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = enemyData != null ? enemyData.colliderSize : new Vector2(1f, 1f);
        }
        
        // SpriteRendererのSortingOrderを設定
        if (spriteRenderer != null)
        {
            int order = enemyData != null ? enemyData.sortingOrder : 10;
            spriteRenderer.sortingOrder = order;
        }
    }
    
    // マウスクリックで攻撃対象として選択
    private void OnMouseDown()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnEnemyClicked(gridPosition);
        }
    }

    public void Act()
    {
        hasMovedThisTurn = false;
        lastPosition = gridPosition;
        
        // 移動パターンに応じて行動
        if (enemyData != null)
        {
            switch (enemyData.movementPattern)
            {
                case MovementPattern.Random:
                    MoveRandomly();
                    break;
                case MovementPattern.Chase:
                    MoveTowardsPlayer();
                    break;
                case MovementPattern.Patrol:
                    MoveInPatrol();
                    break;
                case MovementPattern.Stationary:
                    break;
            }
        }
        else
        {
            MoveRandomly();
        }
        
        // プレイヤーとの距離をチェックして、遠い場合は追跡移動を追加
        if (Player.Instance != null)
        {
            Vector2Int playerPos = Player.Instance.gridPosition;
            int distanceToPlayer = GetGridDistance(playerPos, gridPosition);
            
            if (distanceToPlayer > 3)
            {
                MoveTowardsPlayer();
            }
        }
        
        // 攻撃パターンに応じて攻撃
        if (enemyData != null && enemyData.canAttack)
        {
            switch (enemyData.attackPattern)
            {
                case AttackPattern.Melee:
                    TryAttackPlayerMelee();
                    break;
                case AttackPattern.Ranged:
                    TryAttackPlayerRanged();
                    break;
                case AttackPattern.Area:
                    TryAttackPlayerArea();
                    break;
                case AttackPattern.Special:
                    TryAttackPlayerSpecial();
                    break;
            }
        }
        else
        {
            TryAttackPlayerMelee();
        }
    }
    
    // ランダム移動
    private void MoveRandomly()
    {
        if (enemyData != null && !enemyData.canMove) return;
        
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        
        // プレイヤーとの距離をチェック
        Vector2Int preferredDirection = Vector2Int.zero;
        if (Player.Instance != null)
        {
            Vector2Int playerPos = Player.Instance.gridPosition;
            int distanceToPlayer = GetGridDistance(playerPos, gridPosition);
            
            if (distanceToPlayer > 2)
            {
                preferredDirection = NormalizeVector2Int(playerPos - gridPosition);
            }
        }
        
        Vector2Int dir;
        if (preferredDirection != Vector2Int.zero && UnityEngine.Random.Range(0f, 1f) < 0.7f)
        {
            dir = preferredDirection;
        }
        else
        {
            dir = directions[UnityEngine.Random.Range(0, directions.Length)];
        }
        
        Vector2Int newPos = gridPosition + dir;

        if (GridManager.Instance.IsInsideGrid(newPos) && !GridManager.Instance.IsOccupied(newPos))
        {
            gridPosition = newPos;
            transform.position = GridManager.Instance.GetWorldPosition(gridPosition);
            hasMovedThisTurn = true;
            
            // 敵の移動後に視界範囲を更新
            if (Player.Instance != null && GridManager.Instance != null)
            {
                GridManager.Instance.UpdateTileVisibility(Player.Instance.gridPosition);
            }
        }
    }
    
    // プレイヤーを追跡
    private void MoveTowardsPlayer()
    {
        if (enemyData != null && !enemyData.canMove) return;
        if (Player.Instance == null) return;
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        Vector2Int direction = NormalizeVector2Int(playerPos - gridPosition);
        Vector2Int newPos = gridPosition + direction;

        if (GridManager.Instance.IsInsideGrid(newPos) && !GridManager.Instance.IsOccupied(newPos))
        {
            gridPosition = newPos;
            transform.position = GridManager.Instance.GetWorldPosition(gridPosition);
            hasMovedThisTurn = true;
            
            if (Player.Instance != null && GridManager.Instance != null)
            {
                GridManager.Instance.UpdateTileVisibility(Player.Instance.gridPosition);
            }
        }
        else
        {
            // 追跡移動が失敗した場合、ランダム移動を試行
            MoveRandomly();
        }
    }
    
    // Vector2Intを正規化するヘルパーメソッド
    private Vector2Int NormalizeVector2Int(Vector2Int vector)
    {
        if (vector.x == 0 && vector.y == 0) return Vector2Int.zero;
        
        // 最も大きな成分を1に正規化
        int maxComponent = Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        return new Vector2Int(
            Mathf.RoundToInt((float)vector.x / maxComponent),
            Mathf.RoundToInt((float)vector.y / maxComponent)
        );
    }
    
    // パトロール移動
    private void MoveInPatrol()
    {
        if (enemyData != null && !enemyData.canMove) return;
        MoveRandomly();
    }
    
    // グリッドベースの距離計算（チェビシェフ距離）
    private int GetGridDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Max(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y));
    }
    
    // 近接攻撃
    private void TryAttackPlayerMelee()
    {
        if (Player.Instance == null) return;
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = 1;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower : 1;
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 遠距離攻撃
    private void TryAttackPlayerRanged()
    {
        if (Player.Instance == null) return;
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = enemyData != null ? enemyData.rangedAttackRange : 2;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower : 1;
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 範囲攻撃
    private void TryAttackPlayerArea()
    {
        if (Player.Instance == null) return;
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = 2;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower : 1;
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 特殊攻撃
    private void TryAttackPlayerSpecial()
    {
        if (Player.Instance == null) return;
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = 1;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower * 2 : 2;
            Player.Instance.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        int oldHP = currentHP;
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        int actualDamage = oldHP - currentHP;
        
        string enemyName = enemyData != null ? enemyData.enemyName : "敵";
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"{enemyName}が{actualDamage}ダメージを受けた！HP: {currentHP}/{maxHP}");
        }
        
        if (IsDead)
        {
            Die();
        }
    }

    protected override void Die()
    {
        string enemyName = enemyData != null ? enemyData.enemyName : "敵";
        
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