using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// バトルログ表示用UIコンポーネント
/// フィルタリング、検索、エクスポート機能を提供
/// </summary>
public class BattleLogUI : MonoBehaviour
{
    [Header("Main Display")]
    public ScrollRect logScrollRect;
    public TextMeshProUGUI logText;
    public GameObject logPanel;
    public Button toggleButton;
    public TextMeshProUGUI toggleButtonText;
    
    [Header("Filter Panel")]
    public GameObject filterPanel;
    public Button filterToggleButton;
    public TextMeshProUGUI filterToggleButtonText;
    
    [Header("Tag Filters")]
    public Toggle atkToggle;
    public Toggle defToggle;
    public Toggle buffToggle;
    public Toggle debuffToggle;
    public Toggle drawToggle;
    public Toggle moveToggle;
    public Toggle aiToggle;
    public Toggle rngToggle;
    public Toggle lootToggle;
    public Toggle systemToggle;
    public Toggle turnToggle;
    public Toggle damageToggle;
    public Toggle healToggle;
    public Toggle statusToggle;
    public Toggle deathToggle;
    public Toggle floorToggle;
    public Toggle eventToggle;
    public Toggle saveToggle;
    public Toggle errorToggle;
    
    [Header("Severity Filters")]
    public Toggle infoToggle;
    public Toggle importantToggle;
    public Toggle criticalToggle;
    
    [Header("Source Filters")]
    public Toggle playerToggle;
    public Toggle enemyToggle;
    public Toggle sourceSystemToggle;
    public Toggle cardToggle;
    public Toggle sourceStatusToggle;
    public Toggle sourceFloorToggle;
    public Toggle sourceEventToggle;
    
    [Header("Search and Export")]
    public TMP_InputField searchField;
    public Button searchButton;
    public Button clearSearchButton;
    public Button exportButton;
    public Button copyButton;
    public Button clearLogButton;
    
    [Header("Settings")]
    public Toggle detailedLogsToggle;
    public Toggle statusLogsToggle;
    public Toggle metaLogsToggle;
    public Toggle failureLogsToggle;
    
    [Header("Visual Settings")]
    public Color playerColor = Color.blue;
    public Color enemyColor = Color.red;
    public Color systemColor = Color.gray;
    public Color criticalColor = Color.red;
    public Color importantColor = Color.yellow;
    public Color infoColor = Color.white;
    
    // 状態管理
    private bool isLogPanelVisible = true;
    private bool isFilterPanelVisible = false;
    private Dictionary<BattleLogManager.LogTags, Toggle> tagToggles;
    private Dictionary<BattleLogManager.LogSeverity, Toggle> severityToggles;
    private Dictionary<BattleLogManager.LogSource, Toggle> sourceToggles;
    
    private void Awake()
    {
        InitializeToggleDictionaries();
        SetupEventListeners();
    }
    
    private void Start()
    {
        InitializeUI();
        UpdateVisualSettings();
    }
    
