using UnityEngine;

public abstract class Unit : MonoBehaviour
{
    [Header("Stats")]
    public int maxHP = 10;
    public int currentHP;
    
    public bool IsDead => currentHP <= 0;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public virtual void TakeDamage(int amount)
    {
        int oldHP = currentHP;
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        int actualDamage = oldHP - currentHP;
        
        // プレイヤーの場合、GameManagerのデータも更新
        if (this is Player && GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerHP(currentHP, maxHP);
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.AddLog($"ダメージを受けた！HP: {currentHP}/{maxHP}");
        }
        
        if (IsDead)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        // Override in Player or Enemy
    }
} 