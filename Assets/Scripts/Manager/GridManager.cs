using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[DefaultExecutionOrder(-90)]
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }
    
    public static GridManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("GridManager");
            Instance = go.AddComponent<GridManager>();
        }
        return Instance;
    }

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
    
    // ミニマップ用のTile管理Dict
    public Dictionary<Vector2Int, Tile> tileDict = new();

    // 初期化完了フラグ
    private bool isGridGenerated = false;
    private bool isPlayerSpawned = false;
    private bool isExitSpawned = false;
    private bool isEnemiesSpawned = false;
    private bool isAllObjectsStored = false;

    private void Awake()
    {
        Debug.Log("GridManager: Awake開始");
        
        // Singletonパターンの実装
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GridManager: 重複するGridManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化（シーン間で保持）
        DontDestroyOnLoad(gameObject);
        
        // ミニマップ用のTile管理Dictを初期化
        if (tileDict == null)
        {
            tileDict = new Dictionary<Vector2Int, Tile>();
        }
        
        Debug.Log("GridManager: Awake完了");
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
        
        // 従来の順次実行方式を使用（AsyncOperationManagerは問題を引き起こすため）
        // 1. グリッド生成
        yield return StartCoroutine(GenerateGridCoroutine());
        isGridGenerated = true;
        Debug.Log("GridManager: グリッド生成完了");
        
        // 2. オブジェクト参照保存
        yield return StartCoroutine(StoreAllObjectsCoroutine());
        isAllObjectsStored = true;
        Debug.Log("GridManager: オブジェクト参照保存完了");
        
        // 3. プレイヤー生成
        yield return StartCoroutine(SpawnPlayerCoroutine(new Vector2Int(width / 2, height / 2)));
        isPlayerSpawned = true;
        Debug.Log("GridManager: プレイヤー生成完了");
        
        // 4. Exit生成
        yield return StartCoroutine(SpawnExitCoroutine());
        isExitSpawned = true;
        Debug.Log("GridManager: Exit生成完了");
        
        // 5. 敵生成
        yield return StartCoroutine(SpawnEnemiesCoroutine());
        isEnemiesSpawned = true;
        Debug.Log("GridManager: 敵生成完了");
        
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
                Vector2Int pos = new(x, y);
                Vector3 worldPos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                GameObject tile = Instantiate(tilePrefab, worldPos, Quaternion.identity, transform);
                tile.name = $"Tile_{x}_{y}";

                Tile tileScript = tile.GetComponent<Tile>();
                if (tileScript == null)
                {
                    Debug.LogError($"GridManager: Tile_{x}_{y}にTileコンポーネントが見つかりません");
                    continue;
                }
                
                tileScript.Initialize(x, y);
                if (tileDict == null)
                {
                    Debug.LogWarning("GridManager: tileDictがnullです。新しく初期化します。");
                    tileDict = new Dictionary<Vector2Int, Tile>();
                }
                tileDict[pos] = tileScript; // ミニマップ用にTile管理Dictに登録
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
         int attempts = 0;
         do
         {
             exitPos = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
             attempts++;
             
             // プレイヤー位置と被らないかチェック
             bool isPlayerPosition = (Player.Instance != null && exitPos == Player.Instance.gridPosition);
             bool isCenterPosition = (exitPos == new Vector2Int(width / 2, height / 2));
             
             // グリッド範囲内かチェック（UnityEngine.Random.Range(0, width)は0からwidth-1を返すので、実際には常にグリッド内）
             if (!IsInsideGrid(exitPos))
             {
                 Debug.LogWarning($"GridManager: Exit位置がグリッド外 - {exitPos}, グリッドサイズ: {width}x{height}");
                 continue;
             }
             
             // プレイヤー位置や中心位置と被らない場合はループを抜ける
             if (!isPlayerPosition && !isCenterPosition)
             {
                 break;
             }
         }
         while (attempts < 100); // 最大試行回数を制限
         
         // 最終的な位置を確認
         if (!IsInsideGrid(exitPos))
         {
             Debug.LogError($"GridManager: Exit位置がグリッド外です - {exitPos}, グリッドサイズ: {width}x{height}");
             // フォールバック: グリッド内の安全な位置を使用
             exitPos = new Vector2Int(width - 1, height - 1);
         }

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

        // 修正: 敵の位置チェックを改善（EnemyManagerから最新の敵リストを取得）
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemies();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gridPosition == pos)
                    return true;
            }
        }

        // フォールバック: キャッシュされたallEnemiesを使用
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
    
    /// <summary>
    /// Exit位置かどうかをチェック
    /// </summary>
    public bool IsExitPosition(Vector2Int pos)
    {
        return pos == exitPosition;
    }
    
    /// <summary>
    /// Exit位置への移動が可能かどうかをチェック
    /// </summary>
    public bool CanMoveToExit(Vector2Int pos)
    {
        return IsExitPosition(pos) && IsInsideGrid(pos);
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
        
        // タイル配列が既に設定されている場合は確認のみ
        if (allTiles == null)
        {
            allTiles = FindObjectsOfType<Tile>();
        }
        
        // 敵の参照も保存（既に生成されている場合があるため）
        UpdateEnemiesCache();
        
        // 有効なタイル数をカウント
        int validTileCount = 0;
        if (allTiles != null)
        {
            foreach (var tile in allTiles)
            {
                if (tile != null) validTileCount++;
            }
        }
        
        Debug.Log($"GridManager: タイル数: {validTileCount}/{allTiles?.Length ?? 0}, 敵数: {allEnemies?.Length ?? 0}");
        
        // exitObjectはSpawnExitで既に設定済み
        if (exitObject == null)
        {
            Debug.LogWarning("GridManager: exitObjectが設定されていません");
        }
        
        Debug.Log("GridManager: オブジェクト参照保存完了");
    }
    
    // 修正: 敵キャッシュを更新するメソッドを追加
    public void UpdateEnemiesCache()
    {
        allEnemies = FindObjectsOfType<Enemy>();
        Debug.Log($"GridManager: 敵キャッシュ更新 - {allEnemies?.Length ?? 0}体の敵を検出");
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
            Debug.Log($"GridManager: 敵の透明度更新開始 - 敵数: {allEnemies.Length}");
            foreach (Enemy enemy in allEnemies)
            {
                if (enemy != null)
                {
                    Vector2Int enemyPos = enemy.gridPosition;
                    Tile.VisibilityState visibility = GetTileVisibilityState(enemyPos, playerPosition);
                    
                    // 非表示の場合のみSetActive(false)、それ以外は表示
                    bool shouldBeVisible = (visibility != Tile.VisibilityState.Hidden);
                    enemy.gameObject.SetActive(shouldBeVisible);
                    
                    // 透明度も更新（タイルの透明度に依存せず、直接計算）
                    if (shouldBeVisible)
                    {
                        float alpha = (visibility == Tile.VisibilityState.Transparent) ? 0.4f : 1.0f;
                        UpdateSpriteTransparency(enemy.gameObject, alpha);
                        Debug.Log($"GridManager: 敵透明度更新 - 位置: {enemyPos}, 状態: {visibility}, 透明度: {alpha}");
                    }
                }
            }
        }
        else
        {
            // allEnemiesがnullの場合は、FindObjectsOfTypeで再取得
            Enemy[] currentEnemies = FindObjectsOfType<Enemy>();
            Debug.Log($"GridManager: allEnemiesがnull - 再取得した敵数: {currentEnemies.Length}");
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
                        Debug.Log($"GridManager: 敵透明度更新（再取得） - 位置: {enemyPos}, 状態: {visibility}, 透明度: {alpha}");
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
        Debug.Log($"GridManager: 移動可能タイルハイライト開始 - 中心: {center}, 範囲: {range}");
        
        if (allTiles == null)
        {
            Debug.LogError("GridManager: allTilesがnullです");
            return;
        }
        
        foreach (Tile tile in allTiles)
        {
            if (tile != null)
            {
                Vector2Int tilePos = new Vector2Int(tile.x, tile.y);
                int distance = Mathf.Abs(tilePos.x - center.x) + Mathf.Abs(tilePos.y - center.y);
                
                // 移動可能範囲内かチェック
                bool inRange = distance <= range;
                bool walkable = IsWalkable(tilePos);
                bool isExitPosition = IsExitPosition(tilePos);
                
                // Exit位置の場合は特別にハイライト
                if (isExitPosition)
                {
                    tile.Highlight(Color.green); // Exit位置は緑色でハイライト
                    Debug.Log($"GridManager: Exit位置をハイライト - 位置: {tilePos}");
                }
                else if (inRange && walkable)
                {
                    tile.Highlight(Color.blue);
                }
                else
                {
                    tile.ResetColor();
                }
            }
        }
        
        Debug.Log("GridManager: 移動可能タイルハイライト完了");
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
        // 敵の透明度はUpdateTileVisibility内で直接計算するため、ここでは更新しない
        // （タイルの透明度に依存すると、敵の透明度が正しく設定されない可能性がある）
        
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
        
        // 敵の場合は直接spriteRendererフィールドを使用
        Enemy enemy = obj.GetComponent<Enemy>();
        if (enemy != null && enemy.spriteRenderer != null)
        {
            Color color = enemy.spriteRenderer.color;
            color.a = alpha;
            enemy.spriteRenderer.color = color;
            Debug.Log($"GridManager: 敵SpriteRenderer透明度更新 - オブジェクト: {obj.name}, 透明度: {alpha}");
            return;
        }
        
        // その他のオブジェクトの場合はGetComponentInChildrenを使用
        SpriteRenderer spriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
            Debug.Log($"GridManager: SpriteRenderer透明度更新 - オブジェクト: {obj.name}, 透明度: {alpha}");
        }
        else
        {
            Debug.LogWarning($"GridManager: SpriteRendererが見つかりません - オブジェクト: {obj.name}");
        }
    }

    // 新しい階層を生成（段階1実装）
    public void GenerateNewFloor()
    {
        Debug.Log("GridManager: 新しい階層の生成を開始");
        
        // 初期化フラグをリセット
        ResetInitializationFlags();
        
        // 段階的に初期化を実行
        StartCoroutine(InitializeFloorCoroutine());
    }
    
    // 初期化フラグをリセット
    private void ResetInitializationFlags()
    {
        isGridGenerated = false;
        isPlayerSpawned = false;
        isExitSpawned = false;
        isEnemiesSpawned = false;
        isAllObjectsStored = false;
    }
    
    private IEnumerator InitializeFloorCoroutine()
    {
        Debug.Log("GridManager: 段階的初期化開始");
        
        // 1. 古いオブジェクトを削除
        ClearOldObjects();
        yield return null;
        
        // 2. グリッド生成
        GenerateGrid();
        isGridGenerated = true;
        Debug.Log("GridManager: グリッド生成完了");
        yield return new WaitForSeconds(0.1f);
        
        // 3. オブジェクト参照保存
        StoreAllObjects();
        isAllObjectsStored = true;
        Debug.Log("GridManager: オブジェクト参照保存完了");
        yield return new WaitForSeconds(0.1f);
        
        // 4. プレイヤー生成
        SpawnPlayer(new Vector2Int(width / 2, height / 2));
        isPlayerSpawned = true;
        Debug.Log("GridManager: プレイヤー生成完了");
        yield return new WaitForSeconds(0.1f);
        
        // 5. Exit生成
        SpawnExit();
        isExitSpawned = true;
        Debug.Log("GridManager: Exit生成完了");
        yield return new WaitForSeconds(0.1f);
        
        // 6. 敵生成
        SpawnEnemies();
        isEnemiesSpawned = true;
        Debug.Log("GridManager: 敵生成完了");
        yield return new WaitForSeconds(0.1f);
        
        // 7. 全オブジェクト初期化完了チェック
        yield return StartCoroutine(WaitForAllObjectsInitialized());
        
        // 8. 最終的な初期化処理
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
    
    // 古いオブジェクトを削除
    private void ClearOldObjects()
    {
        Debug.Log("GridManager: 古いオブジェクトを削除中");
        
        // 古いタイルを削除
        if (allTiles != null)
        {
            foreach (Tile tile in allTiles)
            {
                if (tile != null)
                {
                    DestroyImmediate(tile.gameObject);
                }
            }
        }
        
        // 古いExitを削除（複数存在する可能性があるため、すべてのExitを検索して削除）
        GameObject[] allGameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allGameObjects)
        {
            if (obj != null && obj.name.Contains("Exit"))
            {
                DestroyImmediate(obj);
                Debug.Log("GridManager: 古いExitを削除しました");
            }
        }
        
        // 現在のExitオブジェクトも削除
        if (exitObject != null)
        {
            DestroyImmediate(exitObject);
            exitObject = null;
        }
        
        // 配列をクリア
        allTiles = null;
        allEnemies = null;
        
        // ミニマップ用のTile管理Dictもクリア
        if (tileDict != null)
        {
            tileDict.Clear();
        }
        
        Debug.Log("GridManager: 古いオブジェクトの削除完了");
    }
    
    /// <summary>
    /// シーン遷移時のクリーンアップ処理
    /// </summary>
    public void CleanupForSceneTransition()
    {
        Debug.Log("GridManager: シーン遷移時のクリーンアップ開始");
        
        // 初期化フラグをリセット
        ResetInitializationFlags();
        
        // 古いオブジェクトを削除
        ClearOldObjects();
        
        Debug.Log("GridManager: シーン遷移時のクリーンアップ完了");
    }
    
    /// <summary>
    /// メインシーン用の初期化処理
    /// </summary>
    public void InitializeForMainScene()
    {
        Debug.Log("GridManager: メインシーン初期化開始");
        
        // 初期化フラグをリセット
        ResetInitializationFlags();
        
        // 段階的初期化を開始
        StartCoroutine(InitializeFloorCoroutine());
        
        Debug.Log("GridManager: メインシーン初期化完了");
    }

    /// <summary>
    /// グリッド生成コルーチン
    /// </summary>
    private System.Collections.IEnumerator GenerateGridCoroutine()
    {
        Debug.Log($"GridManager: グリッド生成開始 ({width}x{height})");
        
        // タイル配列を事前に初期化
        allTiles = new Tile[width * height];
        
        // ミニマップ用のTile管理Dictを初期化
        if (tileDict == null)
        {
            tileDict = new Dictionary<Vector2Int, Tile>();
        }
        else
        {
            tileDict.Clear();
        }
        
        int tileIndex = 0;
        
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
                
                // 配列に直接保存
                allTiles[tileIndex] = tileScript;
                
                // ミニマップ用にTile管理Dictに登録
                Vector2Int gridPos = new Vector2Int(x, y);
                if (tileDict == null)
                {
                    Debug.LogWarning("GridManager: tileDictがnullです。新しく初期化します。");
                    tileDict = new Dictionary<Vector2Int, Tile>();
                }
                tileDict[gridPos] = tileScript;
                
                tileIndex++;
            }
            
            // フレーム分割（より頻繁に）
            if (x % 2 == 0)
                yield return null;
        }
        
        Debug.Log($"GridManager: グリッド生成完了 - {tileIndex}タイル生成、配列サイズ: {allTiles.Length}");
    }
    
    /// <summary>
    /// オブジェクト参照保存コルーチン
    /// </summary>
    private System.Collections.IEnumerator StoreAllObjectsCoroutine()
    {
        Debug.Log("GridManager: オブジェクト参照保存開始");
        
        // プレイヤー参照を保存
        var existingPlayer = FindObjectOfType<Player>();
        if (existingPlayer == null)
        {
            Debug.LogError("GridManager: Playerが見つかりません");
        }
        
        // 敵参照を保存
        allEnemies = FindObjectsOfType<Enemy>();
        Debug.Log($"GridManager: {allEnemies.Length}体の敵を検出");
        
        // Exit参照を保存（既に生成されている場合）
        if (exitObject == null)
        {
            exitObject = GameObject.FindGameObjectWithTag("Exit");
            if (exitObject != null)
            {
                exitPosition = Vector2Int.RoundToInt(exitObject.transform.position);
            }
        }
        
        // タイル配列の確認
        if (allTiles != null)
        {
            int validTileCount = 0;
            foreach (var tile in allTiles)
            {
                if (tile != null) validTileCount++;
            }
            Debug.Log($"GridManager: 有効なタイル数: {validTileCount}/{allTiles.Length}");
        }
        else
        {
            Debug.LogWarning("GridManager: allTilesがnullです");
        }
        
        yield return null;
        Debug.Log("GridManager: オブジェクト参照保存完了");
    }
    
    /// <summary>
    /// プレイヤー生成コルーチン
    /// </summary>
    private System.Collections.IEnumerator SpawnPlayerCoroutine(Vector2Int position)
    {
        Debug.Log($"GridManager: プレイヤー生成開始 - 位置: {position}");
        
        if (Player.Instance == null)
        {
            Vector3 worldPos = GetWorldPosition(position);
            GameObject playerObj = Instantiate(playerPrefab, worldPos, Quaternion.identity);
            var playerComponent = playerObj.GetComponent<Player>();
            
            if (playerComponent != null)
            {
                playerComponent.InitializePosition();
                OnPlayerSpawned?.Invoke(playerComponent);
                Debug.Log("GridManager: プレイヤー生成完了");
                
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
            }
            else
            {
                Debug.LogError("GridManager: プレイヤーコンポーネントが見つかりません");
            }
        }
        else
        {
            Player.Instance.Initialize(position);
            OnPlayerSpawned?.Invoke(Player.Instance);
            Debug.Log("GridManager: 既存プレイヤーを初期化");
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Exit生成コルーチン
    /// </summary>
    private System.Collections.IEnumerator SpawnExitCoroutine()
    {
        Debug.Log("GridManager: Exit生成開始");
        
        // 新しいExit位置を生成（グリッド範囲内に制限）
        Vector2Int exitPos;
        int attempts = 0;
        do
        {
            exitPos = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
            attempts++;
            
            // プレイヤー位置と被らないかチェック
            bool isPlayerPosition = (Player.Instance != null && exitPos == Player.Instance.gridPosition);
            bool isCenterPosition = (exitPos == new Vector2Int(width / 2, height / 2));
            
            // グリッド範囲内かチェック（UnityEngine.Random.Range(0, width)は0からwidth-1を返すので、実際には常にグリッド内）
            if (!IsInsideGrid(exitPos))
            {
                Debug.LogWarning($"GridManager: Exit位置がグリッド外 - {exitPos}, グリッドサイズ: {width}x{height}");
                continue;
            }
            
            // プレイヤー位置や中心位置と被らない場合はループを抜ける
            if (!isPlayerPosition && !isCenterPosition)
            {
                break;
            }
        }
        while (attempts < 100); // 最大試行回数を制限
        
        // 最終的な位置を確認
        if (!IsInsideGrid(exitPos))
        {
            Debug.LogError($"GridManager: Exit位置がグリッド外です - {exitPos}, グリッドサイズ: {width}x{height}");
            // フォールバック: グリッド内の安全な位置を使用
            exitPos = new Vector2Int(width - 1, height - 1);
        }
        
        Vector3 worldPos = GetWorldPosition(exitPos);
        exitObject = Instantiate(exitPrefab, worldPos, Quaternion.identity);
        exitPosition = exitPos;
        
        OnExitSpawned?.Invoke(exitObject);
        Debug.Log($"GridManager: Exit生成完了 - 位置: {exitPosition}, グリッド内: {IsInsideGrid(exitPosition)}");
        
        yield return null;
    }
    
    /// <summary>
    /// 敵生成コルーチン
    /// </summary>
    private System.Collections.IEnumerator SpawnEnemiesCoroutine()
    {
        Debug.Log("GridManager: 敵生成開始");
        
        if (EnemyManager.Instance != null)
        {
            // EnemyManagerを使用して敵を生成
            EnemyManager.Instance.SpawnEnemies();
            allEnemies = FindObjectsOfType<Enemy>();
            OnEnemiesSpawned?.Invoke(allEnemies);
            Debug.Log($"GridManager: 敵生成完了 - {allEnemies.Length}体");
        }
        else
        {
            Debug.LogWarning("GridManager: EnemyManagerが見つかりません。手動で敵を生成します。");
            
                         // 手動で敵を生成（デフォルト値を使用）
             int enemyCount = 3; // デフォルト値
             GameObject enemyPrefab = Resources.Load<GameObject>("Prefabs/Enemy"); // デフォルトプレハブ
             
             for (int i = 0; i < enemyCount; i++)
             {
                 Vector2Int enemyPos;
                 int attempts = 0;
                 
                 do
                 {
                     enemyPos = new Vector2Int(UnityEngine.Random.Range(0, width), UnityEngine.Random.Range(0, height));
                     attempts++;
                     
                     // グリッド範囲内かチェック（UnityEngine.Random.Range(0, width)は0からwidth-1を返すので、実際には常にグリッド内）
                     if (!IsInsideGrid(enemyPos))
                     {
                         Debug.LogWarning($"GridManager: 敵位置がグリッド外 - {enemyPos}, グリッドサイズ: {width}x{height}");
                         continue;
                     }
                     
                     // 占有されていない場合はループを抜ける
                     if (!IsOccupied(enemyPos))
                     {
                         break;
                     }
                 } while (attempts < 100);
                 
                 // 最終的な位置を確認
                 if (!IsInsideGrid(enemyPos))
                 {
                     Debug.LogError($"GridManager: 敵位置がグリッド外です - {enemyPos}, グリッドサイズ: {width}x{height}");
                     continue; // この敵はスキップ
                 }
                 
                 if (!IsOccupied(enemyPos))
                 {
                     Vector3 worldPos = GetWorldPosition(enemyPos);
                     GameObject enemyObj = Instantiate(enemyPrefab, worldPos, Quaternion.identity);
                     Enemy enemy = enemyObj.GetComponent<Enemy>();
                     
                     if (enemy != null)
                     {
                         enemy.Initialize(enemyPos);
                         Debug.Log($"GridManager: 敵生成完了 - 位置: {enemyPos}, グリッド内: {IsInsideGrid(enemyPos)}");
                     }
                 }
                 else
                 {
                     Debug.LogWarning($"GridManager: 敵の生成位置が占有されています - {enemyPos}");
                 }
             }
            
            allEnemies = FindObjectsOfType<Enemy>();
            OnEnemiesSpawned?.Invoke(allEnemies);
            Debug.Log($"GridManager: 手動敵生成完了 - {allEnemies.Length}体");
        }
        
        yield return null;
    }
} 