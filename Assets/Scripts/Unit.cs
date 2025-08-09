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
    
    // キャッシュされたコンポーネント
    private DamageVFX _damageVFX;
    
    protected virtual void Awake()
    {
        currentHP = maxHP;
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        
        // DamageVFXコンポーネントをキャッシュ
        _damageVFX = GetComponent<DamageVFX>();
    }
    
    public virtual void Initialize(Vector2Int startPos)
    {
        gridPosition = startPos;
        transform.position = GridManager.Instance.GetWorldPosition(startPos);
    }
    
    public virtual void TakeDamage(int damage, Unit source = null)
    {
        if (IsDead) return;
        
        currentHP = Mathf.Max(0, currentHP - damage);
        
        // ダメージイベントを発行
        OnUnitDamaged?.Invoke(this, damage);
        
        // DamageVFXの再生（キャッシュされたコンポーネントを使用）
        Vector2 hitDirection = Vector2.zero;
        if (source != null)
        {
            hitDirection = (transform.position - source.transform.position).normalized;
        }
        
        if (_damageVFX != null)
        {
            _damageVFX.Play(hitDirection);
        }
        
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
        
        // デフォルトではGameObjectを破棄
        Destroy(gameObject);
    }
    
    public virtual void MoveTo(Vector2Int pos)
    {
        Vector2Int oldPos = gridPosition;
        gridPosition = pos;
        transform.position = GridManager.Instance.GetWorldPosition(pos);
        
        // 移動イベントを発行
        OnUnitMoved?.Invoke(this, oldPos);
    }
    
    public virtual void Move(Vector2Int delta)
    {
        MoveTo(gridPosition + delta);
    }
    
    public virtual Vector2Int GetPosition()
    {
        return gridPosition;
    }
    
    public virtual void SetPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        transform.position = GridManager.Instance.GetWorldPosition(newPos);
    }
    
    public virtual void Attack(Unit target, int damage)
    {
        if (target == null || target.IsDead) return;
        
        // 攻撃イベントを発行
        OnUnitAttacked?.Invoke(this, damage);
        
        // ターゲットにダメージを与える
        target.TakeDamage(damage, this);
    }
    
    public virtual string GetUnitName()
    {
        return gameObject.name;
    }
    
    public virtual int GetAttackRange()
    {
        return 1; // デフォルトは1マス
    }
    
    public virtual void ResetTurn()
    {
        // ターンリセット時の処理（必要に応じてオーバーライド）
    }
} 