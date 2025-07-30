using UnityEngine;
using System.Collections.Generic;

[DefaultExecutionOrder(-80)]
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public GameObject enemyPrefab;
    public int enemyCount = 3;
    
    [Header("Spawn Settings")]
    [Tooltip("プレイヤーからの最小距離（マス数）")]
    public int minDistanceFromPlayer = 3;
    
    [Tooltip("プレイヤーからの最大距離（マス数）")]
    public int maxDistanceFromPlayer = 5;
    
    [Tooltip("スポーン位置探索の最大試行回数")]
    public int maxSpawnAttempts = 100;

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
            int minDist = Random.Range(minDistanceFromPlayer, maxDistanceFromPlayer);
            int maxDist = maxDistanceFromPlayer;
            
            do {
                pos = new Vector2Int(Random.Range(0, GridManager.Instance.width), 
                                   Random.Range(0, GridManager.Instance.height));
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
                    continue;
                }
                
                enemy.Initialize(pos);
                enemies.Add(enemy);
                
                // ターン制システムに敵を登録
                RegisterEnemy(enemy);
                
                spawnedCount++;
                Debug.Log($"EnemyManager: 敵{i + 1}スポーン完了 位置: {pos}");
            }
            else
            {
                Debug.LogWarning($"EnemyManager: 敵{i + 1}のスポーン位置が見つかりませんでした（試行回数: {attempts}）");
            }
        }
        
        Debug.Log($"EnemyManager: 敵スポーン完了 成功数: {spawnedCount}/{enemyCount}");
    }

    public void EnemyTurn()
    {
        Debug.Log("EnemyManager: 敵ターン開始");
        
        if (enemies == null || enemies.Count == 0)
        {
            Debug.LogWarning("EnemyManager: 敵が存在しません");
            return;
        }
        
        int activeEnemies = 0;
        foreach (var enemy in enemies.ToArray())
        {
            if (enemy != null)
            {
                enemy.Act();
                activeEnemies++;
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