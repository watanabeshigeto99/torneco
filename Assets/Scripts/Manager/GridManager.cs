using UnityEngine;
using System;

[DefaultExecutionOrder(-90)]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    // イベント定義
    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Enemy[]> OnEnemiesSpawned;
    public static event Action<GameObject> OnExitSpawned;
    public static event Action OnGridInitialized;
    public static event Action OnAllObjectsInitialized; // 全オブジェクト初期化完了イベント

    [Header("Grid Settings")]
    public int width = 10;
    public int height = 10;
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
    public Vector2Int exitPosition; // Exitの位置を記録（publicに変更）

    // 初期化完了フラグ
    private bool isGridGenerated = false;
    private bool isPlayerSpawned = false;
    private bool isExitSpawned = false;
    private bool isEnemiesSpawned = false;
    private bool isAllObjectsStored = false;

    private void Awake()
    {
        // Singletonパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        // 基本的な初期化処理
        Debug.Log("GridManager: 初期化開始");
        
        if (tilePrefab == null)
        {
            Debug.LogError("GridManager: tilePrefabが設定されていません");
            return;
        }
        
        if (playerPrefab == null)
        {
            Debug.LogError("GridManager: playerPrefabが設定されていません");
            return;
        }
        
        if (exitPrefab == null)
        {
            Debug.LogError("GridManager: exitPrefabが設定されていません");
            return;
        }
        
        // 段階的に初期化を実行
        StartCoroutine(InitializeGameCoroutine());
    }
    
    private System.Collections.IEnumerator InitializeGameCoroutine()
    {
        Debug.Log("GridManager: 段階的初期化開始");
        
        // 1. グリッド生成
        GenerateGrid();
        isGridGenerated = true;
        Debug.Log("GridManager: グリッド生成完了");
        yield return null; // 1フレーム待機
        
        // 2. オブジェクト参照保存
        StoreAllObjects();
        isAllObjectsStored = true;
        Debug.Log("GridManager: オブジェクト参照保存完了");
        yield return null; // 1フレーム待機
        
        // 3. プレイヤー生成
        SpawnPlayer(new Vector2Int(width / 2, height / 2));
        isPlayerSpawned = true;
        Debug.Log("GridManager: プレイヤー生成完了");
        yield return null; // 1フレーム待機
        
        // 4. Exit生成
        SpawnExit();
        isExitSpawned = true;
        Debug.Log("GridManager: Exit生成完了");
        yield return null; // 1フレーム待機
        
        // 5. 敵生成
        SpawnEnemies();
        isEnemiesSpawned = true;
        Debug.Log("GridManager: 敵生成完了");
        yield return null; // 1フレーム待機
        
        // 6. 全オブジェクト初期化完了チェック
        yield return StartCoroutine(WaitForAllObjectsInitialized());
        
        // 7. 最終的な初期化処理
        if (Player.Instance != null)
        {
            UpdateTileVisibility(Player.Instance.gridPosition);
            Debug.Log("GridManager: 初期化完了");
            
            // グリッド初期化完了イベントを発行
            OnGridInitialized?.Invoke();
            
            // 全オブジェクト初期化完了イベントを発行
            OnAllObjectsInitialized?.Invoke();
            Debug.Log("GridManager: 全オブジェクト初期化完了イベント発行");
        }
        else
        {
            Debug.LogError("GridManager: Player.Instanceが見つかりません");
        }
    }
    
    private System.Collections.IEnumerator WaitForAllObjectsInitialized()
    {
        Debug.Log("GridManager: 全オブジェクト初期化完了待機開始");
        
        // 全ての初期化フラグがtrueになるまで待機
        while (!isGridGenerated || !isPlayerSpawned || !isExitSpawned || !isEnemiesSpawned || !isAllObjectsStored)
        {
            Debug.Log($"GridManager: 初期化状態 - グリッド: {isGridGenerated}, プレイヤー: {isPlayerSpawned}, Exit: {isExitSpawned}, 敵: {isEnemiesSpawned}, オブジェクト保存: {isAllObjectsStored}");
            yield return new WaitForSeconds(0.1f); // 0.1秒待機
        }
        
        Debug.Log("GridManager: 全オブジェクト初期化完了");
    }
    

    
    private void GenerateGrid()
    {
        Debug.Log($"GridManager: グリッド生成開始 ({width}x{height})");
        
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                GameObject tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";

                Tile tileScript = tile.GetComponent<Tile>();
                if (tileScript == null)
                {
                    Debug.LogError($"GridManager: Tile_{x}_{y}にTileコンポーネントが見つかりません");
                    continue;
                }
                
                tileScript.Initialize(x, y);
            }
        }
        
        Debug.Log("GridManager: グリッド生成完了");
    }
    
    private void SpawnPlayer(Vector2Int position)
    {
        Debug.Log($"GridManager: プレイヤー生成開始 位置: {position}");
        
        Vector3 pos = new Vector3(position.x * tileSpacing, position.y * tileSpacing, 0);
        GameObject playerObj = Instantiate(playerPrefab, pos, Quaternion.identity);
        
        Player playerScript = playerObj.GetComponent<Player>();
        if (playerScript == null)
        {
            Debug.LogError("GridManager: プレイヤーオブジェクトにPlayerコンポーネントが見つかりません");
            return;
        }
        
        playerScript.InitializePosition();
        
        // プレイヤー生成完了イベントを発行
        OnPlayerSpawned?.Invoke(playerScript);
        Debug.Log("GridManager: プレイヤー生成完了イベント発行");
        
        // カメラ追従を開始
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.OnPlayerMoved(playerObj.transform.position);
            Debug.Log("GridManager: カメラ追従開始");
        }
        else
        {
            Debug.LogWarning("GridManager: CameraFollow.Instanceが見つかりません");
        }
        
        Debug.Log("GridManager: プレイヤー生成完了");
    }
    
    private void SpawnExit()
    {
        Debug.Log($"GridManager: Exit生成開始 - グリッドサイズ: {width}x{height}");
        
        Vector2Int exitPos;
        do
        {
            exitPos = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
        }
        while (exitPos == new Vector2Int(width / 2, height / 2) || 
               (Player.Instance != null && exitPos == Player.Instance.gridPosition)); // プレイヤー位置と被らない

        Vector3 pos = new Vector3(exitPos.x * tileSpacing, exitPos.y * tileSpacing, 0);
        GameObject exitObj = Instantiate(exitPrefab, pos, Quaternion.identity);
        
        // Exit位置を記録
        exitObject = exitObj;
        exitPosition = exitPos;
        
        // Exit生成完了イベントを発行
        OnExitSpawned?.Invoke(exitObj);
        Debug.Log("GridManager: Exit生成完了イベント発行");
        
        Debug.Log($"GridManager: Exit生成完了 位置: {exitPos}, グリッド内: {IsInsideGrid(exitPos)}");
    }
    
    // 新しい階層を生成
    public void GenerateNewFloor()
    {
        Debug.Log("GridManager: 新しい階層を生成開始");
        
        // 古いタイルを消去
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        // 古いExitを消去
        if (exitObject != null)
        {
            Destroy(exitObject);
            exitObject = null;
        }
        
        // グリッドを再生成
        GenerateGrid();
        
        // Exitを再生成
        SpawnExit();
        
        // プレイヤー位置をリセット
        if (Player.Instance != null)
        {
            Player.Instance.ResetPlayerPosition();
        }
        
        Debug.Log("GridManager: 新しい階層の生成完了");
    }
    
    private void SpawnEnemies()
    {
        Debug.Log("GridManager: 敵スポーン開始");
        
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.SpawnEnemies();
            
            // 敵生成後にallEnemiesを更新
            allEnemies = FindObjectsOfType<Enemy>();
            
            // 敵生成完了イベントを発行
            if (allEnemies != null)
            {
                OnEnemiesSpawned?.Invoke(allEnemies);
                Debug.Log("GridManager: 敵生成完了イベント発行");
            }
            
            Debug.Log("GridManager: 敵スポーン完了");
        }
        else
        {
            Debug.LogError("GridManager: EnemyManager.Instanceが見つかりません");
        }
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
        // Exit位置は常に歩行可能
        if (exitPosition == pos)
            return false;

        // プレイヤーの位置チェック
        if (Player.Instance != null && Player.Instance.gridPosition == pos)
            return true;

        // 敵の位置チェック（キャッシュされたallEnemiesを使用）
        if (allEnemies != null)
        {
            foreach (var enemy in allEnemies)
            {
                if (enemy != null && enemy.gridPosition == pos)
                    return true;
            }
        }

        return false;
    }

    public bool IsWalkable(Vector2Int pos)
    {
        bool insideGrid = IsInsideGrid(pos);
        bool occupied = IsOccupied(pos);
        bool walkable = insideGrid && !occupied;
        
        // Exit位置の場合は特別なデバッグログ
        if (pos == exitPosition)
        {
            Debug.Log($"GridManager: Exit位置チェック - 位置: {pos}, グリッドサイズ: {width}x{height}, グリッド内: {insideGrid}, 占有: {occupied}, 歩行可能: {walkable}");
        }
        
        return walkable;
    }

    // 視界範囲内かどうかをチェック
    public bool IsInVisionRange(Vector2Int position, Vector2Int playerPosition)
    {
        int distance = Mathf.Max(Mathf.Abs(position.x - playerPosition.x), Mathf.Abs(position.y - playerPosition.y));
        return distance <= visionRange;
    }

    // 全オブジェクトの参照を保存
    public void StoreAllObjects()
    {
        Debug.Log("GridManager: オブジェクト参照保存開始");
        
        allTiles = FindObjectsOfType<Tile>();
        // 敵はまだ生成されていないので、allEnemiesは後で設定
        allEnemies = new Enemy[0]; // 空配列で初期化
        
        Debug.Log($"GridManager: タイル数: {allTiles?.Length ?? 0}, 敵数: {allEnemies?.Length ?? 0}");
        
        // exitObjectはSpawnExitで既に設定済み
        if (exitObject == null)
        {
            Debug.LogWarning("GridManager: exitObjectが設定されていません");
        }
        
        Debug.Log("GridManager: オブジェクト参照保存完了");
    }
    
    // 新しいタイル表示制御システム
    public void UpdateTileVisibility(Vector2Int playerPosition)
    {
        Debug.Log($"GridManager: 視界範囲更新開始 プレイヤー位置: {playerPosition}");
        
        if (allTiles == null)
        {
            Debug.LogError("GridManager: allTilesがnullです");
            return;
        }
        
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
                    
                    // 透明度も更新
                    if (shouldBeVisible)
                    {
                        float alpha = (visibility == Tile.VisibilityState.Transparent) ? 0.4f : 1.0f;
                        UpdateSpriteTransparency(enemy.gameObject, alpha);
                    }
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
                    
                    // 透明度も更新
                    if (shouldBeVisible)
                    {
                        float alpha = (visibility == Tile.VisibilityState.Transparent) ? 0.4f : 1.0f;
                        UpdateSpriteTransparency(enemy.gameObject, alpha);
                    }
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

    // 移動可能なタイルをハイライト表示
    public void HighlightMovableTiles(Vector2Int center, int range)
    {
        Debug.Log($"GridManager: 移動可能タイルハイライト開始 中心: {center}, 範囲: {range}");
        
        int highlightedCount = 0;
        
        // キャッシュされたallTilesを使用
        if (allTiles == null || allTiles.Length == 0)
        {
            Debug.LogWarning("GridManager: タイルが見つかりません");
            return;
        }
        
        foreach (var tile in allTiles)
        {
            if (tile == null) continue;
            
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
        
        Debug.Log($"GridManager: 移動可能タイルハイライト完了 ハイライト数: {highlightedCount}");
    }
    
    // 攻撃可能タイルをハイライト表示
    public void HighlightAttackableTiles(Vector2Int center)
    {
        Debug.Log($"GridManager: 攻撃可能タイルハイライト開始 中心: {center}");
        
        int highlightedCount = 0;
        
        // キャッシュされたallTilesを使用
        if (allTiles == null || allTiles.Length == 0)
        {
            Debug.LogWarning("GridManager: タイルが見つかりません");
            return;
        }
        
        // 攻撃範囲（隣接マス + 斜め）を定義
        Vector2Int[] attackDirections = new Vector2Int[]
        {
            Vector2Int.up,
            Vector2Int.down,
            Vector2Int.left,
            Vector2Int.right,
            new Vector2Int(1, 1),   // 右上
            new Vector2Int(-1, 1),  // 左上
            new Vector2Int(1, -1),  // 右下
            new Vector2Int(-1, -1)  // 左下
        };
        
        foreach (var tile in allTiles)
        {
            if (tile == null) continue;
            
            // 攻撃範囲内かチェック
            bool isInAttackRange = false;
            foreach (Vector2Int direction in attackDirections)
            {
                Vector2Int attackPos = center + direction;
                if (tile.x == attackPos.x && tile.y == attackPos.y)
                {
                    isInAttackRange = true;
                    break;
                }
            }
            
            if (isInAttackRange)
            {
                // 攻撃可能なタイルは必ず表示する（非表示でも表示に変更）
                tile.gameObject.SetActive(true);
                tile.HighlightAttackable();
                highlightedCount++;
            }
            else
            {
                tile.ResetColor();
            }
        }
        
        Debug.Log($"GridManager: 攻撃可能タイルハイライト完了 ハイライト数: {highlightedCount}");
    }
    
    // 全てのタイルの色をリセット
    public void ResetAllTileColors()
    {
        Debug.Log("GridManager: タイル色リセット開始");
        
        // キャッシュされたallTilesを使用
        if (allTiles == null || allTiles.Length == 0)
        {
            Debug.LogWarning("GridManager: リセット対象のタイルが見つかりません");
            return;
        }
        
        foreach (var tile in allTiles)
        {
            if (tile != null)
            {
                tile.ResetColor();
            }
        }
        
        // 色をリセットした後に視界範囲を更新
        if (Player.Instance != null)
        {
            UpdateTileVisibility(Player.Instance.gridPosition);
        }
        else
        {
            Debug.LogWarning("GridManager: Player.Instanceが見つからないため視界範囲更新をスキップ");
        }
        
        Debug.Log("GridManager: タイル色リセット完了");
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