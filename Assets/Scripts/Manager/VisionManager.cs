using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 視界管理専用クラス
/// 責務：視界範囲、タイル可視性、透明度管理のみ
/// </summary>
[DefaultExecutionOrder(-85)]
public class VisionManager : MonoBehaviour
{
    public static VisionManager Instance { get; private set; }
    
    [Header("Vision Settings")]
    [Range(1, 10)]
    [Tooltip("プレイヤーを中心とした視界範囲（マス数）")]
    public int visionRange = 8;
    
    [Tooltip("視界範囲をデバッグログに表示")]
    public bool showVisionRange = true;
    
    [Header("Tile Visibility Settings")]
    [Tooltip("通常表示範囲（プレイヤー周囲のマス数）")]
    public int normalVisibilityRange = 3;
    
    [Tooltip("半透明表示範囲（通常表示範囲の外側のマス数）")]
    public int transparentVisibilityRange = 5;
    
    // イベント定義
    public static event Action<Vector2Int> OnVisionRangeUpdated;
    public static event Action<Vector2Int> OnTileVisibilityUpdated;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// 視界範囲内かチェック
    /// </summary>
    public bool IsInVisionRange(Vector2Int position, Vector2Int playerPosition)
    {
        int distance = Mathf.Abs(position.x - playerPosition.x) + Mathf.Abs(position.y - playerPosition.y);
        return distance <= visionRange;
    }
    
    /// <summary>
    /// タイルの可視性を更新
    /// </summary>
    public void UpdateTileVisibility(Vector2Int playerPosition)
    {
        if (GridManager.Instance == null) return;
        
        // タイルの可視性を更新
        for (int x = 0; x < GridManager.Instance.width; x++)
        {
            for (int y = 0; y < GridManager.Instance.height; y++)
            {
                Vector2Int tilePos = new Vector2Int(x, y);
                Tile.VisibilityState visibilityState = GetTileVisibilityState(tilePos, playerPosition);
                
                // タイルの可視性を設定
                if (GridManager.Instance.tileDict.TryGetValue(tilePos, out Tile tile))
                {
                    tile.SetVisibility(visibilityState);
                }
            }
        }
        
        // 敵の可視性を更新
        UpdateEnemyVisibility(playerPosition);
        
        OnTileVisibilityUpdated?.Invoke(playerPosition);
    }
    
    /// <summary>
    /// 敵の可視性を更新
    /// </summary>
    private void UpdateEnemyVisibility(Vector2Int playerPosition)
    {
        if (EnemyManager.Instance == null) return;
        
        var enemies = EnemyManager.Instance.GetEnemies();
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                Vector2Int enemyPos = enemy.GetPosition();
                Tile.VisibilityState visibilityState = GetTileVisibilityState(enemyPos, playerPosition);
                
                // 敵の表示/非表示を制御
                switch (visibilityState)
                {
                    case Tile.VisibilityState.Hidden:
                        enemy.gameObject.SetActive(false);
                        break;
                    case Tile.VisibilityState.Transparent:
                        enemy.gameObject.SetActive(true);
                        if (enemy.spriteRenderer != null)
                        {
                            Color transparentColor = enemy.spriteRenderer.color;
                            transparentColor.a = 0.5f;
                            enemy.spriteRenderer.color = transparentColor;
                        }
                        break;
                    case Tile.VisibilityState.Visible:
                        enemy.gameObject.SetActive(true);
                        if (enemy.spriteRenderer != null)
                        {
                            Color normalColor = enemy.spriteRenderer.color;
                            normalColor.a = 1f;
                            enemy.spriteRenderer.color = normalColor;
                        }
                        break;
                }
            }
        }
    }
    
    /// <summary>
    /// 視界範囲を更新
    /// </summary>
    public void UpdateVisionRange(Vector2Int playerPosition)
    {
        if (showVisionRange)
        {
            ShowVisionRangeDebug(playerPosition);
        }
        
        OnVisionRangeUpdated?.Invoke(playerPosition);
    }
    
    /// <summary>
    /// タイルの可視性状態を取得
    /// </summary>
    public Tile.VisibilityState GetTileVisibilityState(Vector2Int tilePos, Vector2Int playerPos)
    {
        int distance = Mathf.Abs(tilePos.x - playerPos.x) + Mathf.Abs(tilePos.y - playerPos.y);
        
        if (distance <= normalVisibilityRange)
        {
            return Tile.VisibilityState.Visible;
        }
        else if (distance <= normalVisibilityRange + transparentVisibilityRange)
        {
            return Tile.VisibilityState.Transparent;
        }
        else
        {
            return Tile.VisibilityState.Hidden;
        }
    }
    
    /// <summary>
    /// 視界範囲のデバッグ表示
    /// </summary>
    public void ShowVisionRangeDebug(Vector2Int playerPosition)
    {
#if UNITY_EDITOR
        Debug.Log($"VisionManager: プレイヤー位置 {playerPosition} の視界範囲を表示");
        
        for (int x = -visionRange; x <= visionRange; x++)
        {
            for (int y = -visionRange; y <= visionRange; y++)
            {
                Vector2Int checkPos = playerPosition + new Vector2Int(x, y);
                if (GridManager.Instance.IsInsideGrid(checkPos))
                {
                    Tile.VisibilityState state = GetTileVisibilityState(checkPos, playerPosition);
                    string stateStr = state switch
                    {
                        Tile.VisibilityState.Visible => "V",
                        Tile.VisibilityState.Transparent => "T",
                        Tile.VisibilityState.Hidden => "H",
                        _ => "?"
                    };
                    
                    Debug.Log($"VisionManager: 位置 {checkPos} = {stateStr}");
                }
            }
        }
#endif
    }
}
