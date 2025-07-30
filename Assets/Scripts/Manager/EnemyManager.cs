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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SpawnEnemies()
    {
        StartCoroutine(SpawnEnemiesCoroutine());
    }
    
    private System.Collections.IEnumerator SpawnEnemiesCoroutine()
    {
        if (Player.Instance == null)
        {
            Debug.LogError("プレイヤーが見つかりません");
            yield break;
        }
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        
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
                
                // 敵生成後に1フレーム待機
                yield return new WaitForEndOfFrame();
                
                Enemy enemy = enemyObj.GetComponent<Enemy>();
                enemy.Initialize(pos);
                enemies.Add(enemy);
                
                // ターン制システムに敵を登録
                RegisterEnemy(enemy);
            }
            else
            {
                Debug.LogWarning($"敵{i + 1}のスポーン位置が見つかりませんでした（試行回数: {attempts}）");
            }
        }
        
        // 全敵の生成完了後に1フレーム待機
        yield return new WaitForEndOfFrame();
    }

    public void EnemyTurn()
    {
        foreach (var enemy in enemies.ToArray())
        {
            if (enemy != null)
            {
                enemy.Act();
            }
        }
        
        // 死亡した敵をリストから削除
        enemies.RemoveAll(e => e == null);
        
        // 敵のターン終了後に視界範囲を更新
        Player player = FindObjectOfType<Player>();
        if (player != null && GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(player.gridPosition);
        }
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }
} 