using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// イベントバスパターン実装
/// 責務：システム間の疎結合な通信のみ
/// </summary>
public static class EventBus
{
    private static Dictionary<string, Action<object>> events = new Dictionary<string, Action<object>>();
    private static Dictionary<string, List<Action<object>>> eventListeners = new Dictionary<string, List<Action<object>>>();
    
    /// <summary>
    /// イベントを購読
    /// </summary>
    public static void Subscribe(string eventName, Action<object> handler)
    {
        if (!eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName] = new List<Action<object>>();
        }
        eventListeners[eventName].Add(handler);
    }
    
    /// <summary>
    /// イベントの購読を解除
    /// </summary>
    public static void Unsubscribe(string eventName, Action<object> handler)
    {
        if (eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName].Remove(handler);
        }
    }
    
    /// <summary>
    /// イベントを発行
    /// </summary>
    public static void Publish(string eventName, object data = null)
    {
        if (eventListeners.ContainsKey(eventName))
        {
            foreach (var handler in eventListeners[eventName])
            {
                try
                {
                    handler?.Invoke(data);
                }
                catch (Exception e)
                {
#if UNITY_EDITOR
                    Debug.LogError($"EventBus: イベント {eventName} の処理中にエラーが発生しました: {e.Message}");
#endif
                }
            }
        }
    }
    
    /// <summary>
    /// 特定のイベントの購読者数を取得
    /// </summary>
    public static int GetSubscriberCount(string eventName)
    {
        return eventListeners.ContainsKey(eventName) ? eventListeners[eventName].Count : 0;
    }
    
    /// <summary>
    /// 全てのイベントをクリア
    /// </summary>
    public static void ClearAllEvents()
    {
        eventListeners.Clear();
    }
    
    /// <summary>
    /// 特定のイベントをクリア
    /// </summary>
    public static void ClearEvent(string eventName)
    {
        if (eventListeners.ContainsKey(eventName))
        {
            eventListeners[eventName].Clear();
        }
    }
    
    /// <summary>
    /// デバッグ情報を取得
    /// </summary>
    public static string GetDebugInfo()
    {
        var info = new System.Text.StringBuilder();
        info.AppendLine("=== EventBus Debug Info ===");
        
        foreach (var kvp in eventListeners)
        {
            info.AppendLine($"Event: {kvp.Key} - Subscribers: {kvp.Value.Count}");
        }
        
        return info.ToString();
    }
}

/// <summary>
/// イベント名の定数定義
/// </summary>
public static class EventNames
{
    // プレイヤー関連
    public const string PLAYER_MOVED = "PlayerMoved";
    public const string PLAYER_ATTACKED = "PlayerAttacked";
    public const string PLAYER_HEALED = "PlayerHealed";
    public const string PLAYER_DIED = "PlayerDied";
    public const string PLAYER_LEVEL_UP = "PlayerLevelUp";
    
    // 敵関連
    public const string ENEMY_SPAWNED = "EnemySpawned";
    public const string ENEMY_DIED = "EnemyDied";
    public const string ENEMY_MOVED = "EnemyMoved";
    
    // ゲーム状態関連
    public const string GAME_OVER = "GameOver";
    public const string GAME_CLEAR = "GameClear";
    public const string FLOOR_CHANGED = "FloorChanged";
    
    // UI関連
    public const string UI_UPDATED = "UIUpdated";
    public const string CARD_SELECTED = "CardSelected";
    public const string DECK_CHANGED = "DeckChanged";
    
    // システム関連
    public const string SAVE_COMPLETED = "SaveCompleted";
    public const string LOAD_COMPLETED = "LoadCompleted";
    public const string SYSTEM_INITIALIZED = "SystemInitialized";
}
