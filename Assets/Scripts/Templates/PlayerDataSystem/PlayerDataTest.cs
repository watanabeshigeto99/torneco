using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PlayerDataSystem
{
    /// <summary>
    /// プレイヤーデータシステムのテスト用コンポーネント
    /// 責務：プレイヤーデータシステムのテストのみ
    /// </summary>
    public class PlayerDataTest : MonoBehaviour
    {
        [Header("Player Data System Components")]
        public PlayerDataManager playerDataManager;
        public PlayerDataSO playerData;
        public PlayerEventChannel playerEventChannel;
        
        [Header("UI Elements")]
        public Button addExpButton;
        public Button levelUpButton;
        public Button damageButton;
        public Button healButton;
        public Button addGoldButton;
        public Button spendGoldButton;
        public Button resetButton;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI logText;
        public TextMeshProUGUI detailedInfoText;
        
        [Header("Test Values")]
        public int testExpAmount = 10;
        public int testDamageAmount = 5;
        public int testHealAmount = 10;
        public int testGoldAmount = 100;
        
        private string logHistory = "";
        
        private void Start()
        {
            SetupTestUI();
            SubscribeToEvents();
            ValidateTestSetup();
            UpdateStatusDisplay();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// テスト用UIの設定
        /// </summary>
        private void SetupTestUI()
        {
            if (addExpButton != null)
                addExpButton.onClick.AddListener(OnAddExpClicked);
            
            if (levelUpButton != null)
                levelUpButton.onClick.AddListener(OnLevelUpClicked);
            
            if (damageButton != null)
                damageButton.onClick.AddListener(OnDamageClicked);
            
            if (healButton != null)
                healButton.onClick.AddListener(OnHealClicked);
            
            if (addGoldButton != null)
                addGoldButton.onClick.AddListener(OnAddGoldClicked);
            
            if (spendGoldButton != null)
                spendGoldButton.onClick.AddListener(OnSpendGoldClicked);
            
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (playerEventChannel != null)
            {
                playerEventChannel.OnPlayerDataChanged.AddListener(OnPlayerDataChanged);
                playerEventChannel.OnPlayerLevelUp.AddListener(OnPlayerLevelUp);
                playerEventChannel.OnPlayerExpGained.AddListener(OnPlayerExpGained);
                playerEventChannel.OnPlayerHPChanged.AddListener(OnPlayerHPChanged);
                playerEventChannel.OnPlayerDamaged.AddListener(OnPlayerDamaged);
                playerEventChannel.OnPlayerHealed.AddListener(OnPlayerHealed);
                playerEventChannel.OnPlayerGoldChanged.AddListener(OnPlayerGoldChanged);
                playerEventChannel.OnPlayerDied.AddListener(OnPlayerDied);
            }
        }
        
        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (playerEventChannel != null)
            {
                playerEventChannel.OnPlayerDataChanged.RemoveListener(OnPlayerDataChanged);
                playerEventChannel.OnPlayerLevelUp.RemoveListener(OnPlayerLevelUp);
                playerEventChannel.OnPlayerExpGained.RemoveListener(OnPlayerExpGained);
                playerEventChannel.OnPlayerHPChanged.RemoveListener(OnPlayerHPChanged);
                playerEventChannel.OnPlayerDamaged.RemoveListener(OnPlayerDamaged);
                playerEventChannel.OnPlayerHealed.RemoveListener(OnPlayerHealed);
                playerEventChannel.OnPlayerGoldChanged.RemoveListener(OnPlayerGoldChanged);
                playerEventChannel.OnPlayerDied.RemoveListener(OnPlayerDied);
            }
        }
        
        /// <summary>
        /// テスト設定の検証
        /// </summary>
        private void ValidateTestSetup()
        {
            if (playerDataManager == null)
            {
                Debug.LogError("[PlayerDataTest] PlayerDataManager is not assigned!");
                AddLog("ERROR: PlayerDataManager is not assigned");
            }
            
            if (playerData == null)
            {
                Debug.LogError("[PlayerDataTest] PlayerData is not assigned!");
                AddLog("ERROR: PlayerData is not assigned");
            }
            
            if (playerEventChannel == null)
            {
                Debug.LogError("[PlayerDataTest] PlayerEventChannel is not assigned!");
                AddLog("ERROR: PlayerEventChannel is not assigned");
            }
            
            if (statusText == null)
            {
                Debug.LogWarning("[PlayerDataTest] StatusText is not assigned");
            }
            
            if (logText == null)
            {
                Debug.LogWarning("[PlayerDataTest] LogText is not assigned");
            }
        }
        
        // UIボタンクリックハンドラー
        private void OnAddExpClicked()
        {
            AddLog($"Add {testExpAmount} experience clicked");
            if (playerDataManager != null)
            {
                playerDataManager.AddPlayerExp(testExpAmount);
            }
            else
            {
                AddLog("ERROR: PlayerDataManager is null");
            }
        }
        
        private void OnLevelUpClicked()
        {
            AddLog("Level up clicked");
            if (playerData != null)
            {
                playerData.SetLevel(playerData.level + 1);
            }
            else
            {
                AddLog("ERROR: PlayerData is null");
            }
        }
        
        private void OnDamageClicked()
        {
            AddLog($"Take {testDamageAmount} damage clicked");
            if (playerData != null)
            {
                playerData.TakeDamage(testDamageAmount);
            }
            else
            {
                AddLog("ERROR: PlayerData is null");
            }
        }
        
        private void OnHealClicked()
        {
            AddLog($"Heal {testHealAmount} HP clicked");
            if (playerData != null)
            {
                playerData.Heal(testHealAmount);
            }
            else
            {
                AddLog("ERROR: PlayerData is null");
            }
        }
        
        private void OnAddGoldClicked()
        {
            AddLog($"Add {testGoldAmount} gold clicked");
            if (playerData != null)
            {
                playerData.AddGold(testGoldAmount);
            }
            else
            {
                AddLog("ERROR: PlayerData is null");
            }
        }
        
        private void OnSpendGoldClicked()
        {
            AddLog($"Spend {testGoldAmount} gold clicked");
            if (playerData != null)
            {
                bool success = playerData.SpendGold(testGoldAmount);
                AddLog($"Gold spend {(success ? "successful" : "failed")}");
            }
            else
            {
                AddLog("ERROR: PlayerData is null");
            }
        }
        
        private void OnResetClicked()
        {
            AddLog("Reset clicked");
            if (playerData != null)
            {
                playerData.Initialize();
            }
            else
            {
                AddLog("ERROR: PlayerData is null");
            }
        }
        
        // イベントハンドラー
        private void OnPlayerDataChanged(PlayerDataSO data)
        {
            AddLog($"Player data changed: {data?.playerName ?? "Unknown"}");
            UpdateStatusDisplay();
        }
        
        private void OnPlayerLevelUp(int newLevel)
        {
            AddLog($"Player leveled up to level {newLevel}");
            UpdateStatusDisplay();
        }
        
        private void OnPlayerExpGained(int expAmount)
        {
            AddLog($"Player gained {expAmount} experience");
            UpdateStatusDisplay();
        }
        
        private void OnPlayerHPChanged(int currentHP, int maxHP)
        {
            AddLog($"Player HP changed: {currentHP}/{maxHP}");
            UpdateStatusDisplay();
        }
        
        private void OnPlayerDamaged(int damage)
        {
            AddLog($"Player took {damage} damage");
        }
        
        private void OnPlayerHealed(int healAmount)
        {
            AddLog($"Player healed {healAmount} HP");
        }
        
        private void OnPlayerGoldChanged(int newGold)
        {
            AddLog($"Player gold changed to {newGold}");
            UpdateStatusDisplay();
        }
        
        private void OnPlayerDied()
        {
            AddLog("Player died!");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;
            
            if (playerData != null)
            {
                string status = $"Player: {playerData.playerName}\n";
                status += $"Level: {playerData.level}/{playerData.maxLevel}\n";
                status += $"HP: {playerData.currentHP}/{playerData.maxHP} ({playerData.GetHPPercentage():P1})\n";
                status += $"Exp: {playerData.experience}/{playerData.experienceToNext} ({playerData.GetExpPercentage():P1})\n";
                status += $"Attack: {playerData.attack}\n";
                status += $"Defense: {playerData.defense}\n";
                status += $"Gold: {playerData.gold}\n";
                status += $"Alive: {playerData.IsAlive()}\n";
                status += $"Max Level: {playerData.IsMaxLevel()}";
                
                statusText.text = status;
            }
            else
            {
                statusText.text = "PlayerData not assigned";
            }
            
            // 詳細情報の更新
            if (detailedInfoText != null && playerData != null)
            {
                detailedInfoText.text = playerData.GetDetailedInfo();
            }
        }
        
        /// <summary>
        /// ログの追加
        /// </summary>
        private void AddLog(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}\n";
            logHistory += logEntry;
            
            if (logText != null)
            {
                logText.text = logHistory;
            }
            
            Debug.Log($"[PlayerDataTest] {message}");
        }
        
        /// <summary>
        /// テスト設定の情報を取得
        /// </summary>
        public string GetTestInfo()
        {
            return $"PlayerDataTest - Manager: {(playerDataManager != null ? "✓" : "✗")}, " +
                   $"Data: {(playerData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(playerEventChannel != null ? "✓" : "✗")}";
        }
    }
} 