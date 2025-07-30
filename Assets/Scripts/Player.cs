using UnityEngine;

[DefaultExecutionOrder(-30)]
public class Player : Unit
{
    public static Player Instance { get; private set; }
    
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;

    private bool isAwaitingMoveInput = false;
    private int allowedMoveDistance = 0;

    protected override void Awake()
    {
        // 親クラスのAwakeを呼び出し
        base.Awake();
        
        // Singletonパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // コンポーネント参照の取得
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // 基本的な変数の初期化
        gridPosition = new Vector2Int(2, 2); // 5x5グリッドの中央
        isAwaitingMoveInput = false;
        allowedMoveDistance = 0;
    }

    // 初期化完了後に呼ばれるメソッド
    public void InitializePosition()
    {
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
        }
    }

    public void StartMoveSelection(int moveDistance)
    {
        isAwaitingMoveInput = true;
        allowedMoveDistance = moveDistance;
        GridManager.Instance.HighlightMovableTiles(gridPosition, allowedMoveDistance);
    }

    public void OnTileClicked(Vector2Int clickedPos)
    {
        if (!isAwaitingMoveInput) return;

        int dist = Mathf.Abs(clickedPos.x - gridPosition.x) + Mathf.Abs(clickedPos.y - gridPosition.y);
        
        if (dist <= allowedMoveDistance && GridManager.Instance.IsWalkable(clickedPos))
        {
            gridPosition = clickedPos;
            transform.position = GridManager.Instance.GetWorldPosition(gridPosition);

            isAwaitingMoveInput = false;
            GridManager.Instance.ResetAllTileColors();

            // カメラ追従と視界範囲更新
            NotifyCameraFollow();
            
            // ターン終了
            TurnManager.Instance.OnPlayerCardUsed();
        }
    }

    public void Move(Vector2Int delta)
    {
        Vector2Int newPos = gridPosition + delta;
        
        if (GridManager.Instance.IsWalkable(newPos))
        {
            gridPosition = newPos;
            Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
            transform.position = worldPos;
            
            // カメラ追従通知
            NotifyCameraFollow();
        }
    }

    public void MoveWithDistance(Vector2Int direction, int distance)
    {
        Vector2Int totalMove = direction * distance;
        Vector2Int newPos = gridPosition + totalMove;
        
        if (GridManager.Instance.IsWalkable(newPos))
        {
            gridPosition = newPos;
            Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
            transform.position = worldPos;
            Debug.Log($"移動！方向: {direction}, 距離: {distance}");
            
            // カメラ追従通知
            NotifyCameraFollow();
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"移動！方向: {direction}, 距離: {distance}");
            }
            
            // 効果音再生（移動用の効果音があれば）
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("Select"); // 仮でSelect音を使用
            }
        }
        else
        {
            Debug.Log($"移動できません！位置: {newPos}");
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"移動できません！位置: {newPos}");
            }
        }
    }

    public void SetPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // カメラ追従通知
        NotifyCameraFollow();
    }

    public void ExecuteCardEffect(CardDataSO card)
    {
        if (card == null) return;

        switch (card.type)
        {
            case CardType.Attack:
                Attack(card.power);
                break;
            case CardType.Heal:
                Heal(card.healAmount);
                break;
            case CardType.Move:
                MoveWithDistance(card.moveDirection, card.moveDistance);
                break;
        }
    }

    public void Attack(int damage)
    {
        Debug.Log($"攻撃！ダメージ: {damage}");
        
        // 隣接マスにいる敵を探して攻撃
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        bool hitEnemy = false;
        
        foreach (Enemy enemy in enemies)
        {
            if (enemy != null && Vector2Int.Distance(gridPosition, enemy.gridPosition) == 1)
            {
                enemy.TakeDamage(damage);
                hitEnemy = true;
                Debug.Log($"敵に{damage}ダメージを与えた！");
                
                // UI更新
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.AddLog($"敵に{damage}ダメージを与えた！");
                }
            }
        }
        
        if (!hitEnemy)
        {
            Debug.Log("攻撃範囲に敵がいません");
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲に敵がいません");
            }
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Attack");
        }
    }

    public Vector2Int[] GetAttackRange()
    {
        // 攻撃範囲（隣接マス）を返す
        return new Vector2Int[]
        {
            gridPosition + Vector2Int.up,
            gridPosition + Vector2Int.down,
            gridPosition + Vector2Int.left,
            gridPosition + Vector2Int.right
        };
    }

    private void NotifyCameraFollow()
    {
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.OnPlayerMoved(transform.position);
        }
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
        }
    }

    public void Heal(int amount)
    {
        int oldHP = currentHP;
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        int actualHeal = currentHP - oldHP;
        
        Debug.Log($"回復！回復量: {actualHeal}, HP: {currentHP}/{maxHP}");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.AddLog($"回復！回復量: {actualHeal}, HP: {currentHP}/{maxHP}");
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Heal");
        }
    }

    protected override void Die()
    {
        Debug.Log("Game Over!");
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
} 