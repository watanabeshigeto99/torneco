using UnityEngine;

/// <summary>
/// ゲーム設定を管理するScriptableObject
/// 責務：ゲーム全体の設定値の管理のみ
/// </summary>
[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [Header("Player Settings")]
    public int defaultPlayerHP = 20;
    public int defaultPlayerMaxHP = 20;
    public int defaultPlayerLevel = 1;
    public int defaultPlayerExp = 0;
    public int defaultPlayerExpToNext = 10;
    public int defaultPlayerMaxLevel = 10;
    
    [Header("Grid Settings")]
    public int defaultGridWidth = 10;
    public int defaultGridHeight = 10;
    public float defaultTileSpacing = 1.1f;
    public int defaultVisionRange = 8;
    public int defaultNormalVisibilityRange = 1;
    public int defaultTransparentVisibilityRange = 2;
    
    [Header("Enemy Settings")]
    public int defaultEnemyCount = 5;
    public int minEnemyDistanceFromPlayer = 3;
    public int maxEnemyDistanceFromPlayer = 8;
    
    [Header("Deck Settings")]
    public int defaultDeckSize = 10;
    public int minDeckSize = 5;
    public int maxDeckSize = 15;
    
    [Header("Floor Settings")]
    public int defaultMaxFloor = 10;
    public int defaultCurrentFloor = 1;
    
    [Header("UI Settings")]
    public Vector2 defaultCardSize = new Vector2(100, 150);
    public Vector2 largeCardSize = new Vector2(120, 180);
    public Vector2 smallCardSize = new Vector2(80, 120);
    
    [Header("Performance Settings")]
    public bool enableDebugLogs = true;
    public bool enablePerformanceMonitoring = false;
    public float frameRateTarget = 60f;
    
    [Header("Audio Settings")]
    public float defaultBGMVolume = 0.7f;
    public float defaultSEVolume = 0.8f;
    
    /// <summary>
    /// デフォルト設定を取得
    /// </summary>
    public static GameSettings GetDefaultSettings()
    {
        var settings = Resources.Load<GameSettings>("Settings/DefaultGameSettings");
        if (settings == null)
        {
            Debug.LogWarning("GameSettings: デフォルト設定ファイルが見つかりません。新しい設定を作成します。");
            settings = CreateInstance<GameSettings>();
        }
        return settings;
    }
    
    /// <summary>
    /// 設定を検証
    /// </summary>
    public bool ValidateSettings()
    {
        bool isValid = true;
        
        if (defaultPlayerHP <= 0)
        {
            Debug.LogError("GameSettings: defaultPlayerHPは0より大きい値である必要があります");
            isValid = false;
        }
        
        if (defaultGridWidth <= 0 || defaultGridHeight <= 0)
        {
            Debug.LogError("GameSettings: グリッドサイズは0より大きい値である必要があります");
            isValid = false;
        }
        
        if (defaultVisionRange <= 0)
        {
            Debug.LogError("GameSettings: defaultVisionRangeは0より大きい値である必要があります");
            isValid = false;
        }
        
        if (minDeckSize > maxDeckSize)
        {
            Debug.LogError("GameSettings: minDeckSizeはmaxDeckSize以下である必要があります");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 設定の詳細情報を取得
    /// </summary>
    public string GetSettingsInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine("=== Game Settings Info ===");
        info.AppendLine($"Player HP: {defaultPlayerHP}/{defaultPlayerMaxHP}");
        info.AppendLine($"Grid Size: {defaultGridWidth}x{defaultGridHeight}");
        info.AppendLine($"Vision Range: {defaultVisionRange}");
        info.AppendLine($"Deck Size: {minDeckSize}-{maxDeckSize}");
        info.AppendLine($"Max Floor: {defaultMaxFloor}");
        info.AppendLine($"Card Size: {defaultCardSize}");
        info.AppendLine($"Debug Logs: {enableDebugLogs}");
        info.AppendLine($"Performance Monitoring: {enablePerformanceMonitoring}");
        
        return info.ToString();
    }
}
