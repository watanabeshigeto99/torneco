using UnityEngine;
using System.Collections.Generic;
using System;

[DefaultExecutionOrder(-80)]
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }
    
    // イベント定義
    public static event Action<Enemy[]> OnEnemiesSpawned;

    public GameObject enemyPrefab;
    public int enemyCount = 3;
    
    [Header("Enemy Data")]
    public EnemyDataSO[] enemyDataPool; // 敵のデータプール
    
    [Header("Spawn Settings")]
    [Tooltip("プレイヤーからの最小距離（マス数）")]
    public int minDistanceFromPlayer = 3;
    
    [Tooltip("プレイヤーからの最大距離（マス数）")]
    public int maxDistanceFromPlayer = 5;
    
    [Tooltip("スポーン位置探索の最大試行回数")]
    public int maxSpawnAttempts = 100;
    
    [Header("Spawn Strategy")]
    [Tooltip("ランダムに敵を選択するか、順番に選択するか")]
    public bool useRandomEnemySelection = true;

    private List<Enemy> enemies = new List<Enemy>();

    private void Awake()
    {
        Debug.Log("EnemyManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("EnemyManager: 重複するEnemyManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyManager: enemyPrefabが設定されていません");
        }
        
        if (enemyDataPool == null || enemyDataPool.Length == 0)
        {
            Debug.LogWarning("EnemyManager: enemyDataPoolが設定されていません。デフォルトの敵が使用されます。");
        }
        
        Debug.Log("EnemyManager: Awake完了");
    }

    public void SpawnEnemies()
    {
        Debug.Log($"EnemyManager: 敵スポーン開始 敵数: {enemyCount}");
        
        if (Player.Instance == null)
        {
            Debug.LogError("EnemyManager: Player.Instanceが見つかりません");
            return;
        }
        
        if (GridManager.Instance == null)
        {
            Debug.LogError("EnemyManager: GridManager.Instanceが見つかりません");
            return;
        }
        
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemyManager: enemyPrefabが設定されていません");
            return;
        }
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        Debug.Log($"EnemyManager: プレイヤー位置: {playerPos}");
        
        int spawnedCount = 0;
        
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int pos;
            int attempts = 0;
            bool validPositionFound = false;
            
            // ランダムな距離制限を設定（ばらつきを出す）
            int minDist = UnityEngine.Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            int maxDist = maxDistanceFromPlayer;
            
            do {
                pos = new Vector2Int(UnityEngine.Random.Range(0, GridManager.Instance.width), 
                                   UnityEngine.Random.Range(0, GridManager.Instance.height));
                attempts++;
                
                // プレイヤーとの距離をチェック
                float distanceFromPlayer = Vector2Int.Distance(pos, playerPos);
                
                // 条件を満たすかチェック
                if (distanceFromPlayer >= minDist && 
                    distanceFromPlayer <= maxDist && 
                    GridManager.Instance.IsWalkable(pos))
                {
                    validPositionFound = true;
                }
                
            } while (!validPositionFound && attempts < maxSpawnAttempts);
            
            if (validPositionFound)
            {
                GameObject enemyObj = Instantiate(enemyPrefab);
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                
                if (enemy == null)
                {
                    Debug.LogError($"EnemyManager: 敵{i + 1}にEnemyコンポーネントが見つかりません");
                    Destroy(enemyObj);
                    continue;
                }
                
                // 敵のデータを選択
                EnemyDataSO selectedData = SelectEnemyData(i);
                
                // 敵を初期化
                enemy.Initialize(pos, selectedData);
                enemies.Add(enemy);
                
                // ターン制システムに敵を登録
                RegisterEnemy(enemy);
                
                spawnedCount++;
                string enemyName = selectedData != null ? selectedData.enemyName : "敵";
                Debug.Log($"EnemyManager: 敵{i + 1}スポーン完了 位置: {pos}, 種類: {enemyName}");
                
                // 敵の詳細情報をログ出力
                if (selectedData != null)
                {
                    Debug.Log($"EnemyManager: {enemyName}の設定 - HP: {selectedData.maxHP}, 攻撃力: {selectedData.attackPower}, 移動パターン: {selectedData.movementPattern}, 攻撃パターン: {selectedData.attackPattern}");
                }
            }
            else
            {
                Debug.LogWarning($"EnemyManager: 敵{i + 1}のスポーン位置が見つかりませんでした（試行回数: {attempts}）");
            }
        }
        
        Debug.Log($"EnemyManager: 敵スポーン完了 成功数: {spawnedCount}/{enemyCount}");
        
        // 敵スポーン完了イベントを発行
        OnEnemiesSpawned?.Invoke(enemies.ToArray());
        Debug.Log($"EnemyManager: 敵スポーン完了 スポーン数: {spawnedCount}");
    }
    
    // 敵のデータを選択
    private EnemyDataSO SelectEnemyData(int enemyIndex)
    {
        if (enemyDataPool == null || enemyDataPool.Length == 0)
        {
            Debug.LogWarning("EnemyManager: enemyDataPoolが空のため、デフォルトの敵を使用します");
            return null;
        }
        
        if (useRandomEnemySelection)
        {
            // ランダムに選択
            return enemyDataPool[UnityEngine.Random.Range(0, enemyDataPool.Length)];
        }
        else
        {
            // 順番に選択（インデックスをループ）
            return enemyDataPool[enemyIndex % enemyDataPool.Length];
        }
    }

    public void EnemyTurn()
    {
        Debug.Log("EnemyManager: 敵ターン開始");
        
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("EnemyManager: 敵が存在しません");
            return;
        }
        
        Debug.Log($"EnemyManager: 敵の数: {enemies.Count}");
        
        int activeEnemies = 0;
        foreach (var enemy in enemies.ToArray())
        {
            if (enemy != null)
            {
                string enemyName = enemy.enemyData != null ? enemy.enemyData.enemyName : "敵";
                Debug.Log($"EnemyManager: {enemyName}のターンを実行 位置: {enemy.gridPosition}");
                enemy.Act();
                activeEnemies++;
            }
            else
            {
                Debug.LogWarning("EnemyManager: nullの敵をスキップ");
            }
        }
        
        Debug.Log($"EnemyManager: 敵ターン実行完了 アクティブ敵数: {activeEnemies}");
        
        // 死亡した敵をリストから削除
        int beforeCount = enemies.Count;
        enemies.RemoveAll(e => e == null);
        int afterCount = enemies.Count;
        
        if (beforeCount != afterCount)
        {
            Debug.Log($"EnemyManager: 死亡した敵を削除 {beforeCount} → {afterCount}");
        }
        
        // 敵のターン終了後に視界範囲を更新
        if (Player.Instance != null && GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(Player.Instance.gridPosition);
            Debug.Log("EnemyManager: 視界範囲更新完了");
        }
        else
        {
            Debug.LogWarning("EnemyManager: Player.InstanceまたはGridManager.Instanceが見つからないため視界範囲更新をスキップ");
        }
        
        Debug.Log("EnemyManager: 敵ターン完了");
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (enemy == null)
        {
            Debug.LogError("EnemyManager: 登録しようとした敵がnullです");
            return;
        }
        
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
            Debug.Log($"EnemyManager: 敵を登録 位置: {enemy.gridPosition}");
        }
        else
        {
            Debug.LogWarning("EnemyManager: 既に登録済みの敵です");
        }
    }
    
    // 外部から敵リストを取得するためのメソッド
    public List<Enemy> GetEnemies()
    {
        return enemies;
    }
    
    // 敵の数を取得
    public int GetEnemyCount()
    {
        return enemies?.Count ?? 0;
    }
} 