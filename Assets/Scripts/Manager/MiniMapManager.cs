using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[DefaultExecutionOrder(-85)]
public class MiniMapManager : MonoBehaviour
{
    public static MiniMapManager Instance { get; private set; }
    
    public static MiniMapManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            GameObject go = new GameObject("MiniMapManager");
            Instance = go.AddComponent<MiniMapManager>();
        }
        return Instance;
    }

    [Header("MiniMap Settings")]
    public RectTransform miniMapPanel;
    public GameObject miniMapTilePrefab;
    
    [Header("MiniMap Size Settings")]
    [Tooltip("ミニマップタイルのサイズ（ピクセル）")]
    [Range(4, 20)]
    public float tileSize = 8f;
    
    [Tooltip("タイル間の間隔（ピクセル）")]
    [Range(0, 5)]
    public float tileSpacing = 1f;
    
    [Tooltip("ミニマップパネルの最大幅（ピクセル）")]
    [Range(100, 500)]
    public float maxPanelWidth = 200f;
    
    [Tooltip("ミニマップパネルの最大高さ（ピクセル）")]
    [Range(100, 500)]
    public float maxPanelHeight = 200f;
    
    public enum MiniMapPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }
    
    [Header("MiniMap Position Settings")]
    [Tooltip("ミニマップの表示位置")]
    public MiniMapPosition miniMapPosition = MiniMapPosition.TopRight;
    
    [Tooltip("画面端からの余白（ピクセル）")]
    [Range(10, 50)]
    public float margin = 20f;
    
    [Header("MiniMap Colors")]
    public Color playerColor = Color.green;
    public Color enemyColor = Color.red;
    public Color defaultColor = Color.gray;
    public Color exitColor = Color.yellow;

    private Dictionary<Vector2Int, MiniMapTile> miniMapTiles = new();

    private void Awake()
    {
        Debug.Log("MiniMapManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("MiniMapManager: 重複するMiniMapManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化（シーン間で保持）
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("MiniMapManager: Awake完了");
    }

    private void Start()
    {
        Debug.Log("MiniMapManager: Start開始");
        
        // イベントを購読
        SubscribeToEvents();
        
        Debug.Log("MiniMapManager: Start完了");
    }
    
    private void OnDestroy()
    {
        // イベントの購読を解除
        UnsubscribeFromEvents();
    }
    
    private void SubscribeToEvents()
    {
        GridManager.OnAllObjectsInitialized += OnAllObjectsInitialized;
        
        // EnemyManagerのイベントを購読
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyListChanged += RefreshMiniMap;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        GridManager.OnAllObjectsInitialized -= OnAllObjectsInitialized;
        
        // EnemyManagerのイベントを購読解除
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyListChanged -= RefreshMiniMap;
        }
    }

    private void OnAllObjectsInitialized()
    {
        Debug.Log("MiniMapManager: 全オブジェクト初期化完了イベントを受信");
        
        if (GridManager.Instance != null)
        {
            GenerateMiniMap(GridManager.Instance.width, GridManager.Instance.height);
        }
        
        // EnemyManagerのイベントを購読（初期化完了後に再度試行）
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.OnEnemyListChanged += RefreshMiniMap;
            Debug.Log("MiniMapManager: EnemyManagerのイベントを購読しました");
        }
        else
        {
            Debug.LogWarning("MiniMapManager: EnemyManager.Instanceが見つかりません");
        }
    }

    public void GenerateMiniMap(int width, int height)
    {
        Debug.Log($"MiniMapManager: ミニマップ生成開始 ({width}x{height})");
        
        // 設定の確認
        Debug.Log($"MiniMapManager: miniMapPanel = {(miniMapPanel != null ? "設定済み" : "未設定")}");
        Debug.Log($"MiniMapManager: miniMapTilePrefab = {(miniMapTilePrefab != null ? "設定済み" : "未設定")}");
        
        // 既存のミニマップタイルをクリア
        ClearMiniMap();
        
        if (miniMapPanel == null)
        {
            Debug.LogError("MiniMapManager: miniMapPanelが設定されていません");
            return;
        }
        
        if (miniMapTilePrefab == null)
        {
            Debug.LogError("MiniMapManager: miniMapTilePrefabが設定されていません");
            return;
        }
        
        // ミニマップパネルのサイズを設定
        SetupMiniMapPanelSize(width, height);
        
        // ミニマップの位置を設定
        SetupMiniMapPosition();
        
        // 既存のGridLayoutGroupを削除
        GridLayoutGroup existingGrid = miniMapPanel.GetComponent<GridLayoutGroup>();
        if (existingGrid != null)
        {
            DestroyImmediate(existingGrid);
        }
        
        // 手動でタイルを配置（座標変換を修正）
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int gamePos = new Vector2Int(x, y);
                
                GameObject tileObj = Instantiate(miniMapTilePrefab, miniMapPanel);
                MiniMapTile tile = tileObj.GetComponent<MiniMapTile>();
                
                if (tile == null)
                {
                    Debug.LogError($"MiniMapManager: MiniMapTileコンポーネントが見つかりません");
                    continue;
                }
                
                // タイルの位置を手動で計算（パネルの設定位置に合わせて調整）
                RectTransform tileRect = tileObj.GetComponent<RectTransform>();
                float tileX = x * (tileSize + tileSpacing);
                float tileY = -(height - 1 - y) * (tileSize + tileSpacing); // パネルの設定位置に合わせて調整
                
                tileRect.anchoredPosition = new Vector2(tileX, tileY);
                tileRect.sizeDelta = new Vector2(tileSize, tileSize);
                
                tile.Initialize(gamePos); // ゲーム座標をそのまま使用
                miniMapTiles[gamePos] = tile;
            }
        }
        
        Debug.Log($"MiniMapManager: ミニマップ生成完了 ({miniMapTiles.Count}個のタイル)");
        
        // 生成されたタイルの詳細をログ出力
        Debug.Log($"MiniMapManager: 生成されたタイル辞書のサイズ: {miniMapTiles.Count}");
        
        // 初期状態でミニマップを更新
        if (Player.Instance != null && EnemyManager.Instance != null && GridManager.Instance != null)
        {
            List<Vector2Int> enemies = EnemyManager.Instance.GetAllEnemyPositions();
            Vector2Int exitPos = GridManager.Instance.exitPosition;
            UpdateMiniMap(Player.Instance.gridPosition, enemies, exitPos);
            Debug.Log($"MiniMapManager: 初期ミニマップ更新完了 - プレイヤー: {Player.Instance.gridPosition}, 敵数: {enemies.Count}, Exit: {exitPos}");
        }
        else
        {
            Debug.LogWarning("MiniMapManager: 初期ミニマップ更新に必要なインスタンスが見つかりません");
        }
    }

    public void UpdateMiniMap(Vector2Int playerPos, List<Vector2Int> enemyPositions, Vector2Int exitPos)
    {
        // 全てのタイルをデフォルト色にリセット
        foreach (var kv in miniMapTiles)
        {
            kv.Value.SetColor(defaultColor);
        }

        // プレイヤー位置を緑色で表示
        if (miniMapTiles.ContainsKey(playerPos))
            miniMapTiles[playerPos].SetColor(playerColor);

        // 敵位置を赤色で表示
        foreach (var pos in enemyPositions)
        {
            if (miniMapTiles.ContainsKey(pos))
                miniMapTiles[pos].SetColor(enemyColor);
        }
        
        // Exit位置を黄色で表示
        if (miniMapTiles.ContainsKey(exitPos))
            miniMapTiles[exitPos].SetColor(exitColor);
    }

    private void SetupMiniMapPanelSize(int width, int height)
    {
        // ミニマップパネルのサイズを計算（手動配置用）
        float panelWidth = width * tileSize + (width - 1) * tileSpacing;
        float panelHeight = height * tileSize + (height - 1) * tileSpacing;
        
        // 最大サイズを超えないように制限
        panelWidth = Mathf.Min(panelWidth, maxPanelWidth);
        panelHeight = Mathf.Min(panelHeight, maxPanelHeight);
        
        // パネルサイズを設定
        miniMapPanel.sizeDelta = new Vector2(panelWidth, panelHeight);
        
        Debug.Log($"MiniMapManager: ミニマップパネルサイズ設定 - 幅: {panelWidth}, 高さ: {panelHeight}");
        Debug.Log($"MiniMapManager: タイルサイズ: {tileSize}, 間隔: {tileSpacing}, 最大サイズ: {maxPanelWidth}x{maxPanelHeight}");
    }
    
    private void SetupMiniMapPosition()
    {
        if (miniMapPanel == null) return;
        
        // パネルのサイズを取得
        Vector2 panelSize = miniMapPanel.sizeDelta;
        
        // 位置を計算（パネルのサイズを考慮）
        Vector2 position = Vector2.zero;
        
        switch (miniMapPosition)
        {
            case MiniMapPosition.TopLeft:
                position = new Vector2(margin, -margin);
                miniMapPanel.anchorMin = new Vector2(0, 1);
                miniMapPanel.anchorMax = new Vector2(0, 1);
                break;
                
            case MiniMapPosition.TopRight:
                position = new Vector2(-margin - panelSize.x, -margin);
                miniMapPanel.anchorMin = new Vector2(1, 1);
                miniMapPanel.anchorMax = new Vector2(1, 1);
                break;
                
            case MiniMapPosition.BottomLeft:
                position = new Vector2(margin, margin + panelSize.y);
                miniMapPanel.anchorMin = new Vector2(0, 0);
                miniMapPanel.anchorMax = new Vector2(0, 0);
                break;
                
            case MiniMapPosition.BottomRight:
                position = new Vector2(-margin - panelSize.x, margin + panelSize.y);
                miniMapPanel.anchorMin = new Vector2(1, 0);
                miniMapPanel.anchorMax = new Vector2(1, 0);
                break;
                
            case MiniMapPosition.Center:
                position = new Vector2(-panelSize.x * 0.5f, -panelSize.y * 0.5f);
                miniMapPanel.anchorMin = new Vector2(0.5f, 0.5f);
                miniMapPanel.anchorMax = new Vector2(0.5f, 0.5f);
                break;
        }
        
        // 位置を設定
        miniMapPanel.anchoredPosition = position;
        
        Debug.Log($"MiniMapManager: ミニマップ位置設定 - 位置: {miniMapPosition}, 座標: {position}, パネルサイズ: {panelSize}");
    }
    

    
    private void ClearMiniMap()
    {
        foreach (var tile in miniMapTiles.Values)
        {
            if (tile != null)
                Destroy(tile.gameObject);
        }
        miniMapTiles.Clear();
    }

    // ミニマップを即座に更新するメソッド
    private void RefreshMiniMap()
    {
        if (Player.Instance != null && EnemyManager.Instance != null && GridManager.Instance != null)
        {
            List<Vector2Int> enemies = EnemyManager.Instance.GetAllEnemyPositions();
            Vector2Int exitPos = GridManager.Instance.exitPosition;
            UpdateMiniMap(Player.Instance.gridPosition, enemies, exitPos);
            Debug.Log($"MiniMapManager: 敵リスト変更によりミニマップを更新 - プレイヤー: {Player.Instance.gridPosition}, 敵数: {enemies.Count}, Exit: {exitPos}");
        }
    }
} 