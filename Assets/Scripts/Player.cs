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
    
    // 攻撃方向選択用の変数
    private bool isAwaitingAttackInput = false;
    private int attackPower = 0;

    protected override void Awake()
    {
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
        
        // プレイヤー固有のHP設定
        maxHP = 20;
        currentHP = maxHP;
        
        // 基本的な変数の初期化
        gridPosition = new Vector2Int(2, 2);
        isAwaitingMoveInput = false;
        allowedMoveDistance = 0;
        isAwaitingAttackInput = false;
        attackPower = 0;
    }

    // 初期化完了後に呼ばれるメソッド
    public void InitializePosition()
    {
        if (GridManager.Instance == null) return;
        
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
        }
    }

    public void StartMoveSelection(int moveDistance)
    {
        if (GridManager.Instance == null) return;
        
        isAwaitingMoveInput = true;
        allowedMoveDistance = moveDistance;
        GridManager.Instance.HighlightMovableTiles(gridPosition, allowedMoveDistance);
    }
    
    // 攻撃方向選択を開始
    public void StartAttackSelection(int power)
    {
        if (GridManager.Instance == null) return;
        
        isAwaitingAttackInput = true;
        attackPower = power;
        GridManager.Instance.HighlightAttackableTiles(gridPosition);
    }

    public void OnTileClicked(Vector2Int clickedPos)
    {
        if (GridManager.Instance == null) return;

        // 攻撃方向選択中の場合
        if (isAwaitingAttackInput)
        {
            HandleAttackDirectionSelection(clickedPos);
            return;
        }
        
        // 移動選択中の場合
        if (isAwaitingMoveInput)
        {
            HandleMoveSelection(clickedPos);
            return;
        }
    }
    
    // 攻撃方向選択の処理
    private void HandleAttackDirectionSelection(Vector2Int clickedPos)
    {
        // 攻撃可能範囲内かチェック
        Vector2Int[] attackRange = GetAttackRange();
        bool isValidAttackTarget = false;
        
        foreach (Vector2Int attackPos in attackRange)
        {
            if (attackPos == clickedPos)
            {
                isValidAttackTarget = true;
                break;
            }
        }
        
        if (isValidAttackTarget)
        {
            // 攻撃実行
            Vector2Int attackDirection = clickedPos - gridPosition;
            ExecuteAttack(attackDirection, attackPower);
            
            // 状態をリセット
            isAwaitingAttackInput = false;
            attackPower = 0;
            GridManager.Instance.ResetAllTileColors();
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
        }
        else
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲外です");
            }
        }
    }
    
    // 敵クリック時の処理
    public void OnEnemyClicked(Vector2Int enemyPos)
    {
        // 攻撃選択中でない場合は無視
        if (!isAwaitingAttackInput) return;
        
        // 攻撃可能範囲内かチェック
        Vector2Int[] attackRange = GetAttackRange();
        bool isValidAttackTarget = false;
        
        foreach (Vector2Int attackPos in attackRange)
        {
            if (attackPos == enemyPos)
            {
                isValidAttackTarget = true;
                break;
            }
        }
        
        if (isValidAttackTarget)
        {
            // 攻撃実行
            Vector2Int attackDirection = enemyPos - gridPosition;
            ExecuteAttack(attackDirection, attackPower);
            
            // 状態をリセット
            isAwaitingAttackInput = false;
            attackPower = 0;
            GridManager.Instance.ResetAllTileColors();
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
        }
        else
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲外の敵です");
            }
        }
    }
    
    // 移動選択の処理
    private void HandleMoveSelection(Vector2Int clickedPos)
    {
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

            // カメラ追従と視界範囲更新
            NotifyCameraFollow();
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
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
            
            // カメラ追従通知
            NotifyCameraFollow();
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"移動！方向: {direction}, 距離: {distance}");
            }
            
            // 効果音再生
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("Select");
            }
        }
        else
        {
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
                StartAttackSelection(card.power);
                break;
            case CardType.Heal:
                Heal(card.healAmount);
                break;
            case CardType.Move:
                MoveWithDistance(card.moveDirection, card.moveDistance);
                break;
        }
    }
    
    // 攻撃実行（方向指定）
    public void ExecuteAttack(Vector2Int direction, int damage)
    {
        Vector2Int targetPos = gridPosition + direction;
        
        // 攻撃イベントを発行
        OnPlayerAttacked?.Invoke(damage);
        
        // 目標位置にいる敵を探して攻撃
        bool hitEnemy = false;
        
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemies();
            
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.gridPosition == targetPos)
                {
                    enemy.TakeDamage(damage);
                    hitEnemy = true;
                    
                    // UI更新
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.AddLog($"敵に{damage}ダメージを与えた！");
                    }
                    
                    // 攻撃エフェクト再生
                    PlayAttackEffect(targetPos);
                    break;
                }
            }
        }
        
        if (!hitEnemy)
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲に敵がいません");
            }
            
            // 空振りエフェクト再生
            PlayMissEffect(targetPos);
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Attack");
        }
    }
    
    // 攻撃エフェクト再生
    private void PlayAttackEffect(Vector2Int targetPos)
    {
        // Unity標準のエフェクトシステムを使用
        // 将来的にパーティクルシステムなどを追加可能
    }
    
    // 空振りエフェクト再生
    private void PlayMissEffect(Vector2Int targetPos)
    {
        // Unity標準のエフェクトシステムを使用
    }

    public void Attack(int damage)
    {
        // 攻撃イベントを発行
        OnPlayerAttacked?.Invoke(damage);
        
        // 攻撃範囲内の敵を探して攻撃
        bool hitEnemy = false;
        
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemies();
            Vector2Int[] attackRange = GetAttackRange();
            
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    // 攻撃範囲内かチェック
                    bool inAttackRange = false;
                    foreach (Vector2Int attackPos in attackRange)
                    {
                        if (attackPos == enemy.gridPosition)
                        {
                            inAttackRange = true;
                            break;
                        }
                    }
                    
                    if (inAttackRange)
                    {
                        enemy.TakeDamage(damage);
                        hitEnemy = true;
                        
                        // UI更新
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.AddLog($"敵に{damage}ダメージを与えた！");
                        }
                        
                        // 1つの敵のみを攻撃（最初に見つかった敵）
                        break;
                    }
                }
            }
        }
        
        if (!hitEnemy)
        {
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
        // 攻撃範囲（隣接マス + 斜め）を返す
        return new Vector2Int[]
        {
            gridPosition + Vector2Int.up,
            gridPosition + Vector2Int.down,
            gridPosition + Vector2Int.left,
            gridPosition + Vector2Int.right,
            gridPosition + new Vector2Int(1, 1),   // 右上
            gridPosition + new Vector2Int(-1, 1),  // 左上
            gridPosition + new Vector2Int(1, -1),  // 右下
            gridPosition + new Vector2Int(-1, -1)  // 左下
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
        
        // 回復イベントを発行
        OnPlayerHealed?.Invoke(actualHeal);
        
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
        // 死亡イベントを発行
        OnPlayerDied?.Invoke();
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
} 