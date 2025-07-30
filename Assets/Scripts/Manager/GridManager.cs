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
    public bool showVisionRange = true; // 視界範囲を視覚的に表示
    
    [Header("Tile Visibility Settings")]
    [Tooltip("通常表示範囲（プレイヤー周囲のマス数）")]
    public int normalVisibilityRange = 1; // プレイヤー周囲1マス = 周囲8マス
    
    [Tooltip("半透明表示範囲（通常表示範囲の外側のマス数）")]
    public int transparentVisibilityRange = 2; // 通常表示範囲の外側2マス = さらに16マス
    
    private Tile[] allTiles;
    private Enemy[] allEnemies;
    private GameObject exitObject;
    private Vector2Int exitPosition; // Exitの位置を記録

    private void Awake()
    {
        // Singletonパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // 基本的な変数の初期化
        allTiles = null;
        allEnemies = null;
        exitObject = null;
    }

    private void Start()
    {
        // 実際の初期化処理を開始
        StartCoroutine(InitializeGame());
    }
    
    private System.Collections.IEnumerator InitializeGame()
    {
        // 1. グリッド生成
        GenerateGrid();
        
        // 2. プレイヤー生成
        SpawnPlayer(new Vector2Int(width / 2, height / 2));
        
        // 3. Exit生成
        SpawnExit();
        
        // 4. 他のオブジェクトの初期化完了を待つ
        yield return StartCoroutine(WaitForOtherManagers());
        
        // 5. 敵のスポーン
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.SpawnEnemies();
        }
        
        // 6. 全オブジェクトの参照を保存
        yield return new WaitForSeconds(0.1f);
        StoreAllObjects();
        
        // 7. 初期視界範囲を設定
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            UpdateTileVisibility(player.gridPosition);
        }
    }
    
    private System.Collections.IEnumerator WaitForOtherManagers()
    {
        // 他のManagerの初期化完了を待つ
        yield return new WaitForEndOfFrame();
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

    // 新しいタイル表示制御システム
    public void UpdateTileVisibility(Vector2Int playerPosition)
    {
        if (allTiles != null)
        {
            foreach (Tile tile in allTiles)
            {
                if (tile != null)
                {
                    Vector2Int tilePos = new Vector2Int(tile.x, tile.y);
                    Tile.VisibilityState visibility = GetTileVisibilityState(tilePos, playerPosition);
                    tile.UpdateVisibility(visibility);
                    
                    // そのマス上のオブジェクトの透明度を同期
                    UpdateObjectTransparencyAt(tilePos, tile.CurrentAlpha);
                }
            }
        }

        // 敵の表示/非表示を更新（新しい表示制御システムに合わせる）
        if (allEnemies != null)
        {
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null)
                {
                    Vector2Int enemyPos = enemy.gridPosition;
                    Tile.VisibilityState visibility = GetTileVisibilityState(enemyPos, playerPosition);
                    
                    // 非表示の場合のみSetActive(false)、それ以外は表示
                    bool shouldBeVisible = (visibility != Tile.VisibilityState.Hidden);
                    enemy.gameObject.SetActive(shouldBeVisible);
                }
            }
        }
        else
        {
            // allEnemiesがnullの場合は、FindObjectsOfTypeで再取得
            Enemy[] currentEnemies = FindObjectsOfType<Enemy>();
            foreach (Enemy enemy in currentEnemies)
            {
                if (enemy != null)
                {
                    Vector2Int enemyPos = enemy.gridPosition;
                    Tile.VisibilityState visibility = GetTileVisibilityState(enemyPos, playerPosition);
                    
                    // 非表示の場合のみSetActive(false)、それ以外は表示
                    bool shouldBeVisible = (visibility != Tile.VisibilityState.Hidden);
                    enemy.gameObject.SetActive(shouldBeVisible);
                }
            }
        }

        // Exitの表示/非表示を更新（新しい表示制御システムに合わせる）
        if (exitObject != null)
        {
            Vector2Int exitPos = GetExitPosition();
            Tile.VisibilityState visibility = GetTileVisibilityState(exitPos, playerPosition);
            
            // 非表示の場合のみSetActive(false)、それ以外は表示
            bool shouldBeVisible = (visibility != Tile.VisibilityState.Hidden);
            exitObject.SetActive(shouldBeVisible);
        }
        
        // 視界範囲を視覚的に表示（デバッグ用）
        if (showVisionRange)
        {
            ShowVisionRangeDebug(playerPosition);
        }
    }
    
    // タイルの表示状態を判定
    private Tile.VisibilityState GetTileVisibilityState(Vector2Int tilePos, Vector2Int playerPos)
    {
        int distance = Mathf.Max(Mathf.Abs(tilePos.x - playerPos.x), Mathf.Abs(tilePos.y - playerPos.y));
        
        if (distance <= normalVisibilityRange)
        {
            return Tile.VisibilityState.Visible; // 通常表示
        }
        else if (distance <= normalVisibilityRange + transparentVisibilityRange)
        {
            return Tile.VisibilityState.Transparent; // 半透明表示
        }
        else
        {
            return Tile.VisibilityState.Hidden; // 非表示
        }
    }

    // 視界範囲を更新（既存メソッド - 後方互換性のため保持）
    public void UpdateVisionRange(Vector2Int playerPosition)
    {
        // 新しい表示制御システムを使用
        UpdateTileVisibility(playerPosition);
    }
    
    // 視界範囲を視覚的に表示（デバッグ用）
    private void ShowVisionRangeDebug(Vector2Int playerPosition)
    {
        // 視界範囲の境界を計算
        int minX = playerPosition.x - visionRange;
        int maxX = playerPosition.x + visionRange;
        int minY = playerPosition.y - visionRange;
        int maxY = playerPosition.y + visionRange;
        
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
    
    // 移動可能なタイルをハイライト表示
    public void HighlightMovableTiles(Vector2Int center, int range)
    {
        int highlightedCount = 0;
        
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            int dist = Mathf.Abs(tile.x - center.x) + Mathf.Abs(tile.y - center.y);
            if (dist <= range)
            {
                // 移動可能なタイルは必ず表示する（非表示でも表示に変更）
                tile.gameObject.SetActive(true);
                tile.Highlight();
                highlightedCount++;
            }
            else
            {
                tile.ResetColor();
            }
        }
    }
    
    // 全てのタイルの色をリセット
    public void ResetAllTileColors()
    {
        foreach (var tile in FindObjectsOfType<Tile>())
        {
            tile.ResetColor();
        }
        
        // 色をリセットした後に視界範囲を更新
        Player player = FindObjectOfType<Player>();
        if (player != null)
        {
            UpdateTileVisibility(player.gridPosition);
        }
    }

    // 指定されたマス上のオブジェクトの透明度を更新
    private void UpdateObjectTransparencyAt(Vector2Int position, float alpha)
    {
        // 敵の透明度を更新
        if (allEnemies != null)
        {
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null && enemy.gridPosition == position)
                {
                    UpdateSpriteTransparency(enemy.gameObject, alpha);
                }
            }
        }
        
        // Exitの透明度を更新
        if (exitObject != null)
        {
            Vector2Int exitPos = GetExitPosition();
            if (exitPos == position)
            {
                UpdateSpriteTransparency(exitObject, alpha);
            }
        }
    }
    
    // 指定されたGameObjectのSpriteRendererの透明度を更新
    private void UpdateSpriteTransparency(GameObject obj, float alpha)
    {
        if (obj == null) return;
        
        SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
} 