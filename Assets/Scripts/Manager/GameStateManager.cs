using UnityEngine;
using System;

/// <summary>
/// ゲーム状態管理専用コンポーネント
/// 責務：ゲームの基本状態（スコア、ゲームオーバー、ゲームクリア）の管理のみ
/// </summary>
[DefaultExecutionOrder(-95)]
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    [Header("Game State")]
    public int score = 0;
    public bool gameOver = false;
    public bool gameClear = false;
    
    // ゲーム状態変更イベント
    public static event Action<int> OnScoreChanged;
    public static event Action OnGameOver;
    public static event Action OnGameClear;
    public static event Action OnGameStateChanged;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        DontDestroyOnLoad(gameObject);
        
        InitializeGameState();
    }
    
    /// <summary>
    /// ゲーム状態の初期化
    /// </summary>
    private void InitializeGameState()
    {
        score = 0;
        gameOver = false;
        gameClear = false;
        
        Debug.Log("GameStateManager: ゲーム状態を初期化しました");
    }
    
    /// <summary>
    /// スコアを設定
    /// </summary>
    /// <param name="newScore">新しいスコア</param>
    public void SetScore(int newScore)
    {
        if (score != newScore)
        {
            score = newScore;
            OnScoreChanged?.Invoke(score);
            OnGameStateChanged?.Invoke();
            
            Debug.Log($"GameStateManager: スコアを設定しました - {score}");
        }
    }
    
    /// <summary>
    /// スコアを加算
    /// </summary>
    /// <param name="amount">加算するスコア</param>
    public void AddScore(int amount)
    {
        if (amount > 0)
        {
            SetScore(score + amount);
        }
    }
    
    /// <summary>
    /// ゲームオーバー状態を設定
    /// </summary>
    public void Defeat()
    {
        if (!gameOver)
        {
            gameOver = true;
            OnGameOver?.Invoke();
            OnGameStateChanged?.Invoke();
            
            Debug.Log("GameStateManager: ゲームオーバー状態を設定しました");
        }
    }
    
    /// <summary>
    /// ゲームクリア状態を設定
    /// </summary>
    public void Victory()
    {
        if (!gameClear)
        {
            gameClear = true;
            OnGameClear?.Invoke();
            OnGameStateChanged?.Invoke();
            
            Debug.Log("GameStateManager: ゲームクリア状態を設定しました");
        }
    }
    
    /// <summary>
    /// ゲーム状態をリセット
    /// </summary>
    public void ResetGameState()
    {
        score = 0;
        gameOver = false;
        gameClear = false;
        
        OnGameStateChanged?.Invoke();
        
        Debug.Log("GameStateManager: ゲーム状態をリセットしました");
    }
    
    /// <summary>
    /// ゲーム状態の情報を取得
    /// </summary>
    /// <returns>ゲーム状態の情報文字列</returns>
    public string GetGameStateInfo()
    {
        return $"GameState - Score: {score}, GameOver: {gameOver}, GameClear: {gameClear}";
    }
} 