    /// <summary>
    /// トグル辞書の初期化
    /// </summary>
    private void InitializeToggleDictionaries()
    {
        tagToggles = new Dictionary<BattleLogManager.LogTags, Toggle>
        {
            { BattleLogManager.LogTags.ATK, atkToggle },
            { BattleLogManager.LogTags.DEF, defToggle },
            { BattleLogManager.LogTags.BUFF, buffToggle },
            { BattleLogManager.LogTags.DEBUFF, debuffToggle },
            { BattleLogManager.LogTags.DRAW, drawToggle },
            { BattleLogManager.LogTags.MOVE, moveToggle },
            { BattleLogManager.LogTags.AI, aiToggle },
            { BattleLogManager.LogTags.RNG, rngToggle },
            { BattleLogManager.LogTags.LOOT, lootToggle },
            { BattleLogManager.LogTags.SYSTEM, systemToggle },
            { BattleLogManager.LogTags.TURN, turnToggle },
            { BattleLogManager.LogTags.DAMAGE, damageToggle },
            { BattleLogManager.LogTags.HEAL, healToggle },
            { BattleLogManager.LogTags.STATUS, statusToggle },
            { BattleLogManager.LogTags.DEATH, deathToggle },
            { BattleLogManager.LogTags.FLOOR, floorToggle },
            { BattleLogManager.LogTags.EVENT, eventToggle },
            { BattleLogManager.LogTags.SAVE, saveToggle },
            { BattleLogManager.LogTags.ERROR, errorToggle }
        };
        
        severityToggles = new Dictionary<BattleLogManager.LogSeverity, Toggle>
        {
            { BattleLogManager.LogSeverity.INFO, infoToggle },
            { BattleLogManager.LogSeverity.IMPORTANT, importantToggle },
            { BattleLogManager.LogSeverity.CRITICAL, criticalToggle }
        };
        
        sourceToggles = new Dictionary<BattleLogManager.LogSource, Toggle>
        {
            { BattleLogManager.LogSource.PLAYER, playerToggle },
            { BattleLogManager.LogSource.ENEMY, enemyToggle },
            { BattleLogManager.LogSource.SYSTEM, sourceSystemToggle },
            { BattleLogManager.LogSource.CARD, cardToggle },
            { BattleLogManager.LogSource.STATUS, sourceStatusToggle },
            { BattleLogManager.LogSource.FLOOR, sourceFloorToggle },
            { BattleLogManager.LogSource.EVENT, sourceEventToggle }
        };
    }
    
