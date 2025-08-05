using UnityEngine;
using System;

namespace PlayerDataSystem
{
    /// <summary>
    /// プレイヤーデータ管理専用コンポーネント
    /// 責務：プレイヤーデータの管理のみ
    /// </summary>
    [DefaultExecutionOrder(-150)]
    public class PlayerDataManager : MonoBehaviour
    {
        public static PlayerDataManager Instance { get; private set; }
        
        [Header("Player Data Settings")]
        public PlayerDataSO playerData;
        public PlayerEventChannel playerEventChannel;
        
        // プレイヤーデータ変更イベント
        public static event Action<PlayerDataSO> OnPlayerDataChanged;
        public static event Action<int> OnPlayerLevelUp;
        public static event Action<int> OnPlayerExpGained;
        public static event Action<int, int> OnPlayerHPChanged;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializePlayerData();
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        /// <summary>
        /// プレイヤーデータの初期化
        /// </summary>
        private void InitializePlayerData()
        {
            if (playerData == null)
            {
                Debug.LogError("PlayerDataManager: playerDataが設定されていません");
                return;
            }
            
            playerData.Initialize();
            Debug.Log("PlayerDataManager: プレイヤーデータを初期化しました");
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (playerEventChannel != null)
            {
                playerEventChannel.OnPlayerDataChanged.AddListener(OnPlayerDataChangedHandler);
                playerEventChannel.OnPlayerLevelUp.AddListener(OnPlayerLevelUpHandler);
                playerEventChannel.OnPlayerExpGained.AddListener(OnPlayerExpGainedHandler);
                playerEventChannel.OnPlayerHPChanged.AddListener(OnPlayerHPChangedHandler);
            }
        }
        
        /// <summary>
        /// プレイヤーデータ変更ハンドラー
        /// </summary>
        private void OnPlayerDataChangedHandler(PlayerDataSO data)
        {
            OnPlayerDataChanged?.Invoke(data);
            Debug.Log("PlayerDataManager: プレイヤーデータが変更されました");
        }
        
        /// <summary>
        /// プレイヤーレベルアップハンドラー
        /// </summary>
        private void OnPlayerLevelUpHandler(int newLevel)
        {
            OnPlayerLevelUp?.Invoke(newLevel);
            Debug.Log($"PlayerDataManager: プレイヤーがレベルアップしました - レベル{newLevel}");
        }
        
        /// <summary>
        /// プレイヤー経験値獲得ハンドラー
        /// </summary>
        private void OnPlayerExpGainedHandler(int expAmount)
        {
            OnPlayerExpGained?.Invoke(expAmount);
            Debug.Log($"PlayerDataManager: プレイヤーが経験値を獲得しました - +{expAmount}");
        }
        
        /// <summary>
        /// プレイヤーHP変更ハンドラー
        /// </summary>
        private void OnPlayerHPChangedHandler(int currentHP, int maxHP)
        {
            OnPlayerHPChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"PlayerDataManager: プレイヤーHPが変更されました - {currentHP}/{maxHP}");
        }
        
        /// <summary>
        /// 経験値の追加
        /// </summary>
        public void AddPlayerExp(int amount)
        {
            if (playerData == null) return;
            
            playerData.AddExperience(amount);
            
            if (playerEventChannel != null)
            {
                playerEventChannel.RaisePlayerExpGained(amount);
            }
        }
        
        /// <summary>
        /// プレイヤーHPの設定
        /// </summary>
        public void SetPlayerHP(int currentHP, int maxHP = -1)
        {
            if (playerData == null) return;
            
            playerData.SetHP(currentHP, maxHP);
            
            if (playerEventChannel != null)
            {
                playerEventChannel.RaisePlayerHPChanged(currentHP, maxHP > 0 ? maxHP : playerData.maxHP);
            }
        }
        
        /// <summary>
        /// プレイヤーレベルの設定
        /// </summary>
        public void SetPlayerLevel(int level)
        {
            if (playerData == null) return;
            
            playerData.SetLevel(level);
            
            if (playerEventChannel != null)
            {
                playerEventChannel.RaisePlayerLevelUp(level);
            }
        }
        
        /// <summary>
        /// プレイヤーデータの取得
        /// </summary>
        public PlayerDataSO GetPlayerData()
        {
            return playerData;
        }
        
        /// <summary>
        /// プレイヤーデータの同期（GameManager統合用）
        /// </summary>
        public void SyncWithGameManager()
        {
            if (GameManager.Instance != null && playerData != null)
            {
                // GameManagerからデータを取得
                playerData.SetLevel(GameManager.Instance.playerLevel);
                playerData.SetExperience(GameManager.Instance.playerExp);
                playerData.SetHP(GameManager.Instance.playerCurrentHP, GameManager.Instance.playerMaxHP);
                
                Debug.Log("PlayerDataManager: GameManagerとデータを同期しました");
            }
        }
        
        /// <summary>
        /// GameManagerにデータを適用（GameManager統合用）
        /// </summary>
        public void ApplyToGameManager()
        {
            if (GameManager.Instance != null && playerData != null)
            {
                // GameManagerにデータを適用
                GameManager.Instance.playerLevel = playerData.level;
                GameManager.Instance.playerExp = playerData.experience;
                GameManager.Instance.playerCurrentHP = playerData.currentHP;
                GameManager.Instance.playerMaxHP = playerData.maxHP;
                
                Debug.Log("PlayerDataManager: GameManagerにデータを適用しました");
            }
        }
        
        /// <summary>
        /// プレイヤーデータ管理システムの情報を取得
        /// </summary>
        public string GetPlayerDataSystemInfo()
        {
            return $"PlayerDataManager - Data: {(playerData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(playerEventChannel != null ? "✓" : "✗")}";
        }
    }
} 