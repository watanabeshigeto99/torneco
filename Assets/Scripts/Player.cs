using UnityEngine;

public class Player : Unit
{
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // 初期位置を設定
        gridPosition = new Vector2Int(2, 2); // 5x5グリッドの中央
    }

    public void Move(Vector2Int delta)
    {
        Vector2Int newPos = gridPosition + delta;
        
        if (GridManager.Instance.IsWalkable(newPos))
        {
            gridPosition = newPos;
            Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
            transform.position = worldPos;
        }
    }

    public void SetPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
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
                Heal(card.power);
                break;
            case CardType.Move:
                Move(Vector2Int.down); // 下に移動
                break;
        }
    }

    public void Attack(int damage)
    {
        Debug.Log($"攻撃！ダメージ: {damage}");
        // 前方の敵を攻撃（後で実装）
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Attack");
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        Debug.Log($"回復！HP: {currentHP}/{maxHP}");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.AddLog($"回復！HP: {currentHP}/{maxHP}");
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