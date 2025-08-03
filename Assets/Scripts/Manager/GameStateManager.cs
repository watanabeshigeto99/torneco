using UnityEngine;
using System;

/// <summary>
/// ゲーム状態を管理するクラス
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    
    // ゲーム状態の定義
    public enum GameState
    {
        MainMenu,       // メインメニュー
        DeckBuilder,    // デッキ構築
        Battle,         // 戦闘中
        PlayerTurn,     // プレイヤーターン
        EnemyTurn,      // 敵ターン
        Victory,        // 勝利
        Defeat,         // 敗北
        Paused          // 一時停止
    }
    
    // 現在のゲーム状態
    private GameState currentState = GameState.MainMenu;
    
    // 状態変更イベント
    public static event Action<GameState, GameState> OnGameStateChanged;
    
    // プロパティ
    public GameState CurrentState => currentState;
    public bool IsInBattle => currentState == GameState.Battle || currentState == GameState.PlayerTurn || currentState == GameState.EnemyTurn;
    public bool IsPlayerTurn => currentState == GameState.PlayerTurn;
    public bool IsEnemyTurn => currentState == GameState.EnemyTurn;
    
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
    /// ゲーム状態を変更する
    /// </summary>
    /// <param name="newState">新しい状態</param>
    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState oldState = currentState;
        currentState = newState;
        
        Debug.Log($"GameStateManager: 状態変更 {oldState} → {newState}");
        
        // 状態変更イベントを発行
        OnGameStateChanged?.Invoke(oldState, newState);
        
        // 状態に応じた処理
        HandleStateChange(oldState, newState);
    }
    
    /// <summary>
    /// 状態変更時の処理
    /// </summary>
    private void HandleStateChange(GameState oldState, GameState newState)
    {
        switch (newState)
        {
            case GameState.MainMenu:
                HandleMainMenuState();
                break;
            case GameState.DeckBuilder:
                HandleDeckBuilderState();
                break;
            case GameState.Battle:
                HandleBattleState();
                break;
            case GameState.PlayerTurn:
                HandlePlayerTurnState();
                break;
            case GameState.EnemyTurn:
                HandleEnemyTurnState();
                break;
            case GameState.Victory:
                HandleVictoryState();
                break;
            case GameState.Defeat:
                HandleDefeatState();
                break;
            case GameState.Paused:
                HandlePausedState();
                break;
        }
    }
    
    private void HandleMainMenuState()
    {
        // メインメニュー状態の処理
        Time.timeScale = 1f;
    }
    
    private void HandleDeckBuilderState()
    {
        // デッキ構築状態の処理
        Time.timeScale = 1f;
    }
    
    private void HandleBattleState()
    {
        // 戦闘状態の処理
        Time.timeScale = 1f;
    }
    
    private void HandlePlayerTurnState()
    {
        // プレイヤーターン状態の処理
        Debug.Log("GameStateManager: プレイヤーターン開始");
        
        // プレイヤーの行動可能状態を設定
        if (Player.Instance != null)
        {
            // プレイヤーのターン開始処理
        }
    }
    
    private void HandleEnemyTurnState()
    {
        // 敵ターン状態の処理
        Debug.Log("GameStateManager: 敵ターン開始");
        
        // 敵の行動を実行
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.ExecuteEnemyTurns();
        }
    }
    
    private void HandleVictoryState()
    {
        // 勝利状態の処理
        Debug.Log("GameStateManager: 勝利");
        Time.timeScale = 0f; // 一時停止
    }
    
    private void HandleDefeatState()
    {
        // 敗北状態の処理
        Debug.Log("GameStateManager: 敗北");
        Time.timeScale = 0f; // 一時停止
    }
    
    private void HandlePausedState()
    {
        // 一時停止状態の処理
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// 戦闘開始
    /// </summary>
    public void StartBattle()
    {
        ChangeState(GameState.Battle);
        ChangeState(GameState.PlayerTurn);
    }
    
    /// <summary>
    /// プレイヤーターン終了
    /// </summary>
    public void EndPlayerTurn()
    {
        if (currentState == GameState.PlayerTurn)
        {
            ChangeState(GameState.EnemyTurn);
        }
    }
    
    /// <summary>
    /// 敵ターン終了
    /// </summary>
    public void EndEnemyTurn()
    {
        if (currentState == GameState.EnemyTurn)
        {
            ChangeState(GameState.PlayerTurn);
        }
    }
    
    /// <summary>
    /// 勝利
    /// </summary>
    public void Victory()
    {
        ChangeState(GameState.Victory);
    }
    
    /// <summary>
    /// 敗北
    /// </summary>
    public void Defeat()
    {
        ChangeState(GameState.Defeat);
    }
    
    /// <summary>
    /// 一時停止切り替え
    /// </summary>
    public void TogglePause()
    {
        if (currentState == GameState.Paused)
        {
            // 一時停止解除
            ChangeState(GameState.Battle);
        }
        else if (IsInBattle)
        {
            // 一時停止
            ChangeState(GameState.Paused);
        }
    }
} 