using System.Collections;
using UnityEngine;
using System;

[DefaultExecutionOrder(-70)]
public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    
    public static TurnManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("TurnManager");
            Instance = go.AddComponent<TurnManager>();
        }
        return Instance;
    }

    private bool playerTurn = true;
    private bool isProcessing = false;

    public Player player;
    public EnemyManager enemyManager;

    private void Awake()
    {
        Debug.Log("TurnManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TurnManager: 重複するTurnManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化（シーン間で保持）
        DontDestroyOnLoad(gameObject);
        
        // 基本的な変数の初期化
        playerTurn = true;
        isProcessing = false;
        
        Debug.Log("TurnManager: Awake完了");
    }

    private void Start()
    {
        Debug.Log("TurnManager: Start開始");
        
        // イベントを購読
        SubscribeToEvents();
        
        Debug.Log("TurnManager: Start完了");
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        // プレイヤーの行動イベントを購読
        Player.OnPlayerMoved += OnPlayerMoved;
        Player.OnPlayerAttacked += OnPlayerAttacked;
        Player.OnPlayerHealed += OnPlayerHealed;
        Player.OnPlayerDied += OnPlayerDied;
        
        // Unitイベントを購読
        Unit.OnUnitMoved += OnUnitMoved;
        Unit.OnUnitAttacked += OnUnitAttacked;
        Unit.OnUnitDamaged += OnUnitDamaged;
        Unit.OnUnitDied += OnUnitDied;
        
        // 全オブジェクト初期化完了イベントを購読
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
        
        Debug.Log("TurnManager: イベント購読完了");
    }
    
    private void UnsubscribeFromEvents()
    {
        // イベントの購読を解除
        Player.OnPlayerMoved -= OnPlayerMoved;
        Player.OnPlayerAttacked -= OnPlayerAttacked;
        Player.OnPlayerHealed -= OnPlayerHealed;
        Player.OnPlayerDied -= OnPlayerDied;
        
        // Unitイベントの購読を解除
        Unit.OnUnitMoved -= OnUnitMoved;
        Unit.OnUnitAttacked -= OnUnitAttacked;
        Unit.OnUnitDamaged -= OnUnitDamaged;
        Unit.OnUnitDied -= OnUnitDied;
        
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
        
        Debug.Log("TurnManager: イベント購読解除完了");
    }
    
    // イベントハンドラー
    private void OnPlayerMoved(Vector2Int newPosition)
    {
        Debug.Log($"TurnManager: プレイヤー移動イベント受信 位置: {newPosition}");
        // 移動後の処理（必要に応じて）
    }
    
    private void OnPlayerAttacked(int damage)
    {
        Debug.Log($"TurnManager: プレイヤー攻撃イベント受信 ダメージ: {damage}");
        // 攻撃後の処理（必要に応じて）
    }
    
    private void OnPlayerHealed(int healAmount)
    {
        Debug.Log($"TurnManager: プレイヤー回復イベント受信 回復量: {healAmount}");
        // 回復後の処理（必要に応じて）
    }
    
    private void OnPlayerDied()
    {
        Debug.Log("TurnManager: プレイヤー死亡イベント受信");
        // 死亡時の処理（必要に応じて）
    }
    
    // Unitイベントハンドラー
    private void OnUnitMoved(Unit unit, Vector2Int newPosition)
    {
        Debug.Log($"TurnManager: ユニット移動イベント受信 {unit.GetUnitName()} 位置: {newPosition}");
    }
    
    private void OnUnitAttacked(Unit attacker, int damage)
    {
        Debug.Log($"TurnManager: ユニット攻撃イベント受信 {attacker.GetUnitName()} ダメージ: {damage}");
    }
    
    private void OnUnitDamaged(Unit target, int damage)
    {
        Debug.Log($"TurnManager: ユニットダメージイベント受信 {target.GetUnitName()} ダメージ: {damage}");
    }
    
    private void OnUnitDied(Unit unit)
    {
        Debug.Log($"TurnManager: ユニット死亡イベント受信 {unit.GetUnitName()}");
    }
    
    private void OnAllObjectsInitialized()
    {
        Debug.Log("TurnManager: 全オブジェクト初期化完了イベント受信");
        
        // 参照を取得（全オブジェクト初期化完了後に取得）
        player = Player.Instance;
        enemyManager = EnemyManager.Instance;
        
        if (player == null)
        {
            Debug.LogError("TurnManager: Player.Instanceが見つかりません");
        }
        
        if (enemyManager == null)
        {
            Debug.LogError("TurnManager: EnemyManager.Instanceが見つかりません");
        }
        
        // ターン管理の初期化
        playerTurn = true;
        isProcessing = false;
        
        Debug.Log("TurnManager: ターン管理初期化完了 - プレイヤーターン開始");
    }

    public void OnPlayerCardUsed()
    {
        Debug.Log("TurnManager: プレイヤーカード使用");
        
        // 実行時に参照を再取得（安全のため）
        if (player == null)
        {
            player = Player.Instance;
        }
        
        if (enemyManager == null)
        {
            enemyManager = EnemyManager.Instance;
        }
        
        if (player == null)
        {
            Debug.LogError("TurnManager: playerがnullです");
            return;
        }
        
        if (enemyManager == null)
        {
            Debug.LogError("TurnManager: enemyManagerがnullです");
            return;
        }
        
        playerTurn = false;
        isProcessing = true;
        
        // 敵のターンを実行
        StartCoroutine(EnemyTurnCoroutine());
    }

    private IEnumerator EnemyTurnCoroutine()
    {
        Debug.Log("TurnManager: 敵ターン開始");
        
        yield return new WaitForSeconds(0.5f);
        
        if (enemyManager != null)
        {
            enemyManager.EnemyTurn();
        }
        else
        {
            Debug.LogError("TurnManager: enemyManagerがnullのため敵ターンをスキップ");
        }
        
        // プレイヤーのターンに戻る
        playerTurn = true;
        isProcessing = false;
        
        Debug.Log("TurnManager: 敵ターン完了、プレイヤーターンに戻る");
    }

    public bool IsPlayerTurn()
    {
        return playerTurn && !isProcessing;
    }
} 