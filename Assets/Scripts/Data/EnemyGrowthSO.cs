using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemyGrowth", menuName = "Card Battle/Enemy Growth Data")]
public class EnemyGrowthSO : ScriptableObject
{
    [Header("Growth Curves")]
    [Tooltip("階層に応じたHP増加曲線")]
    public AnimationCurve hpGrowthCurve = AnimationCurve.Linear(1, 0, 50, 50);
    
    [Tooltip("階層に応じた攻撃力増加曲線")]
    public AnimationCurve attackGrowthCurve = AnimationCurve.Linear(1, 0, 50, 10);
    
    [Header("Base Stats")]
    [Tooltip("基本HP")]
    public int baseHP = 5;
    
    [Tooltip("基本攻撃力")]
    public int baseAttackPower = 1;
    
    [Header("Growth Settings")]
    [Tooltip("最大階層")]
    public int maxFloor = 50;
    
    [Tooltip("成長倍率")]
    public float growthMultiplier = 1.0f;
    
    /// <summary>
    /// 指定階層でのHPを取得
    /// </summary>
    /// <param name="floor">階層</param>
    /// <returns>強化されたHP</returns>
    public int GetHPForFloor(int floor)
    {
        float curveValue = hpGrowthCurve.Evaluate(Mathf.Clamp(floor, 1, maxFloor));
        return Mathf.RoundToInt(baseHP + (curveValue * growthMultiplier));
    }
    
    /// <summary>
    /// 指定階層での攻撃力を取得
    /// </summary>
    /// <param name="floor">階層</param>
    /// <returns>強化された攻撃力</returns>
    public int GetAttackPowerForFloor(int floor)
    {
        float curveValue = attackGrowthCurve.Evaluate(Mathf.Clamp(floor, 1, maxFloor));
        return Mathf.RoundToInt(baseAttackPower + (curveValue * growthMultiplier));
    }
    
    /// <summary>
    /// 階層に応じた敵の強化データを取得
    /// </summary>
    /// <param name="floor">階層</param>
    /// <returns>強化データ</returns>
    public EnemyGrowthData GetGrowthDataForFloor(int floor)
    {
        return new EnemyGrowthData
        {
            hp = GetHPForFloor(floor),
            attackPower = GetAttackPowerForFloor(floor),
            floor = floor
        };
    }
}

/// <summary>
/// 敵の成長データ
/// </summary>
[System.Serializable]
public struct EnemyGrowthData
{
    public int hp;
    public int attackPower;
    public int floor;
    
    public override string ToString()
    {
        return $"Floor {floor}: HP={hp}, Attack={attackPower}";
    }
} 