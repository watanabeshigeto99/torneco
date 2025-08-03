using UnityEngine;
using System.Collections;

public class CardExecutor : MonoBehaviour
{
    public static CardExecutor Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    
    /// <summary>
    /// カード効果を実行する
    /// </summary>
    /// <param name="card">実行するカード</param>
    /// <param name="target">対象ユニット（プレイヤー）</param>
    public void ExecuteCardEffect(CardDataSO card, Unit target)
    {
        if (card == null || target == null) return;
        
        Debug.Log($"CardExecutor: {card.cardName}の効果を実行");
        
        switch (card.type)
        {
            case CardType.Move:
                ExecuteMoveCard(card, target);
                break;
            case CardType.Attack:
                ExecuteAttackCard(card, target);
                break;
            case CardType.Heal:
                ExecuteHealCard(card, target);
                break;
            case CardType.Special:
                ExecuteSpecialCard(card, target);
                break;
            default:
                Debug.LogWarning($"CardExecutor: 未対応のカードタイプ {card.type}");
                break;
        }
    }
    
    private void ExecuteMoveCard(CardDataSO card, Unit target)
    {
        int moveDistance = card.GetEffectiveMoveDistance();
        Debug.Log($"CardExecutor: 移動カード実行 - 距離: {moveDistance}");
        
        // 移動選択モードを開始
        if (target is Player player)
        {
            player.StartMoveSelection(moveDistance);
        }
    }
    
    private void ExecuteAttackCard(CardDataSO card, Unit target)
    {
        int attackPower = card.GetEffectivePower();
        Debug.Log($"CardExecutor: 攻撃カード実行 - 攻撃力: {attackPower}");
        
        // 攻撃選択モードを開始
        if (target is Player player)
        {
            player.StartAttackSelection(attackPower);
        }
    }
    
    private void ExecuteHealCard(CardDataSO card, Unit target)
    {
        int healAmount = card.GetEffectiveHealAmount();
        Debug.Log($"CardExecutor: 回復カード実行 - 回復量: {healAmount}");
        
        // 即座に回復を実行
        target.Heal(healAmount);
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Heal");
        }
    }
    
    private void ExecuteSpecialCard(CardDataSO card, Unit target)
    {
        Debug.Log($"CardExecutor: 特殊カード実行 - {card.cardName}");
        
        // 特殊効果の実装
        // 例：状態異常付与、範囲攻撃、など
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Special");
        }
    }
    
    /// <summary>
    /// カード効果の実行を遅延させる（アニメーション用）
    /// </summary>
    public IEnumerator ExecuteCardEffectDelayed(CardDataSO card, Unit target, float delay = 0.5f)
    {
        yield return new WaitForSeconds(delay);
        ExecuteCardEffect(card, target);
    }
} 