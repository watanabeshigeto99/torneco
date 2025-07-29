using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int width = 5;
    public int height = 5;
    public float tileSpacing = 1.1f;
    public GameObject tilePrefab;

    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject exitPrefab;

    [Header("Vision Settings")]
    [Range(1, 10)]
    [Tooltip("プレイヤーを中心とした視界範囲（マス数）")]
    public int visionRange = 8; // 視界範囲（プレイヤーからの距離）
    
    [Tooltip("視界範囲をデバッグログに表示")]
    public bool showVisionRange = false; // 視界範囲を視覚的に表示
    private Tile[] allTiles;
    private Enemy[] allEnemies;
    private GameObject exitObject;
    private Vector2Int exitPosition; // Exitの位置を記録

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        GenerateGrid();
        SpawnPlayer(new Vector2Int(width / 2, height / 2));
        SpawnExit();
        
        // 敵をスポーン
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.SpawnEnemies();
        }
        
        // 全オブジェクトの参照を保存
        StartCoroutine(StoreObjectsAfterSpawn());
    }

    private System.Collections.IEnumerator StoreObjectsAfterSpawn()
    {
        // 全てのオブジェクトが生成されるまで待機
        yield return new WaitForSeconds(0.1f);
        
        StoreAllObjects();
        
        // 初期視界範囲を設定
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            UpdateVisionRange(player.gridPosition);
        }
    }

    private void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";

                Tile tileScript = tile.GetComponent<Tile>();
                tileScript.Initialize(x, y);
            }
        }
    }

    private void SpawnPlayer(Vector2Int position)
    {
        Vector3 pos = new Vector3(position.x * tileSpacing, position.y * tileSpacing, 0);
        GameObject playerObj = Instantiate(playerPrefab, pos, Quaternion.identity);
        
        // プレイヤーの初期化を実行
        Player playerScript = playerObj.GetComponent<Player>();
        if (playerScript != null)
        {
            playerScript.InitializePosition();
        }
        
        // プレイヤー生成後にカメラ追従を開始
        StartCoroutine(NotifyCameraAfterPlayerSpawn(playerObj));
    }

    private System.Collections.IEnumerator NotifyCameraAfterPlayerSpawn(GameObject playerObj)
    {
        // プレイヤーの初期化が完了するまで待機
        yield return new WaitForEndOfFrame();
        
        CameraFollow cameraFollow = FindObjectOfType<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.OnPlayerMoved(playerObj.transform.position);
        }
    }

    private void SpawnExit()
    {
        Vector2Int exitPos;
        do
        {
            exitPos = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
        }
        while (exitPos == new Vector2Int(width / 2, height / 2)); // プレイヤー位置と被らない

        Vector3 pos = new Vector3(exitPos.x * tileSpacing, exitPos.y * tileSpacing, 0);
        GameObject exitObj = Instantiate(exitPrefab, pos, Quaternion.identity);
        
        // Exit位置を記録
        exitPosition = exitPos;
        exitObject = exitObj;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x * tileSpacing, gridPos.y * tileSpacing, 0);
    }

    public bool IsInsideGrid(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsOccupied(Vector2Int pos)
    {
        // プレイヤーの位置チェック
        Player player = FindObjectOfType<Player>();
        if (player != null && player.gridPosition == pos)
            return true;

        // 敵の位置チェック
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in enemies)
        {
            if (enemy.gridPosition == pos)
                return true;
        }

        return false;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        return IsInsideGrid(pos) && !IsOccupied(pos);
    }

    // 視界範囲内かどうかをチェック
    public bool IsInVisionRange(Vector2Int position, Vector2Int playerPosition)
    {
        int distance = Mathf.Max(Mathf.Abs(position.x - playerPosition.x), Mathf.Abs(position.y - playerPosition.y));
        return distance <= visionRange;
    }

    // 視界範囲を更新
    public void UpdateVisionRange(Vector2Int playerPosition)
    {
        
        // タイルの表示/非表示を更新
        if (allTiles != null)
        {
            foreach (Tile tile in allTiles)
            {
                if (tile != null)
                {
                    bool inVision = IsInVisionRange(new Vector2Int(tile.x, tile.y), playerPosition);
                    tile.gameObject.SetActive(inVision);
                }
            }
        }

        // 敵の表示/非表示を更新
        if (allEnemies != null)
        {
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null)
                {
                    bool inVision = IsInVisionRange(enemy.gridPosition, playerPosition);
                    enemy.gameObject.SetActive(inVision);
                }
            }
        }

        // Exitの表示/非表示を更新
        if (exitObject != null)
        {
            Vector2Int exitPos = GetExitPosition();
            bool inVision = IsInVisionRange(exitPos, playerPosition);
            exitObject.SetActive(inVision);
        }
        
        // 視界範囲を視覚的に表示（デバッグ用）
        if (showVisionRange)
        {
            ShowVisionRangeDebug(playerPosition);
        }
    }
    
    // 視界範囲を視覚的に表示（デバッグ用）
    private void ShowVisionRangeDebug(Vector2Int playerPosition)
    {
        // 視界範囲の境界を計算
        int minX = playerPosition.x - visionRange;
        int maxX = playerPosition.x + visionRange;
        int minY = playerPosition.y - visionRange;
        int maxY = playerPosition.y + visionRange;
        
        Debug.Log($"視界範囲境界: X({minX} to {maxX}), Y({minY} to {maxY})");
        
        // プレイヤーを中心とした視界範囲の確認
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (IsInVisionRange(pos, playerPosition))
                {
                    // 視界範囲内の位置をログに表示（デバッグ用）
                    if (x == minX || x == maxX || y == minY || y == maxY)
                    {
                        Debug.Log($"視界境界: {pos}");
                    }
                }
            }
        }
    }

    // Exitの位置を取得
    private Vector2Int GetExitPosition()
    {
        return exitPosition;
    }

    // 全オブジェクトの参照を保存
    public void StoreAllObjects()
    {
        allTiles = FindObjectsOfType<Tile>();
        allEnemies = FindObjectsOfType<Enemy>();
        // exitObjectはSpawnExitで既に設定済み
    }
} 