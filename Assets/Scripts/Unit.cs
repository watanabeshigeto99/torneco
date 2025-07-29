using UnityEngine;

public class Unit : MonoBehaviour
{
    public int maxHP = 10;
    public int currentHP;

    public bool IsDead => currentHP <= 0;

    protected virtual void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);

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

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
    }

    protected virtual void Die()
    {
        // Override in Player or Enemy
    }
} 