using UnityEngine;
using System;

/// <summary>
/// プレイヤーデータ管理専用コンポーネント
/// 責務：プレイヤーのレベル、経験値、HPの管理のみ
/// </summary>
[DefaultExecutionOrder(-90)]
public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager Instance { get; private set; }
    
    [Header("Player Data")]
    public int playerLevel = 1;
    public int playerExp = 0;
    public int playerExpToNext = 10;
    public int playerMaxHP = 20;
    public int playerCurrentHP = 20;
    public int playerMaxLevel = 10;
    
    // プレイヤーデータ変更イベント
    public static event Action<int> OnPlayerLevelUp;
    public static event Action<int> OnPlayerExpGained;
    public static event Action<int, int> OnPlayerHPChanged;
    public static event Action OnPlayerDataChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializePlayerData();
    }
    
    /// <summary>
    /// プレイヤーデータの初期化
    /// </summary>
    private void InitializePlayerData()
    {
        playerLevel = 1;
        playerExp = 0;
        playerExpToNext = 10;
        playerMaxHP = 20;
        playerCurrentHP = 20;
        playerMaxLevel = 10;
        
        Debug.Log("PlayerDataManager: プレイヤーデータを初期化しました");
    }
    
    /// <summary>
    /// プレイヤーのHPを設定
    /// </summary>
    /// <param name="currentHP">現在のHP</param>
    /// <param name="maxHP">最大HP（-1の場合は変更しない）</param>
    public void SetPlayerHP(int currentHP, int maxHP = -1)
    {
        bool changed = false;
        
        if (currentHP != playerCurrentHP)
        {
            playerCurrentHP = Mathf.Clamp(currentHP, 0, playerMaxHP);
            changed = true;
        }
        
        if (maxHP > 0 && maxHP != playerMaxHP)
        {
            playerMaxHP = maxHP;
            playerCurrentHP = Mathf.Clamp(playerCurrentHP, 0, playerMaxHP);
            changed = true;
        }
        
        if (changed)
        {
            OnPlayerHPChanged?.Invoke(playerCurrentHP, playerMaxHP);
            OnPlayerDataChanged?.Invoke();
            
            Debug.Log($"PlayerDataManager: プレイヤーHPを設定しました - {playerCurrentHP}/{playerMaxHP}");
        }
    }
    
    /// <summary>
    /// プレイヤーのレベルを設定
    /// </summary>
    /// <param name="level">新しいレベル</param>
    public void SetPlayerLevel(int level)
    {
        if (level != playerLevel && level >= 1 && level <= playerMaxLevel)
        {
            int oldLevel = playerLevel;
            playerLevel = level;
            
            OnPlayerLevelUp?.Invoke(playerLevel);
            OnPlayerDataChanged?.Invoke();
            
            Debug.Log($"PlayerDataManager: プレイヤーレベルを設定しました - {oldLevel} → {playerLevel}");
        }
    }
    
    /// <summary>
    /// プレイヤーに経験値を加算
    /// </summary>
    /// <param name="amount">加算する経験値</param>
    public void AddPlayerExp(int amount)
    {
        if (amount <= 0 || playerLevel >= playerMaxLevel) return;
        
        playerExp += amount;
        OnPlayerExpGained?.Invoke(amount);
        OnPlayerDataChanged?.Invoke();
        
        Debug.Log($"PlayerDataManager: プレイヤーに経験値を加算しました - +{amount} (合計: {playerExp})");
        
        // レベルアップチェック
        CheckLevelUp();
    }
    
    /// <summary>
    /// レベルアップチェック
    /// </summary>
    private void CheckLevelUp()
    {
        while (playerExp >= playerExpToNext && playerLevel < playerMaxLevel)
        {
            playerExp -= playerExpToNext;
            playerLevel++;
            
            // レベルアップ時の成長
            int oldMaxHP = playerMaxHP;
            playerMaxHP += 5;
            playerCurrentHP = playerMaxHP; // レベルアップ時はHP全回復
            playerExpToNext = Mathf.RoundToInt(playerExpToNext * 1.5f);
            
            OnPlayerLevelUp?.Invoke(playerLevel);
            OnPlayerHPChanged?.Invoke(playerCurrentHP, playerMaxHP);
            OnPlayerDataChanged?.Invoke();
            
            Debug.Log($"PlayerDataManager: プレイヤーがレベルアップしました - レベル{playerLevel}, HP {oldMaxHP} → {playerMaxHP}");
        }
    }
    
    /// <summary>
    /// プレイヤーデータをリセット
    /// </summary>
    public void ResetPlayerData()
    {
        InitializePlayerData();
        OnPlayerDataChanged?.Invoke();
        
        Debug.Log("PlayerDataManager: プレイヤーデータをリセットしました");
    }
    
    /// <summary>
    /// プレイヤーデータの情報を取得
    /// </summary>
    /// <returns>プレイヤーデータの情報文字列</returns>
    public string GetPlayerDataInfo()
    {
        return $"PlayerData - Level: {playerLevel}, Exp: {playerExp}/{playerExpToNext}, HP: {playerCurrentHP}/{playerMaxHP}";
    }
} 