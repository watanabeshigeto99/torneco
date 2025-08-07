using UnityEngine;

/// <summary>
/// 初期化順序の定数定義
/// 責務：システム間の初期化順序の管理のみ
/// </summary>
public static class InitializationOrder
{
    // システム基盤（最優先）
    public const int EVENT_BUS = -100;
    public const int GAME_MANAGER = -100;
    public const int GAME_SETTINGS = -95;
    
    // データ管理
    public const int PLAYER_DATA_MANAGER = -90;
    public const int SAVE_MANAGER = -90;
    public const int DECK_MANAGER = -90;
    
    // ゲーム世界管理
    public const int GRID_MANAGER = -90;
    public const int VISION_MANAGER = -85;
    public const int FLOOR_MANAGER = -85;
    
    // ゲームロジック
    public const int TURN_MANAGER = -80;
    public const int ENEMY_MANAGER = -80;
    public const int CARD_MANAGER = -80;
    
    // プレイヤー・敵
    public const int PLAYER = -30;
    public const int ENEMY = -30;
    
    // UI・サウンド
    public const int SOUND_MANAGER = -20;
    public const int UI_MANAGER = -10;
    public const int TRANSITION_MANAGER = -10;
    
    // カメラ・エフェクト
    public const int CAMERA_FOLLOW = 0;
    public const int PARTICLE_SYSTEM = 10;
    
    // 自動セットアップ（最後）
    public const int AUTO_SETUP_MANAGER = 50;
    
    /// <summary>
    /// 初期化順序を検証
    /// </summary>
    public static bool ValidateInitializationOrder()
    {
        bool isValid = true;
        
        // 基盤システムが最初に初期化されることを確認
        if (EVENT_BUS >= GAME_MANAGER)
        {
            Debug.LogError("InitializationOrder: EVENT_BUSはGAME_MANAGERより先に初期化される必要があります");
            isValid = false;
        }
        
        // データ管理がゲーム世界管理より先に初期化されることを確認
        if (PLAYER_DATA_MANAGER >= GRID_MANAGER)
        {
            Debug.LogError("InitializationOrder: PLAYER_DATA_MANAGERはGRID_MANAGERより先に初期化される必要があります");
            isValid = false;
        }
        
        // ゲーム世界管理がゲームロジックより先に初期化されることを確認
        if (GRID_MANAGER >= TURN_MANAGER)
        {
            Debug.LogError("InitializationOrder: GRID_MANAGERはTURN_MANAGERより先に初期化される必要があります");
            isValid = false;
        }
        
        // ゲームロジックがプレイヤー・敵より先に初期化されることを確認
        if (TURN_MANAGER >= PLAYER)
        {
            Debug.LogError("InitializationOrder: TURN_MANAGERはPLAYERより先に初期化される必要があります");
            isValid = false;
        }
        
        return isValid;
    }
    
    /// <summary>
    /// 初期化順序の詳細情報を取得
    /// </summary>
    public static string GetInitializationOrderInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine("=== Initialization Order Info ===");
        info.AppendLine($"Event Bus: {EVENT_BUS}");
        info.AppendLine($"Game Manager: {GAME_MANAGER}");
        info.AppendLine($"Game Settings: {GAME_SETTINGS}");
        info.AppendLine($"Player Data Manager: {PLAYER_DATA_MANAGER}");
        info.AppendLine($"Grid Manager: {GRID_MANAGER}");
        info.AppendLine($"Vision Manager: {VISION_MANAGER}");
        info.AppendLine($"Turn Manager: {TURN_MANAGER}");
        info.AppendLine($"Player: {PLAYER}");
        info.AppendLine($"Sound Manager: {SOUND_MANAGER}");
        info.AppendLine($"UI Manager: {UI_MANAGER}");
        info.AppendLine($"Auto Setup Manager: {AUTO_SETUP_MANAGER}");
        
        return info.ToString();
    }
}
