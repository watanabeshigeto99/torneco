using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// 階層システム管理専用コンポーネント
/// 責務：階層の進行、階層データの管理のみ
/// </summary>
[DefaultExecutionOrder(-85)]
public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }
    
    [Header("Floor System")]
    public int currentFloor = 1;
    public int maxFloor = 10;
    
    // 階層システム変更イベント
    public static event Action<int> OnFloorChanged;
    public static event Action OnGameClear;
    public static event Action OnFloorSystemChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeFloorSystem();
    }
    
    /// <summary>
    /// 階層システムの初期化
    /// </summary>
    private void InitializeFloorSystem()
    {
        currentFloor = 1;
        maxFloor = 10;
        
        Debug.Log("FloorManager: 階層システムを初期化しました");
    }
    
    /// <summary>
    /// 階層を設定
    /// </summary>
    /// <param name="floor">設定する階層</param>
    public void SetFloor(int floor)
    {
        if (floor != currentFloor && floor >= 1 && floor <= maxFloor)
        {
            int oldFloor = currentFloor;
            currentFloor = floor;
            
            OnFloorChanged?.Invoke(currentFloor);
            OnFloorSystemChanged?.Invoke();
            
            Debug.Log($"FloorManager: 階層を設定しました - {oldFloor} → {currentFloor}");
            
            // 最終階層到達時の処理
            if (currentFloor >= maxFloor)
            {
                OnGameClear?.Invoke();
                Debug.Log("FloorManager: 最終階層に到達しました");
            }
        }
    }
    
    /// <summary>
    /// 次の階層に進む
    /// </summary>
    public void GoToNextFloor()
    {
        if (currentFloor < maxFloor)
        {
            SetFloor(currentFloor + 1);
        }
        else
        {
            Debug.LogWarning("FloorManager: 既に最終階層です");
        }
    }
    
    /// <summary>
    /// 階層をリセット
    /// </summary>
    public void ResetFloor()
    {
        SetFloor(1);
    }
    
    /// <summary>
    /// 最大階層を設定
    /// </summary>
    /// <param name="maxFloor">最大階層</param>
    public void SetMaxFloor(int maxFloor)
    {
        if (maxFloor > 0 && maxFloor != this.maxFloor)
        {
            this.maxFloor = maxFloor;
            OnFloorSystemChanged?.Invoke();
            
            Debug.Log($"FloorManager: 最大階層を設定しました - {maxFloor}");
        }
    }
    
    /// <summary>
    /// 現在の階層情報を取得
    /// </summary>
    /// <returns>階層情報文字列</returns>
    public string GetFloorInfo()
    {
        return $"Floor - Current: {currentFloor}, Max: {maxFloor}";
    }
    
    /// <summary>
    /// 最終階層に到達しているかチェック
    /// </summary>
    /// <returns>最終階層に到達している場合true</returns>
    public bool IsAtMaxFloor()
    {
        return currentFloor >= maxFloor;
    }
    
    /// <summary>
    /// 階層進行の進捗率を取得
    /// </summary>
    /// <returns>進捗率（0.0f～1.0f）</returns>
    public float GetFloorProgress()
    {
        return (float)(currentFloor - 1) / (maxFloor - 1);
    }
} 