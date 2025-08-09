using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// バトルログシステムのテスト用マネージャー
/// サンプルログエントリを生成してシステムの動作を確認
/// </summary>
public class BattleLogTestManager : MonoBehaviour
{
    [Header("Test Settings")]
    public bool enableAutoTest = false;
    public float testInterval = 2.0f;
    public int maxTestEntries = 10;
    
    [Header("Test Buttons")]
    public Button testTurnButton;
    public Button testCombatButton;
    public Button testStatusButton;
    public Button testMetaButton;
    public Button testFailureButton;
    public Button clearLogButton;
    
    private float testTimer = 0f;
    private int testEntryCount = 0;
    
    private void Start()
    {
        SetupTestButtons();
    }
    
    private void Update()
    {
        if (enableAutoTest)
        {
            testTimer += Time.deltaTime;
            if (testTimer >= testInterval)
            {
                testTimer = 0f;
                GenerateRandomTestLog();
            }
        }
    }
    
    /// <summary>
    /// テストボタンの設定
    /// </summary>
    private void SetupTestButtons()
    {
        if (testTurnButton != null)
        {
            testTurnButton.onClick.AddListener(TestTurnLogs);
        }
        
        if (testCombatButton != null)
        {
            testCombatButton.onClick.AddListener(TestCombatLogs);
        }
        
        if (testStatusButton != null)
        {
            testStatusButton.onClick.AddListener(TestStatusLogs);
        }
        
        if (testMetaButton != null)
        {
            testMetaButton.onClick.AddListener(TestMetaLogs);
        }
        
        if (testFailureButton != null)
        {
            testFailureButton.onClick.AddListener(TestFailureLogs);
        }
        
        if (clearLogButton != null)
        {
            clearLogButton.onClick.AddListener(ClearAllLogs);
        }
    }
    
    /// <summary>
    /// ターン関連ログのテスト
    /// </summary>
    public void TestTurnLogs()
    {
        if (BattleLogManager.Instance == null) return;
        
        int turn = Random.Range(1, 10);
        int handSize = Random.Range(3, 8);
        int deckSize = Random.Range(20, 40);
        int discardSize = Random.Range(0, 5);
        
        BattleLogManager.Instance.LogTurnHeader(turn, handSize, deckSize, discardSize);
        
        // カード使用ログ
        string[] cardNames = { "攻撃", "防御", "回復", "移動", "特殊" };
        string[] targets = { "敵A", "敵B", "プレイヤー", "全体" };
        
        for (int i = 0; i < Random.Range(1, 4); i++)
        {
            string cardName = cardNames[Random.Range(0, cardNames.Length)];
            string target = targets[Random.Range(0, targets.Length)];
            BattleLogManager.Instance.LogActionSelection(cardName, target, BattleLogManager.LogSource.PLAYER);
        }
        
        // ターン終了サマリ
        BattleLogManager.Instance.LogTurnSummary(
            Random.Range(1, 4),
            Random.Range(0, 3),
            Random.Range(0, 2) == 1
        );
        
        Debug.Log("ターン関連ログのテストを実行しました");
    }
    
    /// <summary>
    /// 戦闘関連ログのテスト
    /// </summary>
    public void TestCombatLogs()
    {
        if (BattleLogManager.Instance == null) return;
        
        // ダメージ結果ログ
        for (int i = 0; i < Random.Range(2, 5); i++)
        {
            int damage = Random.Range(5, 25);
            bool isCritical = Random.Range(0, 10) == 0; // 10%でクリティカル
            int reduction = Random.Range(0, 5);
            
            BattleLogManager.Instance.LogDamageResult(damage, isCritical, reduction, BattleLogManager.LogSource.PLAYER);
        }
        
        // 被弾結果ログ
        string[] attackers = { "敵A", "敵B", "敵C" };
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            string attacker = attackers[Random.Range(0, attackers.Length)];
            int damage = Random.Range(3, 15);
            int reduction = Random.Range(0, 3);
            
            BattleLogManager.Instance.LogDamageReceived(attacker, damage, reduction, BattleLogManager.LogSource.ENEMY);
        }
        
