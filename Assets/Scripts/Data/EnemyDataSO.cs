using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Card Battle/Enemy Data")]
public class EnemyDataSO : ScriptableObject
{
    [Header("Basic Info")]
    public string enemyName;
    public Sprite sprite;
    public Color spriteColor = Color.white;
    
    [Header("Stats")]
    public int maxHP = 10;
    public int attackPower = 1;
    public int moveRange = 1;
    
    [Header("Behavior")]
    public EnemyType enemyType = EnemyType.Basic;
    public MovementPattern movementPattern = MovementPattern.Random;
    public AttackPattern attackPattern = AttackPattern.Melee;
    
    [Header("Visual")]
    public int sortingOrder = 10;
    public Vector2 colliderSize = new Vector2(1f, 1f);
    
    [Header("Special Abilities")]
    public bool canMove = true;
    public bool canAttack = true;
    public bool isRanged = false;
    [Tooltip("遠距離攻撃の範囲（マス数）")]
    public int rangedAttackRange = 3;
}

// 敵の種類
public enum EnemyType
{
    Basic,      // 基本敵
    Fast,       // 高速移動
    Tank,       // 高HP低攻撃
    Ranged,     // 遠距離攻撃
    Boss        // ボス敵
}

// 移動パターン
public enum MovementPattern
{
    Random,     // ランダム移動
    Chase,      // プレイヤーを追跡
    Patrol,     // 決まったパターンで移動
    Stationary  // 移動しない
}

// 攻撃パターン
public enum AttackPattern
{
    Melee,      // 近接攻撃
    Ranged,     // 遠距離攻撃
    Area,       // 範囲攻撃
    Special     // 特殊攻撃
} 