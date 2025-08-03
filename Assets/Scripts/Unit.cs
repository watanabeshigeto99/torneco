using UnityEngine;
using System;

public class Unit : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP;
    
    [Header("Grid Position")]
    public Vector2Int gridPosition;
    
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    
    // 状態管理
    public bool IsDead => currentHP <= 0;
    
    // 演出用の変数（共通）
    [Header("Effects")]
    public ParticleSystem moveEffect;
    public ParticleSystem attackEffect;
    public float moveAnimationDuration = 0.3f;
    public float attackAnimationDuration = 0.2f;
    public float jumpHeight = 0.5f;
    
    // イベント定義
    public static event Action<Unit, Vector2Int> OnUnitMoved;
    public static event Action<Unit, int> OnUnitAttacked;
    public static event Action<Unit, int> OnUnitDamaged;
    public static event Action<Unit> OnUnitDied;
    
    protected virtual void Awake()
    {
        currentHP = maxHP;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    public virtual void Initialize(Vector2Int startPos)
    {
        gridPosition = startPos;
        transform.position = GridManager.Instance.GetWorldPosition(startPos);
    }
    
    public virtual void TakeDamage(int damage)
    {
        if (IsDead) return;
        
        currentHP = Mathf.Max(0, currentHP - damage);
        
        // ダメージイベントを発行
        OnUnitDamaged?.Invoke(this, damage);
        
        if (IsDead)
        {
            Die();
        }
    }
    
    public virtual void Heal(int amount)
    {
        if (IsDead) return;
        
        currentHP = Mathf.Min(maxHP, currentHP + amount);
    }
    
    protected virtual void Die()
    {
        // 死亡イベントを発行
        OnUnitDied?.Invoke(this);
    }
    
    public virtual void MoveTo(Vector2Int pos)
    {
        if (!GridManager.Instance.IsInsideGrid(pos) || !GridManager.Instance.IsWalkable(pos))
            return;
        
        Vector2Int oldPos = gridPosition;
        gridPosition = pos;
        transform.position = GridManager.Instance.GetWorldPosition(pos);
        
        // 移動イベントを発行
        OnUnitMoved?.Invoke(this, pos);
    }
    
    public virtual void Move(Vector2Int delta)
    {
        Vector2Int newPos = gridPosition + delta;
        MoveTo(newPos);
    }
    
    public virtual Vector2Int GetPosition()
    {
        return gridPosition;
    }
    
    public virtual void SetPosition(Vector2Int newPos)
    {
        if (GridManager.Instance.IsInsideGrid(newPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = newPos;
            transform.position = GridManager.Instance.GetWorldPosition(newPos);
            
            // 移動イベントを発行
            OnUnitMoved?.Invoke(this, newPos);
        }
    }
    
    // 攻撃メソッド
    public virtual void Attack(Unit target, int damage)
    {
        if (target == null || target.IsDead) return;
        
        // 攻撃イベントを発行
        OnUnitAttacked?.Invoke(this, damage);
        
        // 目標にダメージを与える
        target.TakeDamage(damage);
    }
    
    // 共通のユニット名取得メソッド
    public virtual string GetUnitName()
    {
        return "Unit";
    }
    
    // 攻撃範囲を取得（デフォルトは隣接マス）
    public virtual int GetAttackRange()
    {
        return 1;
    }
    
    // ターンリセット
    public virtual void ResetTurn()
    {
        // 派生クラスでオーバーライド
    }
} 