        // HP変更ログ
        string[] targets = { "プレイヤー", "敵A", "敵B" };
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            string target = targets[Random.Range(0, targets.Length)];
            int oldHP = Random.Range(10, 30);
            int newHP = Random.Range(5, oldHP);
            int maxHP = Random.Range(20, 40);
            
            BattleLogManager.Instance.LogHPChange(target, oldHP, newHP, maxHP, BattleLogManager.LogSource.SYSTEM);
        }
        
        // 撃破ログ
        if (Random.Range(0, 3) == 0)
        {
            string[] enemies = { "敵A", "敵B", "敵C" };
            string defeated = enemies[Random.Range(0, enemies.Length)];
            BattleLogManager.Instance.LogDeath(defeated, false, BattleLogManager.LogSource.ENEMY);
        }
        
        Debug.Log("戦闘関連ログのテストを実行しました");
    }
    
    /// <summary>
    /// ステータス関連ログのテスト
    /// </summary>
    public void TestStatusLogs()
    {
        if (BattleLogManager.Instance == null) return;
        
        string[] statusNames = { "毒", "麻痺", "強化", "弱化", "回復", "燃焼" };
        string[] targets = { "プレイヤー", "敵A", "敵B" };
        
        // 状態付与ログ
        for (int i = 0; i < Random.Range(2, 4); i++)
        {
            string statusName = statusNames[Random.Range(0, statusNames.Length)];
            string target = targets[Random.Range(0, targets.Length)];
            int duration = Random.Range(1, 5);
            int effectValue = Random.Range(1, 10);
            
            BattleLogManager.Instance.LogStatusApplied(target, statusName, duration, effectValue, BattleLogManager.LogSource.STATUS);
        }
        
        // スタック変更ログ
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            string statusName = statusNames[Random.Range(0, statusNames.Length)];
            int oldStacks = Random.Range(0, 3);
            int newStacks = Random.Range(0, 5);
            
            BattleLogManager.Instance.LogStackChange(statusName, oldStacks, newStacks, BattleLogManager.LogSource.STATUS);
        }
        
        // 効果失効ログ
        if (Random.Range(0, 2) == 0)
        {
            string statusName = statusNames[Random.Range(0, statusNames.Length)];
            string target = targets[Random.Range(0, targets.Length)];
            
            BattleLogManager.Instance.LogStatusExpired(target, statusName, BattleLogManager.LogSource.STATUS);
        }
        
        Debug.Log("ステータス関連ログのテストを実行しました");
    }
    
    /// <summary>
    /// メタログのテスト
    /// </summary>
    public void TestMetaLogs()
    {
        if (BattleLogManager.Instance == null) return;
        
        // 階層遷移ログ
        int oldFloor = Random.Range(1, 5);
        int newFloor = oldFloor + 1;
        string[] roomTypes = { "通常部屋", "宝箱部屋", "敵部屋", "ボス部屋", "回復部屋" };
        string roomType = roomTypes[Random.Range(0, roomTypes.Length)];
        
        BattleLogManager.Instance.LogFloorTransition(oldFloor, newFloor, roomType, BattleLogManager.LogSource.FLOOR);
        
        // イベント結果ログ
        string[] eventNames = { "宝箱発見", "罠解除", "商人との取引", "謎解き", "選択肢" };
        string[] results = { "成功", "失敗", "部分的成功", "回避" };
        
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            string eventName = eventNames[Random.Range(0, eventNames.Length)];
            string result = results[Random.Range(0, results.Length)];
            
            BattleLogManager.Instance.LogEventResult(eventName, result, BattleLogManager.LogSource.EVENT);
        }
        
        // 実績達成ログ
        if (Random.Range(0, 3) == 0)
        {
            string[] achievements = { "初回クリア", "無傷クリア", "スピードラン", "コレクター", "マスター" };
            string[] descriptions = { "初回クリアを達成", "無傷でクリア", "高速クリア", "全アイテム収集", "マスターランク" };
            
            int index = Random.Range(0, achievements.Length);
            BattleLogManager.Instance.LogAchievement(achievements[index], descriptions[index], BattleLogManager.LogSource.EVENT);
        }
        
        // セーブ/ロードログ
        if (Random.Range(0, 2) == 0)
        {
            string operation = Random.Range(0, 2) == 0 ? "セーブ" : "ロード";
            string slot = $"スロット{Random.Range(1, 4)}";
            
            BattleLogManager.Instance.LogSaveLoad(operation, slot, BattleLogManager.LogSource.SYSTEM);
        }
        
        Debug.Log("メタログのテストを実行しました");
    }
    
    /// <summary>
    /// 失敗・例外ログのテスト
    /// </summary>
    public void TestFailureLogs()
    {
        if (BattleLogManager.Instance == null) return;
        
        // 行動不能ログ
        string[] reasons = { "スタン状態", "麻痺状態", "凍結状態", "睡眠状態" };
        for (int i = 0; i < Random.Range(1, 3); i++)
        {
            string reason = reasons[Random.Range(0, reasons.Length)];
            BattleLogManager.Instance.LogActionBlocked(reason, BattleLogManager.LogSource.SYSTEM);
        }
        
        // コスト不足ログ
        string[] costTypes = { "エナジー", "マナ", "怒気", "チャージ" };
        for (int i = 0; i < Random.Range(1, 2); i++)
        {
            string costType = costTypes[Random.Range(0, costTypes.Length)];
            int required = Random.Range(3, 8);
            int current = Random.Range(0, required);
            
            BattleLogManager.Instance.LogInsufficientCost(costType, required, current, BattleLogManager.LogSource.SYSTEM);
        }
        
        // 対象不適ログ
        string[] invalidReasons = { "射線外", "範囲外", "無効な対象", "ブロック中" };
        for (int i = 0; i < Random.Range(1, 2); i++)
        {
            string reason = invalidReasons[Random.Range(0, invalidReasons.Length)];
            BattleLogManager.Instance.LogInvalidTarget(reason, BattleLogManager.LogSource.SYSTEM);
        }
        
        // クールダウン中ログ
        string[] abilities = { "強力攻撃", "特殊スキル", "必殺技", "奥義" };
        for (int i = 0; i < Random.Range(1, 2); i++)
        {
            string ability = abilities[Random.Range(0, abilities.Length)];
            int remainingTurns = Random.Range(1, 4);
            
            BattleLogManager.Instance.LogCooldownActive(ability, remainingTurns, BattleLogManager.LogSource.SYSTEM);
        }
        
        Debug.Log("失敗・例外ログのテストを実行しました");
    }
    
    /// <summary>
    /// ランダムテストログの生成
    /// </summary>
    private void GenerateRandomTestLog()
    {
        if (testEntryCount >= maxTestEntries)
        {
            enableAutoTest = false;
            Debug.Log("自動テストが最大エントリ数に達しました");
            return;
        }
        
        int testType = Random.Range(0, 5);
        
        switch (testType)
        {
            case 0:
                TestTurnLogs();
                break;
            case 1:
                TestCombatLogs();
                break;
            case 2:
                TestStatusLogs();
                break;
            case 3:
                TestMetaLogs();
                break;
            case 4:
                TestFailureLogs();
                break;
        }
        
        testEntryCount++;
    }
    
    /// <summary>
    /// すべてのログをクリア
    /// </summary>
    public void ClearAllLogs()
    {
        if (BattleLogManager.Instance != null)
        {
            BattleLogManager.Instance.ClearLog();
            testEntryCount = 0;
            Debug.Log("すべてのログをクリアしました");
        }
    }
    
    /// <summary>
    /// 自動テストの開始/停止
    /// </summary>
    public void ToggleAutoTest()
    {
        enableAutoTest = !enableAutoTest;
        testTimer = 0f;
        testEntryCount = 0;
        
        Debug.Log($"自動テストを{(enableAutoTest ? "開始" : "停止")}しました");
    }
    
    /// <summary>
    /// テスト間隔の変更
    /// </summary>
    public void SetTestInterval(float interval)
    {
        testInterval = Mathf.Max(0.5f, interval);
        Debug.Log($"テスト間隔を{testInterval}秒に設定しました");
    }
    
    /// <summary>
    /// 最大テストエントリ数の変更
    /// </summary>
    public void SetMaxTestEntries(int maxEntries)
    {
        maxTestEntries = Mathf.Max(1, maxEntries);
        Debug.Log($"最大テストエントリ数を{maxTestEntries}に設定しました");
    }
}
