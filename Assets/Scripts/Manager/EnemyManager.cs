using UnityEngine;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    public GameObject enemyPrefab;
    public int enemyCount = 3;

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
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2Int pos;
            do {
                pos = new Vector2Int(Random.Range(0, 5), Random.Range(0, 5));
            } while (!GridManager.Instance.IsWalkable(pos));

            GameObject enemyObj = Instantiate(enemyPrefab);
            Enemy enemy = enemyObj.GetComponent<Enemy>();
            enemy.Initialize(pos);
            enemies.Add(enemy);
        }
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
    }

    public void RegisterEnemy(Enemy enemy)
    {
        if (!enemies.Contains(enemy))
            enemies.Add(enemy);
    }
} 