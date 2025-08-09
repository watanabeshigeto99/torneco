using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// トルネコ風ローグライクカードバトルゲーム用アクティブログシステム
/// 必須ログ、詳細ログ、ステータスログ、進行ログ、失敗ログを統合管理
/// </summary>
[DefaultExecutionOrder(-30)]
public class BattleLogManager : MonoBehaviour
{
    public static BattleLogManager Instance { get; private set; }
    
    [Header("Log Display Settings")]
    public ScrollRect logScrollRect;
    public TextMeshProUGUI logText;
    public int maxLogLines = 100;
    public bool enableDetailedLogs = true;
    public bool enableStatusLogs = true;
    public bool enableMetaLogs = true;
    public bool enableFailureLogs = true;
    
    [Header("Log Filtering")]
    public Toggle[] logTypeToggles;
    public Toggle[] severityToggles;
    public Toggle[] sourceToggles;
    public InputField searchField;
    
    [Header("Log Export")]
    public Button exportButton;
    public Button copyButton;
    
    [Header("Visual Settings")]
    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;
    public Color systemColor = Color.gray;
    public Color criticalColor = Color.red;
    public Color importantColor = Color.yellow;
    public Color infoColor = Color.white;
    
    // ログエントリ構造体
    [System.Serializable]
    public struct BattleLogEntry
    {
        public int turn;
        public string message;
        public LogSeverity severity;
        public LogTags tags;
        public LogSource source;
        public DateTime timestamp;
        public string details;
        
        public BattleLogEntry(int turn, string message, LogSeverity severity, LogTags tags, LogSource source, string details = "")
        {
            this.turn = turn;
            this.message = message;
            this.severity = severity;
            this.tags = tags;
            this.source = source;
            this.timestamp = DateTime.Now;
            this.details = details;
        }
    }
    
    // ログ重要度
    public enum LogSeverity
    {
        INFO,
        IMPORTANT,
        CRITICAL
    }
    
    // ログタグ
    [System.Flags]
    public enum LogTags
    {
        NONE = 0,
        ATK = 1 << 0,
        DEF = 1 << 1,
        BUFF = 1 << 2,
        DEBUFF = 1 << 3,
        DRAW = 1 << 4,
        MOVE = 1 << 5,
        AI = 1 << 6,
        RNG = 1 << 7,
        LOOT = 1 << 8,
        SYSTEM = 1 << 9,
        TURN = 1 << 10,
        DAMAGE = 1 << 11,
        HEAL = 1 << 12,
        STATUS = 1 << 13,
        DEATH = 1 << 14,
        FLOOR = 1 << 15,
        EVENT = 1 << 16,
        SAVE = 1 << 17,
        ERROR = 1 << 18
    }
    
    // ログ発生源
    public enum LogSource
    {
        PLAYER,
        ENEMY,
        SYSTEM,
        CARD,
        STATUS,
        FLOOR,
        EVENT
    }
    
    // ログエントリリスト
    private List<BattleLogEntry> logEntries = new List<BattleLogEntry>();
    private List<BattleLogEntry> filteredEntries = new List<BattleLogEntry>();
    
    // フィルタ設定
    private LogTags activeTags = LogTags.NONE;
    private LogSeverity minSeverity = LogSeverity.INFO;
    private LogSource[] activeSources = new LogSource[0];
    private string searchText = "";
    
    // 現在のターン情報
    private int currentTurn = 0;
    private int currentHandSize = 0;
    private int currentDeckSize = 0;
    private int currentDiscardSize = 0;
    