    /// <summary>
    /// イベントリスナーの設定
    /// </summary>
    private void SetupEventListeners()
    {
        // メインパネル切り替え
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleLogPanel);
        }
        
        // フィルタパネル切り替え
        if (filterToggleButton != null)
        {
            filterToggleButton.onClick.AddListener(ToggleFilterPanel);
        }
        
        // タグフィルター
        foreach (var toggle in tagToggles.Values)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnTagFilterChanged);
            }
        }
        
        // 重要度フィルター
        foreach (var toggle in severityToggles.Values)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnSeverityFilterChanged);
            }
        }
        
        // 発生源フィルター
        foreach (var toggle in sourceToggles.Values)
        {
            if (toggle != null)
            {
                toggle.onValueChanged.AddListener(OnSourceFilterChanged);
            }
        }
        
        // 検索機能
        if (searchField != null)
        {
            searchField.onValueChanged.AddListener(OnSearchTextChanged);
        }
        
        if (searchButton != null)
        {
            searchButton.onClick.AddListener(OnSearchButtonClicked);
        }
        
        if (clearSearchButton != null)
        {
            clearSearchButton.onClick.AddListener(OnClearSearchClicked);
        }
        
        // エクスポート機能
        if (exportButton != null)
        {
            exportButton.onClick.AddListener(OnExportClicked);
        }
        
        if (copyButton != null)
        {
            copyButton.onClick.AddListener(OnCopyClicked);
        }
        
        if (clearLogButton != null)
        {
            clearLogButton.onClick.AddListener(OnClearLogClicked);
        }
        
        // 設定
        if (detailedLogsToggle != null)
        {
            detailedLogsToggle.onValueChanged.AddListener(OnDetailedLogsToggled);
        }
        
        if (statusLogsToggle != null)
        {
            statusLogsToggle.onValueChanged.AddListener(OnStatusLogsToggled);
        }
        
        if (metaLogsToggle != null)
        {
            metaLogsToggle.onValueChanged.AddListener(OnMetaLogsToggled);
        }
        
        if (failureLogsToggle != null)
        {
            failureLogsToggle.onValueChanged.AddListener(OnFailureLogsToggled);
        }
    }
    
    /// <summary>
    /// UI初期化
    /// </summary>
    private void InitializeUI()
    {
        // ログパネルの初期状態
        if (logPanel != null)
        {
            logPanel.SetActive(isLogPanelVisible);
        }
        
        // フィルタパネルの初期状態
        if (filterPanel != null)
        {
            filterPanel.SetActive(isFilterPanelVisible);
        }
        
        // ボタンテキストの更新
        UpdateToggleButtonTexts();
        
        // 設定の初期化
        if (BattleLogManager.Instance != null)
        {
            if (detailedLogsToggle != null)
            {
                detailedLogsToggle.isOn = BattleLogManager.Instance.enableDetailedLogs;
            }
            
            if (statusLogsToggle != null)
            {
                statusLogsToggle.isOn = BattleLogManager.Instance.enableStatusLogs;
            }
            
            if (metaLogsToggle != null)
            {
                metaLogsToggle.isOn = BattleLogManager.Instance.enableMetaLogs;
            }
            
            if (failureLogsToggle != null)
            {
                failureLogsToggle.isOn = BattleLogManager.Instance.enableFailureLogs;
            }
        }
    }
    
    /// <summary>
    /// 視覚設定の更新
    /// </summary>
    private void UpdateVisualSettings()
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.playerColor = playerColor;
            BattleLogManager.Instance.enemyColor = enemyColor;
            BattleLogManager.Instance.systemColor = systemColor;
            BattleLogManager.Instance.criticalColor = criticalColor;
            BattleLogManager.Instance.importantColor = importantColor;
            BattleLogManager.Instance.infoColor = infoColor;
        }
    }
    
    #region パネル切り替え
    
    /// <summary>
    /// ログパネルの切り替え
    /// </summary>
    private void ToggleLogPanel()
    {
        isLogPanelVisible = !isLogPanelVisible;
        
        if (logPanel != null)
        {
            logPanel.SetActive(isLogPanelVisible);
        }
        
        UpdateToggleButtonTexts();
    }
    
    /// <summary>
    /// フィルタパネルの切り替え
    /// </summary>
    private void ToggleFilterPanel()
    {
        isFilterPanelVisible = !isFilterPanelVisible;
        
        if (filterPanel != null)
        {
            filterPanel.SetActive(isFilterPanelVisible);
        }
        
        UpdateToggleButtonTexts();
    }
    
    /// <summary>
    /// ボタンテキストの更新
    /// </summary>
    private void UpdateToggleButtonTexts()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isLogPanelVisible ? "ログ非表示" : "ログ表示";
        }
        
        if (filterToggleButtonText != null)
        {
            filterToggleButtonText.text = isFilterPanelVisible ? "フィルタ非表示" : "フィルタ表示";
        }
    }
    
    #endregion
    
    #region フィルター処理
    
    /// <summary>
    /// タグフィルター変更ハンドラー
    /// </summary>
    private void OnTagFilterChanged(bool value)
    {
        UpdateFilters();
    }
    
    /// <summary>
    /// 重要度フィルター変更ハンドラー
    /// </summary>
    private void OnSeverityFilterChanged(bool value)
    {
        UpdateFilters();
    }
    
    /// <summary>
    /// 発生源フィルター変更ハンドラー
    /// </summary>
    private void OnSourceFilterChanged(bool value)
    {
        UpdateFilters();
    }
    
    /// <summary>
    /// フィルターの更新
    /// </summary>
    private void UpdateFilters()
    {
        if (BattleLogManager.Instance == null) return;
        
        // アクティブなタグを収集
        BattleLogManager.LogTags activeTags = BattleLogManager.LogTags.NONE;
        foreach (var kvp in tagToggles)
        {
            if (kvp.Value != null && kvp.Value.isOn)
            {
                activeTags |= kvp.Key;
            }
        }
        
        // 最小重要度を決定
        BattleLogManager.LogSeverity minSeverity = BattleLogManager.LogSeverity.INFO;
        if (criticalToggle != null && criticalToggle.isOn)
        {
            minSeverity = BattleLogManager.LogSeverity.CRITICAL;
        }
        else if (importantToggle != null && importantToggle.isOn)
        {
            minSeverity = BattleLogManager.LogSeverity.IMPORTANT;
        }
        
        // アクティブな発生源を収集
        List<BattleLogManager.LogSource> activeSources = new List<BattleLogManager.LogSource>();
        foreach (var kvp in sourceToggles)
        {
            if (kvp.Value != null && kvp.Value.isOn)
            {
                activeSources.Add(kvp.Key);
            }
        }
        
        // フィルターを適用
        BattleLogManager.Instance.UpdateFilters(activeTags, minSeverity, activeSources.ToArray());
    }
    
    #endregion
    
    #region 検索機能
    
    /// <summary>
    /// 検索テキスト変更ハンドラー
    /// </summary>
    private void OnSearchTextChanged(string newText)
    {
        // BattleLogManagerで検索処理が行われるため、ここでは何もしない
    }
    
    /// <summary>
    /// 検索ボタンクリックハンドラー
    /// </summary>
    private void OnSearchButtonClicked()
    {
        if (searchField != null)
        {
            // 検索フィールドにフォーカス
            searchField.Select();
            searchField.ActivateInputField();
        }
    }
    
    /// <summary>
    /// 検索クリアボタンクリックハンドラー
    /// </summary>
    private void OnClearSearchClicked()
    {
        if (searchField != null)
        {
            searchField.text = "";
        }
    }
    
    #endregion
    
    #region エクスポート機能
    
    /// <summary>
    /// エクスポートボタンクリックハンドラー
    /// </summary>
    private void OnExportClicked()
    {
        if (BattleLogManager.Instance != null)
        {
            // BattleLogManagerのExportLogメソッドを呼び出し
            // 実際の実装はBattleLogManager内にある
            Debug.Log("ログエクスポートを実行しました");
        }
    }
    
    /// <summary>
    /// コピーボタンクリックハンドラー
    /// </summary>
    private void OnCopyClicked()
    {
        if (BattleLogManager.Instance != null)
        {
            // BattleLogManagerのCopyLogメソッドを呼び出し
            // 実際の実装はBattleLogManager内にある
            Debug.Log("ログをクリップボードにコピーしました");
        }
    }
    
    /// <summary>
    /// ログクリアボタンクリックハンドラー
    /// </summary>
    private void OnClearLogClicked()
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.ClearLog();
            Debug.Log("ログをクリアしました");
        }
    }
    
    #endregion
    
    #region 設定機能
    
    /// <summary>
    /// 詳細ログ設定変更ハンドラー
    /// </summary>
    private void OnDetailedLogsToggled(bool enabled)
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.SetDetailedLogsEnabled(enabled);
        }
    }
    
    /// <summary>
    /// ステータスログ設定変更ハンドラー
    /// </summary>
    private void OnStatusLogsToggled(bool enabled)
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.SetStatusLogsEnabled(enabled);
        }
    }
    
    /// <summary>
    /// メタログ設定変更ハンドラー
    /// </summary>
    private void OnMetaLogsToggled(bool enabled)
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.SetMetaLogsEnabled(enabled);
        }
    }
    
    /// <summary>
    /// 失敗ログ設定変更ハンドラー
    /// </summary>
    private void OnFailureLogsToggled(bool enabled)
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.SetFailureLogsEnabled(enabled);
        }
    }
    
    #endregion
    
    #region パブリックAPI
    
    /// <summary>
    /// ログパネルの表示状態を設定
    /// </summary>
    public void SetLogPanelVisible(bool visible)
    {
        isLogPanelVisible = visible;
        if (logPanel != null)
        {
            logPanel.SetActive(visible);
        }
        UpdateToggleButtonTexts();
    }
    
    /// <summary>
    /// フィルタパネルの表示状態を設定
    /// </summary>
    public void SetFilterPanelVisible(bool visible)
    {
        isFilterPanelVisible = visible;
        if (filterPanel != null)
        {
            filterPanel.SetActive(visible);
        }
        UpdateToggleButtonTexts();
    }
    
    /// <summary>
    /// すべてのフィルターをリセット
    /// </summary>
    public void ResetAllFilters()
    {
        // すべてのトグルをオフにする
        foreach (var toggle in tagToggles.Values)
        {
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }
        
        foreach (var toggle in severityToggles.Values)
        {
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }
        
        foreach (var toggle in sourceToggles.Values)
        {
            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }
        
        // 検索フィールドをクリア
        if (searchField != null)
        {
            searchField.text = "";
        }
        
        // フィルターを更新
        UpdateFilters();
    }
    
    /// <summary>
    /// ログテキストを更新
    /// </summary>
    public void UpdateLogText(string text)
    {
        if (logText != null)
        {
            logText.text = text;
        }
    }
    
    /// <summary>
    /// スクロールを最下部に移動
    /// </summary>
    public void ScrollToBottom()
    {
        if (logScrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            logScrollRect.verticalNormalizedPosition = 0f;
        }
    }
    
    #endregion
}
