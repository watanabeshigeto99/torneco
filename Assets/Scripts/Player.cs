using UnityEngine;
using System;

[DefaultExecutionOrder(-30)]
public class Player : Unit
{
    public static Player Instance { get; private set; }
    
    // イベント定義
    public static event Action<Vector2Int> OnPlayerMoved;
    public static event Action<int> OnPlayerAttacked;
    public static event Action<int> OnPlayerHealed;
    public static event Action OnPlayerDied;
    
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;

    private bool isAwaitingMoveInput = false;
    private int allowedMoveDistance = 0;

    protected override void Awake()
    {
        // 親クラスのAwakeを呼び出し
        base.Awake();
        
        Debug.Log("Player: Awake開始");
        
        // Singletonパターンの実装
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Player: 重複するPlayerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // コンポーネント参照の取得
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer == null)
        {
            Debug.LogError("Player: SpriteRendererが見つかりません");
        }
        
        // 基本的な変数の初期化
        gridPosition = new Vector2Int(2, 2); // 5x5グリッドの中央
        isAwaitingMoveInput = false;
        allowedMoveDistance = 0;
        
        Debug.Log("Player: Awake完了");
    }

    // 初期化完了後に呼ばれるメソッド
    public void InitializePosition()
    {
        Debug.Log($"Player: 位置初期化開始 位置: {gridPosition}");
        
        if (GridManager.Instance == null)
        {
            Debug.LogError("Player: GridManager.Instanceが見つかりません");
            return;
        }
        
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
            Debug.Log("Player: 視界範囲更新完了");
        }
        else
        {
            Debug.LogError("Player: GridManager.Instanceが見つからないため視界範囲更新をスキップ");
        }
        
        Debug.Log($"Player: 位置初期化完了 ワールド位置: {transform.position}");
    }

    public void StartMoveSelection(int moveDistance)
    {
        Debug.Log($"Player: 移動選択開始 距離: {moveDistance}");
        
        if (GridManager.Instance == null)
        {
            Debug.LogError("Player: GridManager.Instanceが見つかりません");
            return;
        }
        
        isAwaitingMoveInput = true;
        allowedMoveDistance = moveDistance;
        GridManager.Instance.HighlightMovableTiles(gridPosition, allowedMoveDistance);
        
        Debug.Log($"Player: 移動選択完了 現在位置: {gridPosition}");
    }

    public void OnTileClicked(Vector2Int clickedPos)
    {
        if (!isAwaitingMoveInput)
        {
            Debug.Log("Player: 移動待ち状態ではないためクリックを無視");
            return;
        }

        Debug.Log($"Player: タイルクリック受信 位置: {clickedPos}");
        
        if (GridManager.Instance == null)
        {
            Debug.LogError("Player: GridManager.Instanceが見つかりません");
            return;
        }

        int dist = Mathf.Abs(clickedPos.x - gridPosition.x) + Mathf.Abs(clickedPos.y - gridPosition.y);
        
        if (dist <= allowedMoveDistance && GridManager.Instance.IsWalkable(clickedPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = clickedPos;
            transform.position = GridManager.Instance.GetWorldPosition(gridPosition);

            isAwaitingMoveInput = false;
            GridManager.Instance.ResetAllTileColors();

            // 移動イベントを発行
            OnPlayerMoved?.Invoke(gridPosition);
            Debug.Log($"Player: 移動イベント発行 {oldPos} → {gridPosition}");

            // カメラ追従と視界範囲更新
            NotifyCameraFollow();
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
                Debug.Log($"Player: 移動完了 {oldPos} → {gridPosition}");
            }
            else
            {
                Debug.LogError("Player: TurnManager.Instanceが見つかりません");
            }
        }
        else
        {
            Debug.Log($"Player: 移動不可 距離: {dist}, 最大距離: {allowedMoveDistance}, 歩行可能: {GridManager.Instance.IsWalkable(clickedPos)}");
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
        
        // 攻撃イベントを発行
        OnPlayerAttacked?.Invoke(damage);
        Debug.Log($"Player: 攻撃イベント発行 ダメージ: {damage}");
        
        // 隣接マスにいる敵を探して攻撃（EnemyManagerから取得）
        bool hitEnemy = false;
        
        if (EnemyManager.Instance != null)
        {
            // EnemyManagerから敵リストを取得
            var enemies = EnemyManager.Instance.GetEnemies();
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
        }
        else
        {
            Debug.LogWarning("Player: EnemyManager.Instanceが見つからないため攻撃をスキップ");
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
        
        // 回復イベントを発行
        OnPlayerHealed?.Invoke(actualHeal);
        Debug.Log($"Player: 回復イベント発行 回復量: {actualHeal}");
        
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
        
        // 死亡イベントを発行
        OnPlayerDied?.Invoke();
        Debug.Log("Player: 死亡イベント発行");
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
} 