    // イベント定義
    public static event Action<BattleLogEntry> OnLogEntryAdded;
    public static event Action OnLogCleared;
    public static event Action<int> OnTurnStarted;
    public static event Action<int> OnTurnEnded;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeLogSystem();
    }
    
    private void Start()
    {
        SubscribeToEvents();
        SetupUI();
    }
    
    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    /// <summary>
    /// ログシステムの初期化
    /// </summary>
    private void InitializeLogSystem()
    {
        logEntries.Clear();
        filteredEntries.Clear();
        currentTurn = 0;
        
        // 初期ログエントリを追加
        AddLogEntry(0, "バトルログシステムを開始しました", LogSeverity.INFO, LogTags.SYSTEM, LogSource.SYSTEM);
    }
    
    /// <summary>
    /// イベントの購読
    /// </summary>
    private void SubscribeToEvents()
    {
        // ターン管理イベント
        if (TurnManager.Instance != null)
        {
            // TurnManagerのイベントを購読（実装時に追加）
        }
        
        // カードシステムイベント
        if (CardManager.Instance != null)
        {
            // CardManagerのイベントを購読（実装時に追加）
        }
        
        // プレイヤーイベント
        if (Player.Instance != null)
        {
            // Playerのイベントを購読（実装時に追加）
        }
        
        // 敵イベント
        if (EnemyManager.Instance != null)
        {
            // EnemyManagerのイベントを購読（実装時に追加）
        }
    }
    
    /// <summary>
    /// イベントの購読解除
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        // イベントの購読を解除
    }
    
    /// <summary>
    /// UI設定
    /// </summary>
    private void SetupUI()
    {
        if (exportButton != null)
        {
            exportButton.onClick.AddListener(ExportLog);
        }
        
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(CopyLog);
        }
        
        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchTextChanged);
        }
    }
    
    #region 必須ログ機能
    
    /// <summary>
    /// ターン見出しログ
    /// </summary>
    public void LogTurnHeader(int turn, int handSize, int deckSize, int discardSize)
    {
        currentTurn = turn;
        currentHandSize = handSize;
        currentDeckSize = deckSize;
        currentDiscardSize = discardSize;
        
        string message = $"[Turn {turn}] 手札: {handSize} / デッキ: {deckSize} / 捨て札: {discardSize}";
        AddLogEntry(turn, message, LogSeverity.IMPORTANT, LogTags.TURN, LogSource.SYSTEM);
        
        OnTurnStarted?.Invoke(turn);
    }
    
    /// <summary>
    /// 行動選択ログ
    /// </summary>
    public void LogActionSelection(string cardName, string target, LogSource source)
    {
        string message = $"{GetSourceDisplayName(source)} {cardName} → {target}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.ATK | LogTags.MOVE, source);
    }
    
    /// <summary>
    /// ダメージ結果ログ
    /// </summary>
    public void LogDamageResult(int damage, bool isCritical, int reduction, LogSource source)
    {
        string criticalText = isCritical ? " (クリティカル!)" : "";
        string reductionText = reduction > 0 ? $" (軽減: {reduction})" : "";
        string message = $"{GetSourceDisplayName(source)} ダメージ: {damage}{criticalText}{reductionText}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.DAMAGE, source);
    }
    
    /// <summary>
    /// 被弾結果ログ
    /// </summary>
    public void LogDamageReceived(string attacker, int damage, int reduction, LogSource source)
    {
        string reductionText = reduction > 0 ? $" (軽減: {reduction})" : "";
        string message = $"{attacker}から {damage}ダメージ{reductionText}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.DAMAGE, source);
    }
    
    /// <summary>
    /// 状態変化ログ
    /// </summary>
    public void LogStatusChange(string statusName, bool isApplied, int duration, LogSource source)
    {
        string action = isApplied ? "付与" : "解除";
        string durationText = duration > 0 ? $" ({duration}ターン)" : "";
        string message = $"{statusName} {action}{durationText}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.BUFF | LogTags.DEBUFF, source);
    }
    
    /// <summary>
    /// 撃破/被撃破ログ
    /// </summary>
    public void LogDeath(string target, bool isPlayer, LogSource source)
    {
        string message = $"{target}を撃破しました";
        if (isPlayer)
        {
            message = $"{target}が倒されました";
        }
        AddLogEntry(currentTurn, message, LogSeverity.CRITICAL, LogTags.DEATH, source);
    }
    
    /// <summary>
    /// HP・シールド変動ログ
    /// </summary>
    public void LogHPChange(string target, int oldHP, int newHP, int maxHP, LogSource source)
    {
        string message = $"{target} HP: {oldHP} → {newHP}/{maxHP}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.DAMAGE | LogTags.HEAL, source);
    }
    
    /// <summary>
    /// ターン終了サマリログ
    /// </summary>
    public void LogTurnSummary(int cardsDrawn, int statusEffectsProcessed, bool energyReset)
    {
        string message = $"ターン終了 - ドロー: {cardsDrawn}, 持続効果: {statusEffectsProcessed}";
        if (energyReset)
        {
            message += ", エナジーリセット";
        }
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.TURN, LogSource.SYSTEM);
        
        OnTurnEnded?.Invoke(currentTurn);
    }
    
    #endregion
    
    #region 詳細ログ機能
    
    /// <summary>
    /// カード移動ログ
    /// </summary>
    public void LogCardMovement(string cardName, string fromLocation, string toLocation, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"{cardName}: {fromLocation} → {toLocation}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.MOVE, source);
    }
    
    /// <summary>
    /// ドロー/サーチ結果ログ
    /// </summary>
    public void LogDrawResult(string[] drawnCards, bool isSearch, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string action = isSearch ? "サーチ" : "ドロー";
        string cards = string.Join(", ", drawnCards);
        string message = $"{action}結果: {cards}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.DRAW, source);
    }
    
    /// <summary>
    /// デッキ圧縮/肥大ログ
    /// </summary>
    public void LogDeckChange(int oldSize, int newSize, string reason, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string change = newSize > oldSize ? "肥大" : "圧縮";
        string message = $"デッキ{change}: {oldSize} → {newSize} ({reason})";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.SYSTEM, source);
    }
    
    /// <summary>
    /// タイル効果ログ
    /// </summary>
    public void LogTileEffect(string tileType, string effect, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"{tileType}効果: {effect}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.SYSTEM, source);
    }
    
    /// <summary>
    /// 命中/回避/ブロック計算ログ
    /// </summary>
    public void LogCombatCalculation(string calculation, int result, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"{calculation}: {result}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.ATK | LogTags.DEF, source);
    }
    
    /// <summary>
    /// 敵AIの意図表示ログ
    /// </summary>
    public void LogEnemyIntent(string enemyName, string intent, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"{enemyName}の意図: {intent}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.AI, source);
    }
    
    /// <summary>
    /// RNG情報ログ
    /// </summary>
    public void LogRNGInfo(string context, float value, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"RNG {context}: {value:F3}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.RNG, source);
    }
    
    /// <summary>
    /// リソース変動ログ
    /// </summary>
    public void LogResourceChange(string resourceType, int oldValue, int newValue, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"{resourceType}: {oldValue} → {newValue}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.SYSTEM, source);
    }
    
    /// <summary>
    /// 効果解決順の表示ログ
    /// </summary>
    public void LogEffectResolution(string effectName, int priority, LogSource source)
    {
        if (!enableDetailedLogs) return;
        
        string message = $"効果解決: {effectName} (優先度: {priority})";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.SYSTEM, source);
    }
    
    #endregion
    
    #region ステータス・持続効果ログ
    
    /// <summary>
    /// 状態付与ログ
    /// </summary>
    public void LogStatusApplied(string target, string statusName, int duration, int effectValue, LogSource source)
    {
        if (!enableStatusLogs) return;
        
        string message = $"{target}に{statusName}付与 ({duration}ターン, 効果値: {effectValue})";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.BUFF | LogTags.DEBUFF, source);
    }
    
    /// <summary>
    /// スタック増加/減少ログ
    /// </summary>
    public void LogStackChange(string statusName, int oldStacks, int newStacks, LogSource source)
    {
        if (!enableStatusLogs) return;
        
        string change = newStacks > oldStacks ? "増加" : "減少";
        string message = $"{statusName}スタック{change}: {oldStacks} → {newStacks}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.BUFF | LogTags.DEBUFF, source);
    }
    
    /// <summary>
    /// 効果失効ログ
    /// </summary>
    public void LogStatusExpired(string target, string statusName, LogSource source)
    {
        if (!enableStatusLogs) return;
        
        string message = $"{target}の{statusName}が失効しました";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.BUFF | LogTags.DEBUFF, source);
    }
    
    /// <summary>
    /// ターンまたぎ処理ログ
    /// </summary>
    public void LogTurnTransition(string phase, int processedEffects, LogSource source)
    {
        if (!enableStatusLogs) return;
        
        string message = $"ターン{phase}処理: {processedEffects}個の効果を処理";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.STATUS, source);
    }
    
    #endregion
    
    #region 進行・メタログ
    
    /// <summary>
    /// フロア/部屋遷移ログ
    /// </summary>
    public void LogFloorTransition(int oldFloor, int newFloor, string roomType, LogSource source)
    {
        if (!enableMetaLogs) return;
        
        string message = $"階層遷移: {oldFloor} → {newFloor} ({roomType})";
        AddLogEntry(currentTurn, message, LogSeverity.IMPORTANT, LogTags.FLOOR, source);
    }
    
    /// <summary>
    /// イベント結果ログ
    /// </summary>
    public void LogEventResult(string eventName, string result, LogSource source)
    {
        if (!enableMetaLogs) return;
        
        string message = $"イベント「{eventName}」: {result}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.EVENT, source);
    }
    
    /// <summary>
    /// 実績/クエスト達成ログ
    /// </summary>
    public void LogAchievement(string achievementName, string description, LogSource source)
    {
        if (!enableMetaLogs) return;
        
        string message = $"実績達成: {achievementName} - {description}";
        AddLogEntry(currentTurn, message, LogSeverity.IMPORTANT, LogTags.EVENT, source);
    }
    
    /// <summary>
    /// セーブ/ロードログ
    /// </summary>
    public void LogSaveLoad(string operation, string slot, LogSource source)
    {
        if (!enableMetaLogs) return;
        
        string message = $"{operation}: スロット {slot}";
        AddLogEntry(currentTurn, message, LogSeverity.INFO, LogTags.SAVE, source);
    }
    
    #endregion
    
    #region 失敗・例外ログ
    
    /// <summary>
    /// 行動不能ログ
    /// </summary>
    public void LogActionBlocked(string reason, LogSource source)
    {
        if (!enableFailureLogs) return;
        
        string message = $"行動不能: {reason}";
        AddLogEntry(currentTurn, message, LogSeverity.IMPORTANT, LogTags.ERROR, source);
    }
    
    /// <summary>
    /// コスト不足ログ
    /// </summary>
    public void LogInsufficientCost(string costType, int required, int current, LogSource source)
    {
        if (!enableFailureLogs) return;
        
        string message = $"コスト不足: {costType} 必要: {required}, 現在: {current}";
        AddLogEntry(currentTurn, message, LogSeverity.IMPORTANT, LogTags.ERROR, source);
    }
    
    /// <summary>
    /// 対象不適ログ
    /// </summary>
    public void LogInvalidTarget(string reason, LogSource source)
    {
        if (!enableFailureLogs) return;
        
        string message = $"対象不適: {reason}";
        AddLogEntry(currentTurn, message, LogSeverity.IMPORTANT, LogTags.ERROR, source);
    }
    
    /// <summary>
    /// クールダウン中の使用不可ログ
    /// </summary>
    public void LogCooldownActive(string abilityName, int remainingTurns, LogSource source)
    {
        if (!enableFailureLogs) return;
        
        string message = $"{abilityName}はクールダウン中 (残り{remainingTurns}ターン)";
        AddLogEntry(currentTurn, message, LogSeverity.IMPORTANT, LogTags.ERROR, source);
    }
    
    #endregion
    
    #region ログエントリ管理
    
    /// <summary>
    /// ログエントリを追加
    /// </summary>
    public void AddLogEntry(int turn, string message, LogSeverity severity, LogTags tags, LogSource source, string details = "")
    {
        BattleLogEntry entry = new BattleLogEntry(turn, message, severity, tags, source, details);
        logEntries.Add(entry);
        
        // ログエントリ数が上限を超えた場合、古いログから削除
        if (logEntries.Count > maxLogLines)
        {
            int removeCount = logEntries.Count - maxLogLines;
            logEntries.RemoveRange(0, removeCount);
        }
        
        // フィルタリングを適用
        ApplyFilters();
        
        // UI更新
        UpdateLogDisplay();
        
        // イベント発火
        OnLogEntryAdded?.Invoke(entry);
    }
    
    /// <summary>
    /// フィルタリングを適用
    /// </summary>
    private void ApplyFilters()
    {
        filteredEntries = logEntries.Where(entry =>
            (activeTags == LogTags.NONE || (entry.tags & activeTags) != 0) &&
            entry.severity >= minSeverity &&
            (activeSources.Length == 0 || activeSources.Contains(entry.source)) &&
            (string.IsNullOrEmpty(searchText) || 
             entry.message.ToLower().Contains(searchText.ToLower()) ||
             entry.details.ToLower().Contains(searchText.ToLower()))
        ).ToList();
    }
    
    /// <summary>
    /// ログ表示を更新
    /// </summary>
    private void UpdateLogDisplay()
    {
        if (logText == null) return;
        
        string displayText = "";
        foreach (var entry in filteredEntries.Take(maxLogLines))
        {
            displayText += FormatLogEntry(entry) + "\n";
        }
        
        logText.text = displayText;
        
        // スクロールを最下部に
        if (logScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            logScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    /// <summary>
    /// ログエントリをフォーマット
    /// </summary>
    private string FormatLogEntry(BattleLogEntry entry)
    {
        // シンプルな表示形式：メッセージのみ
        return entry.message;
        
        // 以下のコメントアウトされた形式から選択可能：
        // 1. メッセージのみ
        // return entry.message;
        
        // 2. ターン情報付き
        // string turnInfo = entry.turn > 0 ? $"[Turn {entry.turn}] " : "";
        // return $"{turnInfo}{entry.message}";
        
        // 3. 発生源付き
        // string sourceColor = GetSourceColor(entry.source);
        // return $"<color=#{sourceColor}>{GetSourceDisplayName(entry.source)}</color> {entry.message}";
        
        // 4. 重要度による色分けのみ
        // string severityColor = GetSeverityColor(entry.severity);
        // return $"<color=#{severityColor}>{entry.message}</color>";
        
        // 5. 元の完全な形式（コメントアウト）
        // string timestamp = $"[{entry.timestamp:HH:mm:ss}]";
        // string turnInfo = entry.turn > 0 ? $"[Turn {entry.turn}]" : "";
        // string sourceColor = GetSourceColor(entry.source);
        // string severityColor = GetSeverityColor(entry.severity);
        // string tags = GetTagsDisplay(entry.tags);
        // return $"{timestamp} {turnInfo} <color=#{sourceColor}>{GetSourceDisplayName(entry.source)}</color> {entry.message} <color=#{severityColor}>{tags}</color>";
    }
    
    /// <summary>
    /// 発生源の表示名を取得
    /// </summary>
    private string GetSourceDisplayName(LogSource source)
    {
        switch (source)
        {
            case LogSource.PLAYER: return "プレイヤー";
            case LogSource.ENEMY: return "敵";
            case LogSource.SYSTEM: return "システム";
            case LogSource.CARD: return "カード";
            case LogSource.STATUS: return "状態";
            case LogSource.FLOOR: return "階層";
            case LogSource.EVENT: return "イベント";
            default: return "不明";
        }
    }
    
    /// <summary>
    /// 発生源の色を取得
    /// </summary>
    private string GetSourceColor(LogSource source)
    {
        Color color = systemColor;
        switch (source)
        {
            case LogSource.PLAYER: color = playerColor; break;
            case LogSource.ENEMY: color = enemyColor; break;
            case LogSource.SYSTEM: color = systemColor; break;
        }
        return ColorUtility.ToHtmlStringRGB(color);
    }
    
    /// <summary>
    /// 重要度の色を取得
    /// </summary>
    private string GetSeverityColor(LogSeverity severity)
    {
        Color color = infoColor;
        switch (severity)
        {
            case LogSeverity.CRITICAL: color = criticalColor; break;
            case LogSeverity.IMPORTANT: color = importantColor; break;
            case LogSeverity.INFO: color = infoColor; break;
        }
        return ColorUtility.ToHtmlStringRGB(color);
    }
    
    /// <summary>
    /// タグの表示文字列を取得
    /// </summary>
    private string GetTagsDisplay(LogTags tags)
    {
        List<string> tagList = new List<string>();
        
        if ((tags & LogTags.ATK) != 0) tagList.Add("ATK");
        if ((tags & LogTags.DEF) != 0) tagList.Add("DEF");
        if ((tags & LogTags.BUFF) != 0) tagList.Add("BUFF");
        if ((tags & LogTags.DEBUFF) != 0) tagList.Add("DEBUFF");
        if ((tags & LogTags.DRAW) != 0) tagList.Add("DRAW");
        if ((tags & LogTags.MOVE) != 0) tagList.Add("MOVE");
        if ((tags & LogTags.AI) != 0) tagList.Add("AI");
        if ((tags & LogTags.RNG) != 0) tagList.Add("RNG");
        if ((tags & LogTags.LOOT) != 0) tagList.Add("LOOT");
        if ((tags & LogTags.SYSTEM) != 0) tagList.Add("SYSTEM");
        if ((tags & LogTags.TURN) != 0) tagList.Add("TURN");
        if ((tags & LogTags.DAMAGE) != 0) tagList.Add("DAMAGE");
        if ((tags & LogTags.HEAL) != 0) tagList.Add("HEAL");
        if ((tags & LogTags.STATUS) != 0) tagList.Add("STATUS");
        if ((tags & LogTags.DEATH) != 0) tagList.Add("DEATH");
        if ((tags & LogTags.FLOOR) != 0) tagList.Add("FLOOR");
        if ((tags & LogTags.EVENT) != 0) tagList.Add("EVENT");
        if ((tags & LogTags.SAVE) != 0) tagList.Add("SAVE");
        if ((tags & LogTags.ERROR) != 0) tagList.Add("ERROR");
        
        return string.Join(",", tagList);
    }
    
    #endregion
    
    #region UI操作
    
    /// <summary>
    /// 検索テキスト変更ハンドラー
    /// </summary>
    private void OnSearchTextChanged(string newText)
    {
        searchText = newText;
        ApplyFilters();
        UpdateLogDisplay();
    }
    
    /// <summary>
    /// ログをエクスポート
    /// </summary>
    private void ExportLog()
    {
        string exportPath = $"{Application.persistentDataPath}/battle_log_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string content = "";
        
        foreach (var entry in logEntries)
        {
            content += $"[{entry.timestamp:yyyy-MM-dd HH:mm:ss}] Turn {entry.turn}: {entry.message}\n";
        }
        
        try
        {
            System.IO.File.WriteAllText(exportPath, content);
            Debug.Log($"バトルログをエクスポートしました: {exportPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"ログエクスポートに失敗しました: {e.Message}");
        }
    }
    
    /// <summary>
    /// ログをクリップボードにコピー
    /// </summary>
    private void CopyLog()
    {
        string content = "";
        foreach (var entry in filteredEntries.Take(maxLogLines))
        {
            content += $"[{entry.timestamp:HH:mm:ss}] Turn {entry.turn}: {entry.message}\n";
        }
        
        GUIUtility.systemCopyBuffer = content;
        Debug.Log("バトルログをクリップボードにコピーしました");
    }
    
    /// <summary>
    /// ログをクリア
    /// </summary>
    public void ClearLog()
    {
        logEntries.Clear();
        filteredEntries.Clear();
        UpdateLogDisplay();
        OnLogCleared?.Invoke();
    }
    
    #endregion
    
    #region パブリックAPI
    
    /// <summary>
    /// ログエントリを取得
    /// </summary>
    public List<BattleLogEntry> GetLogEntries()
    {
        return new List<BattleLogEntry>(logEntries);
    }
    
    /// <summary>
    /// フィルタされたログエントリを取得
    /// </summary>
    public List<BattleLogEntry> GetFilteredLogEntries()
    {
        return new List<BattleLogEntry>(filteredEntries);
    }
    
    /// <summary>
    /// 現在のターン情報を取得
    /// </summary>
    public (int turn, int handSize, int deckSize, int discardSize) GetCurrentTurnInfo()
    {
        return (currentTurn, currentHandSize, currentDeckSize, currentDiscardSize);
    }
    
    /// <summary>
    /// フィルタ設定を更新
    /// </summary>
    public void UpdateFilters(LogTags tags, LogSeverity severity, LogSource[] sources)
    {
        activeTags = tags;
        minSeverity = severity;
        activeSources = sources;
        ApplyFilters();
        UpdateLogDisplay();
    }
    
    /// <summary>
    /// 詳細ログの有効/無効を切り替え
    /// </summary>
    public void SetDetailedLogsEnabled(bool enabled)
    {
        enableDetailedLogs = enabled;
    }
    
    /// <summary>
    /// ステータスログの有効/無効を切り替え
    /// </summary>
    public void SetStatusLogsEnabled(bool enabled)
    {
        enableStatusLogs = enabled;
    }
    
    /// <summary>
    /// メタログの有効/無効を切り替え
    /// </summary>
    public void SetMetaLogsEnabled(bool enabled)
    {
        enableMetaLogs = enabled;
    }
    
    /// <summary>
    /// 失敗ログの有効/無効を切り替え
    /// </summary>
    public void SetFailureLogsEnabled(bool enabled)
    {
        enableFailureLogs = enabled;
    }
    
    #endregion
}
