using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 戦闘設定を管理するScriptableObject
    /// 責務：戦闘の設定値管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "BattleConfig", menuName = "Battle System/Battle Config")]
    public class BattleConfigSO : ScriptableObject
    {
        [Header("Turn Settings")]
        [Tooltip("プレイヤーの手札枚数")]
        public int playerHandSize = 7;
        
        [Tooltip("ターン制限（0 = 無制限）")]
        public int maxTurns = 0;
        
        [Tooltip("カード選択時間制限（秒）")]
        public float cardSelectionTimeLimit = 30f;
        
        [Header("Battle Settings")]
        [Tooltip("戦闘開始時のプレイヤーHP")]
        public int playerStartingHP = 20;
        
        [Tooltip("戦闘開始時のプレイヤー攻撃力")]
        public int playerStartingAttack = 5;
        
        [Tooltip("戦闘開始時のプレイヤー防御力")]
        public int playerStartingDefense = 2;
        
        [Header("Enemy Settings")]
        [Tooltip("敵の最大数")]
        public int maxEnemyCount = 3;
        
        [Tooltip("敵の基本HP")]
        public int enemyBaseHP = 15;
        
        [Tooltip("敵の基本攻撃力")]
        public int enemyBaseAttack = 4;
        
        [Tooltip("敵の基本防御力")]
        public int enemyBaseDefense = 1;
        
        [Header("Reward Settings")]
        [Tooltip("勝利時の経験値")]
        public int victoryExp = 10;
        
        [Tooltip("勝利時のスコア")]
        public int victoryScore = 100;
        
        [Header("UI Settings")]
        [Tooltip("ダメージ表示の持続時間")]
        public float damageDisplayDuration = 1.5f;
        
        [Tooltip("ヒール表示の持続時間")]
        public float healDisplayDuration = 1.5f;
        
        [Tooltip("カード使用アニメーション時間")]
        public float cardUseAnimationDuration = 0.5f;
        
        [Header("Audio Settings")]
        [Tooltip("攻撃効果音")]
        public AudioClip attackSound;
        
        [Tooltip("ヒール効果音")]
        public AudioClip healSound;
        
        [Tooltip("カード使用効果音")]
        public AudioClip cardUseSound;
        
        [Tooltip("勝利効果音")]
        public AudioClip victorySound;
        
        [Tooltip("敗北効果音")]
        public AudioClip defeatSound;
        
        /// <summary>
        /// 設定の検証
        /// </summary>
        public bool ValidateConfig()
        {
            if (playerHandSize <= 0)
            {
                Debug.LogError("BattleConfigSO: playerHandSizeは1以上である必要があります");
                return false;
            }
            
            if (playerStartingHP <= 0)
            {
                Debug.LogError("BattleConfigSO: playerStartingHPは1以上である必要があります");
                return false;
            }
            
            if (maxEnemyCount <= 0)
            {
                Debug.LogError("BattleConfigSO: maxEnemyCountは1以上である必要があります");
                return false;
            }
            
            if (enemyBaseHP <= 0)
            {
                Debug.LogError("BattleConfigSO: enemyBaseHPは1以上である必要があります");
                return false;
            }
            
            Debug.Log("BattleConfigSO: 設定の検証が完了しました");
            return true;
        }
        
        /// <summary>
        /// プレイヤーの初期ステータスを取得
        /// </summary>
        public PlayerStats GetPlayerStartingStats()
        {
            return new PlayerStats
            {
                maxHP = playerStartingHP,
                currentHP = playerStartingHP,
                attack = playerStartingAttack,
                defense = playerStartingDefense
            };
        }
        
        /// <summary>
        /// 敵の基本ステータスを取得
        /// </summary>
        public EnemyStats GetEnemyBaseStats()
        {
            return new EnemyStats
            {
                maxHP = enemyBaseHP,
                currentHP = enemyBaseHP,
                attack = enemyBaseAttack,
                defense = enemyBaseDefense
            };
        }
        
        /// <summary>
        /// 設定情報の取得
        /// </summary>
        public string GetConfigInfo()
        {
            return $"BattleConfig: 手札{playerHandSize}枚, プレイヤーHP{playerStartingHP}, 敵最大{maxEnemyCount}体";
        }
    }
    
    /// <summary>
    /// プレイヤーステータス構造体
    /// </summary>
    [System.Serializable]
    public struct PlayerStats
    {
        public int maxHP;
        public int currentHP;
        public int attack;
        public int defense;
    }
    
    /// <summary>
    /// 敵ステータス構造体
    /// </summary>
    [System.Serializable]
    public struct EnemyStats
    {
        public int maxHP;
        public int currentHP;
        public int attack;
        public int defense;
    }
} 