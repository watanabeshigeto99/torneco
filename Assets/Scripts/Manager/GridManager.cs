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
        Instantiate(playerPrefab, pos, Quaternion.identity);
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
        Instantiate(exitPrefab, pos, Quaternion.identity);
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
} 