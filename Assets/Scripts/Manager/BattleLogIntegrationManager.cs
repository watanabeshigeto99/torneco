using UnityEngine;
using System;

/// <summary>
/// バトルログシステムと既存ゲームシステムの統合管理
/// 各システムのイベントをバトルログに連携
/// </summary>
[DefaultExecutionOrder(-25)]
public class BattleLogIntegrationManager : MonoBehaviour
{
    public static BattleLogIntegrationManager Instance { get; private set; }
    
    [Header("Integration Settings")]
    public bool enableTurnLogging = true;
    public bool enableCardLogging = true;
    public bool enablePlayerLogging = true;
    public bool enableEnemyLogging = true;
    public bool enableDamageLogging = true;
    public bool enableStatusLogging = true;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    private void Start()
    {
        SubscribeToGameEvents();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromGameEvents();
    }
    
    /// <summary>
    /// ゲームイベントの購読
    /// </summary>
    private void SubscribeToGameEvents()
    {
        // ターン管理イベント
        if (TurnManager.Instance != null)
        {
            // TurnManagerのイベントを購読（実装時に追加）
            Debug.Log("BattleLogIntegrationManager: TurnManagerイベントを購読しました");
        }
        
        // カードシステムイベント
        if (CardManager.Instance != null)
        {
            // CardManagerのイベントを購読（実装時に追加）
            Debug.Log("BattleLogIntegrationManager: CardManagerイベントを購読しました");
        }
        
        // プレイヤーイベント
        if (Player.Instance != null)
        {
            // Playerのイベントを購読（実装時に追加）
            Debug.Log("BattleLogIntegrationManager: Playerイベントを購読しました");
        }
        
        // 敵イベント
        if (EnemyManager.Instance != null)
        {
            // EnemyManagerのイベントを購読（実装時に追加）
            Debug.Log("BattleLogIntegrationManager: EnemyManagerイベントを購読しました");
        }
        
        // バトルログイベント
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.OnLogEntryAdded += OnLogEntryAdded;
            BattleLogManager.OnTurnStarted += OnTurnStarted;
            BattleLogManager.OnTurnEnded += OnTurnEnded;
        }
    }
    
    /// <summary>
    /// ゲームイベントの購読解除
    /// </summary>
    private void UnsubscribeFromGameEvents()
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.OnLogEntryAdded -= OnLogEntryAdded;
            BattleLogManager.OnTurnStarted -= OnTurnStarted;
            BattleLogManager.OnTurnEnded -= OnTurnEnded;
        }
    }
    
    #region バトルログイベントハンドラー
    
    /// <summary>
    /// ログエントリ追加ハンドラー
    /// </summary>
    private void OnLogEntryAdded(BattleLogManager.BattleLogEntry entry)
    {
        Debug.Log($"BattleLogIntegrationManager: ログエントリが追加されました - {entry.message}");
    }
    
    /// <summary>
    /// ターン開始ハンドラー
    /// </summary>
    private void OnTurnStarted(int turn)
    {
        Debug.Log($"BattleLogIntegrationManager: ターン {turn} が開始されました");
    }
    
    /// <summary>
    /// ターン終了ハンドラー
    /// </summary>
    private void OnTurnEnded(int turn)
    {
        Debug.Log($"BattleLogIntegrationManager: ターン {turn} が終了しました");
    }
    
    #endregion
    
    #region ターン管理統合
    
    /// <summary>
    /// ターン開始ログ
    /// </summary>
    public void LogTurnStart(int turn, int handSize, int deckSize, int discardSize)
    {
        if (!enableTurnLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogTurnHeader(turn, handSize, deckSize, discardSize);
    }
    
    /// <summary>
    /// ターン終了ログ
    /// </summary>
    public void LogTurnEnd(int cardsDrawn, int statusEffectsProcessed, bool energyReset)
    {
        if (!enableTurnLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogTurnSummary(cardsDrawn, statusEffectsProcessed, energyReset);
    }
    
    #endregion
    
    #region カードシステム統合
    
    /// <summary>
    /// カード使用ログ
    /// </summary>
    public void LogCardUsed(string cardName, string target, BattleLogManager.LogSource source)
    {
        if (!enableCardLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogActionSelection(cardName, target, source);
    }
    
    /// <summary>
    /// カード移動ログ
    /// </summary>
    public void LogCardMoved(string cardName, string fromLocation, string toLocation, BattleLogManager.LogSource source)
    {
        if (!enableCardLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogCardMovement(cardName, fromLocation, toLocation, source);
    }
    
    /// <summary>
    /// ドロー結果ログ
    /// </summary>
    public void LogDrawResult(string[] drawnCards, bool isSearch, BattleLogManager.LogSource source)
    {
        if (!enableCardLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogDrawResult(drawnCards, isSearch, source);
    }
    
    /// <summary>
    /// デッキ変更ログ
    /// </summary>
    public void LogDeckChanged(int oldSize, int newSize, string reason, BattleLogManager.LogSource source)
    {
        if (!enableCardLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogDeckChange(oldSize, newSize, reason, source);
    }
    
    #endregion
    
    #region プレイヤー統合
    
    /// <summary>
    /// プレイヤー移動ログ
    /// </summary>
    public void LogPlayerMoved(string direction, int distance)
    {
        if (!enablePlayerLogging || BattleLogManager.Instance == null) return;
        
        string message = $"プレイヤー移動: {direction} ({distance}マス)";
        BattleLogManager.Instance.AddLogEntry(
            GetCurrentTurn(),
            message,
            BattleLogManager.LogSeverity.INFO,
            BattleLogManager.LogTags.MOVE,
            BattleLogManager.LogSource.PLAYER
        );
    }
    
    /// <summary>
    /// プレイヤー攻撃ログ
    /// </summary>
    public void LogPlayerAttack(string target, int damage, bool isCritical)
    {
        if (!enablePlayerLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogDamageResult(damage, isCritical, 0, BattleLogManager.LogSource.PLAYER);
    }
    
    /// <summary>
    /// プレイヤーHP変更ログ
    /// </summary>
    public void LogPlayerHPChanged(int oldHP, int newHP, int maxHP)
    {
        if (!enablePlayerLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogHPChange("プレイヤー", oldHP, newHP, maxHP, BattleLogManager.LogSource.PLAYER);
    }
    
    /// <summary>
    /// プレイヤー経験値獲得ログ
    /// </summary>
    public void LogPlayerExpGained(int amount, int currentExp, int expToNext)
    {
        if (!enablePlayerLogging || BattleLogManager.Instance == null) return;
        
        string message = $"経験値獲得: +{amount} (現在: {currentExp}/{expToNext})";
        BattleLogManager.Instance.AddLogEntry(
            GetCurrentTurn(),
            message,
            BattleLogManager.LogSeverity.INFO,
            BattleLogManager.LogTags.SYSTEM,
            BattleLogManager.LogSource.PLAYER
        );
    }
    
    /// <summary>
    /// プレイヤーレベルアップログ
    /// </summary>
    public void LogPlayerLevelUp(int newLevel, int newMaxHP)
    {
        if (!enablePlayerLogging || BattleLogManager.Instance == null) return;
        
        string message = $"レベルアップ: レベル {newLevel}, HP {newMaxHP}";
        BattleLogManager.Instance.AddLogEntry(
            GetCurrentTurn(),
            message,
            BattleLogManager.LogSeverity.IMPORTANT,
            BattleLogManager.LogTags.SYSTEM,
            BattleLogManager.LogSource.PLAYER
        );
    }
    
    #endregion
    
    #region 敵統合
    
    /// <summary>
    /// 敵攻撃ログ
    /// </summary>
    public void LogEnemyAttack(string enemyName, string target, int damage, bool isCritical)
    {
        if (!enableEnemyLogging || BattleLogManager.Instance == null) return;
        
        string message = $"{enemyName}が{target}を攻撃";
        BattleLogManager.Instance.AddLogEntry(
            GetCurrentTurn(),
            message,
            BattleLogManager.LogSeverity.INFO,
            BattleLogManager.LogTags.ATK,
            BattleLogManager.LogSource.ENEMY
        );
        
        BattleLogManager.Instance.LogDamageResult(damage, isCritical, 0, BattleLogManager.LogSource.ENEMY);
    }
    
    /// <summary>
    /// 敵HP変更ログ
    /// </summary>
    public void LogEnemyHPChanged(string enemyName, int oldHP, int newHP, int maxHP)
    {
        if (!enableEnemyLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogHPChange(enemyName, oldHP, newHP, maxHP, BattleLogManager.LogSource.ENEMY);
    }
    
    /// <summary>
    /// 敵撃破ログ
    /// </summary>
    public void LogEnemyDefeated(string enemyName)
    {
        if (!enableEnemyLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogDeath(enemyName, false, BattleLogManager.LogSource.ENEMY);
    }
    
    /// <summary>
    /// 敵AI意図ログ
    /// </summary>
    public void LogEnemyIntent(string enemyName, string intent)
    {
        if (!enableEnemyLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogEnemyIntent(enemyName, intent, BattleLogManager.LogSource.ENEMY);
    }
    
    #endregion
    
    #region ダメージ統合
    
    /// <summary>
    /// ダメージ計算ログ
    /// </summary>
    public void LogDamageCalculation(string calculation, int result, BattleLogManager.LogSource source)
    {
        if (!enableDamageLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogCombatCalculation(calculation, result, source);
    }
    
    /// <summary>
    /// ダメージ軽減ログ
    /// </summary>
    public void LogDamageReduction(string target, int originalDamage, int reducedDamage, string reason)
    {
        if (!enableDamageLogging || BattleLogManager.Instance == null) return;
        
        string message = $"{target}のダメージ軽減: {originalDamage} → {reducedDamage} ({reason})";
        BattleLogManager.Instance.AddLogEntry(
            GetCurrentTurn(),
            message,
            BattleLogManager.LogSeverity.INFO,
            BattleLogManager.LogTags.DEF,
            BattleLogManager.LogSource.SYSTEM
        );
    }
    
    #endregion
    
    #region 状態異常統合
    
    /// <summary>
    /// 状態異常付与ログ
    /// </summary>
    public void LogStatusApplied(string target, string statusName, int duration, int effectValue, BattleLogManager.LogSource source)
    {
        if (!enableStatusLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogStatusApplied(target, statusName, duration, effectValue, source);
    }
    
    /// <summary>
    /// 状態異常解除ログ
    /// </summary>
    public void LogStatusRemoved(string target, string statusName, BattleLogManager.LogSource source)
    {
        if (!enableStatusLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogStatusChange(statusName, false, 0, source);
    }
    
    /// <summary>
    /// 状態異常失効ログ
    /// </summary>
    public void LogStatusExpired(string target, string statusName, BattleLogManager.LogSource source)
    {
        if (!enableStatusLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogStatusExpired(target, statusName, source);
    }
    
    /// <summary>
    /// スタック変更ログ
    /// </summary>
    public void LogStackChanged(string statusName, int oldStacks, int newStacks, BattleLogManager.LogSource source)
    {
        if (!enableStatusLogging || BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogStackChange(statusName, oldStacks, newStacks, source);
    }
    
    #endregion
    
    #region 階層・進行統合
    
    /// <summary>
    /// 階層遷移ログ
    /// </summary>
    public void LogFloorTransition(int oldFloor, int newFloor, string roomType)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogFloorTransition(oldFloor, newFloor, roomType, BattleLogManager.LogSource.FLOOR);
    }
    
    /// <summary>
    /// イベント結果ログ
    /// </summary>
    public void LogEventResult(string eventName, string result)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogEventResult(eventName, result, BattleLogManager.LogSource.EVENT);
    }
    
    /// <summary>
    /// 実績達成ログ
    /// </summary>
    public void LogAchievement(string achievementName, string description)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogAchievement(achievementName, description, BattleLogManager.LogSource.EVENT);
    }
    
    /// <summary>
    /// セーブ/ロードログ
    /// </summary>
    public void LogSaveLoad(string operation, string slot)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogSaveLoad(operation, slot, BattleLogManager.LogSource.SYSTEM);
    }
    
    #endregion
    
    #region 失敗・例外統合
    
    /// <summary>
    /// 行動不能ログ
    /// </summary>
    public void LogActionBlocked(string reason, BattleLogManager.LogSource source)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogActionBlocked(reason, source);
    }
    
    /// <summary>
    /// コスト不足ログ
    /// </summary>
    public void LogInsufficientCost(string costType, int required, int current, BattleLogManager.LogSource source)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogInsufficientCost(costType, required, current, source);
    }
    
    /// <summary>
    /// 対象不適ログ
    /// </summary>
    public void LogInvalidTarget(string reason, BattleLogManager.LogSource source)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogInvalidTarget(reason, source);
    }
    
    /// <summary>
    /// クールダウン中ログ
    /// </summary>
    public void LogCooldownActive(string abilityName, int remainingTurns, BattleLogManager.LogSource source)
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.LogCooldownActive(abilityName, remainingTurns, source);
    }
    
    #endregion
    
    #region ユーティリティ
    
    /// <summary>
    /// 現在のターンを取得
    /// </summary>
    private int GetCurrentTurn()
    {
        if (TurnManager.Instance != null)
        {
            // TurnManagerから現在のターンを取得する実装
            return 1; // 仮の実装
        }
        return 0;
    }
    
    /// <summary>
    /// カスタムログエントリを追加
    /// </summary>
    public void AddCustomLog(string message, BattleLogManager.LogSeverity severity, BattleLogManager.LogTags tags, BattleLogManager.LogSource source, string details = "")
    {
        if (BattleLogManager.Instance == null) return;
        
        BattleLogManager.Instance.AddLogEntry(GetCurrentTurn(), message, severity, tags, source, details);
    }
    
    #endregion
    
    #region パブリックAPI
    
    /// <summary>
    /// 統合設定を更新
    /// </summary>
    public void UpdateIntegrationSettings(bool turnLogging, bool cardLogging, bool playerLogging, bool enemyLogging, bool damageLogging, bool statusLogging)
    {
        enableTurnLogging = turnLogging;
        enableCardLogging = cardLogging;
        enablePlayerLogging = playerLogging;
        enableEnemyLogging = enemyLogging;
        enableDamageLogging = damageLogging;
        enableStatusLogging = statusLogging;
    }
    
    /// <summary>
    /// 統合状態を取得
    /// </summary>
    public string GetIntegrationStatus()
    {
        return $"BattleLogIntegrationManager - Turn: {(enableTurnLogging ? "✓" : "✗")}, " +
               $"Card: {(enableCardLogging ? "✓" : "✗")}, " +
               $"Player: {(enablePlayerLogging ? "✓" : "✗")}, " +
               $"Enemy: {(enableEnemyLogging ? "✓" : "✗")}, " +
               $"Damage: {(enableDamageLogging ? "✓" : "✗")}, " +
               $"Status: {(enableStatusLogging ? "✓" : "✗")}";
    }
    
    #endregion
}
