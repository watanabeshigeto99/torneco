using UnityEngine;
using System;
using System.Collections.Generic;

namespace UISystem
{
    /// <summary>
    /// UIデータ管理用ScriptableObject
    /// 責務：UIデータの状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "UIData", menuName = "UI System/UI Data")]
    public class UIDataSO : ScriptableObject
    {
        [Header("Player Status")]
        public int currentHP = 20;
        public int maxHP = 20;
        public int playerLevel = 1;
        public int playerExp = 0;
        public int playerExpToNext = 10;
        public int score = 0;
        public int currentFloor = 1;
        
        [Header("UI State")]
        public bool isPaused = false;
        public bool isLoading = false;
        public bool isGameOver = false;
        public bool isGameClear = false;
        public bool isMenuOpen = false;
        
        [Header("UI Settings")]
        public bool showHUD = true;
        public bool showLog = true;
        public bool showCards = true;
        public float uiScale = 1.0f;
        public bool enableAnimations = true;
        
        [Header("Log System")]
        public List<string> logMessages = new List<string>();
        public int maxLogMessages = 100;
        public bool autoScrollLog = true;
        
        [Header("Card UI")]
        public List<CardDataSO> displayedCards = new List<CardDataSO>();
        public int maxDisplayedCards = 10;
        public bool showCardDetails = true;
        
        [Header("UI Statistics")]
        public int totalLogMessages = 0;
        public int totalCardUIsCreated = 0;
        public int totalScreenTransitions = 0;
        public float averageUIRefreshRate = 0f;
        
        // イベント
        public event Action<UIDataSO> OnDataChanged;
        public event Action<int, int> OnHPChanged;
        public event Action<int, int, int> OnLevelChanged;
        public event Action<int> OnScoreChanged;
        public event Action<int> OnFloorChanged;
        public event Action<string> OnLogAdded;
        public event Action OnLogCleared;
        public event Action<bool> OnPauseStateChanged;
        public event Action<bool> OnLoadingStateChanged;
        
        /// <summary>
        /// UIデータの初期化
        /// </summary>
        public void Initialize()
        {
            currentHP = 20;
            maxHP = 20;
            playerLevel = 1;
            playerExp = 0;
            playerExpToNext = 10;
            score = 0;
            currentFloor = 1;
            
            isPaused = false;
            isLoading = false;
            isGameOver = false;
            isGameClear = false;
            isMenuOpen = false;
            
            showHUD = true;
            showLog = true;
            showCards = true;
            uiScale = 1.0f;
            enableAnimations = true;
            
            logMessages.Clear();
            displayedCards.Clear();
            
            totalLogMessages = 0;
            totalCardUIsCreated = 0;
            totalScreenTransitions = 0;
            averageUIRefreshRate = 0f;
            
            Debug.Log("UIDataSO: UIデータを初期化しました");
        }
        
        /// <summary>
        /// HPを設定
        /// </summary>
        public void SetHP(int currentHP, int maxHP)
        {
            this.currentHP = currentHP;
            this.maxHP = maxHP;
            
            OnHPChanged?.Invoke(currentHP, maxHP);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: HPを設定しました - {currentHP}/{maxHP}");
        }
        
        /// <summary>
        /// レベルを設定
        /// </summary>
        public void SetLevel(int level, int exp, int expToNext)
        {
            playerLevel = level;
            playerExp = exp;
            playerExpToNext = expToNext;
            
            OnLevelChanged?.Invoke(level, exp, expToNext);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: レベルを設定しました - {level}, Exp: {exp}/{expToNext}");
        }
        
        /// <summary>
        /// スコアを設定
        /// </summary>
        public void SetScore(int score)
        {
            this.score = score;
            
            OnScoreChanged?.Invoke(score);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: スコアを設定しました - {score}");
        }
        
        /// <summary>
        /// 階層を設定
        /// </summary>
        public void SetFloor(int floor)
        {
            currentFloor = floor;
            
            OnFloorChanged?.Invoke(floor);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: 階層を設定しました - {floor}");
        }
        
        /// <summary>
        /// ログを追加
        /// </summary>
        public void AddLog(string message)
        {
            if (string.IsNullOrEmpty(message)) return;
            
            logMessages.Insert(0, message);
            totalLogMessages++;
            
            // 最大ログ数を制限
            if (logMessages.Count > maxLogMessages)
            {
                logMessages.RemoveAt(logMessages.Count - 1);
            }
            
            OnLogAdded?.Invoke(message);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: ログを追加しました - {message}");
        }
        
        /// <summary>
        /// ログをクリア
        /// </summary>
        public void ClearLog()
        {
            logMessages.Clear();
            
            OnLogCleared?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log("UIDataSO: ログをクリアしました");
        }
        
        /// <summary>
        /// ポーズ状態を設定
        /// </summary>
        public void SetPauseState(bool isPaused)
        {
            this.isPaused = isPaused;
            
            OnPauseStateChanged?.Invoke(isPaused);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: ポーズ状態を設定しました - {isPaused}");
        }
        
        /// <summary>
        /// ローディング状態を設定
        /// </summary>
        public void SetLoadingState(bool isLoading)
        {
            this.isLoading = isLoading;
            
            OnLoadingStateChanged?.Invoke(isLoading);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: ローディング状態を設定しました - {isLoading}");
        }
        
        /// <summary>
        /// カードを表示リストに追加
        /// </summary>
        public void AddDisplayedCard(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (displayedCards.Count >= maxDisplayedCards)
            {
                Debug.LogWarning("UIDataSO: 表示カード数が最大に達しています");
                return;
            }
            
            displayedCards.Add(cardData);
            totalCardUIsCreated++;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: 表示カードを追加しました - {cardData.cardName}");
        }
        
        /// <summary>
        /// カードを表示リストから削除
        /// </summary>
        public void RemoveDisplayedCard(CardDataSO cardData)
        {
            if (cardData == null) return;
            
            if (displayedCards.Remove(cardData))
            {
                OnDataChanged?.Invoke(this);
                
                Debug.Log($"UIDataSO: 表示カードを削除しました - {cardData.cardName}");
            }
        }
        
        /// <summary>
        /// すべての表示カードをクリア
        /// </summary>
        public void ClearDisplayedCards()
        {
            displayedCards.Clear();
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log("UIDataSO: すべての表示カードをクリアしました");
        }
        
        /// <summary>
        /// 画面遷移を記録
        /// </summary>
        public void RecordScreenTransition()
        {
            totalScreenTransitions++;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: 画面遷移を記録しました - 合計: {totalScreenTransitions}");
        }
        
        /// <summary>
        /// UIスケールを設定
        /// </summary>
        public void SetUIScale(float scale)
        {
            uiScale = Mathf.Clamp(scale, 0.5f, 2.0f);
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: UIスケールを設定しました - {uiScale}");
        }
        
        /// <summary>
        /// アニメーション設定を変更
        /// </summary>
        public void SetAnimationEnabled(bool enabled)
        {
            enableAnimations = enabled;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: アニメーション設定を変更しました - {enabled}");
        }
        
        /// <summary>
        /// HUD表示設定を変更
        /// </summary>
        public void SetHUDVisibility(bool visible)
        {
            showHUD = visible;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: HUD表示設定を変更しました - {visible}");
        }
        
        /// <summary>
        /// ログ表示設定を変更
        /// </summary>
        public void SetLogVisibility(bool visible)
        {
            showLog = visible;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: ログ表示設定を変更しました - {visible}");
        }
        
        /// <summary>
        /// カード表示設定を変更
        /// </summary>
        public void SetCardVisibility(bool visible)
        {
            showCards = visible;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"UIDataSO: カード表示設定を変更しました - {visible}");
        }
        
        /// <summary>
        /// 現在のHPを取得
        /// </summary>
        public int GetCurrentHP()
        {
            return currentHP;
        }
        
        /// <summary>
        /// 最大HPを取得
        /// </summary>
        public int GetMaxHP()
        {
            return maxHP;
        }
        
        /// <summary>
        /// HPの割合を取得
        /// </summary>
        public float GetHPPercentage()
        {
            return maxHP > 0 ? (float)currentHP / maxHP : 0f;
        }
        
        /// <summary>
        /// プレイヤーレベルを取得
        /// </summary>
        public int GetPlayerLevel()
        {
            return playerLevel;
        }
        
        /// <summary>
        /// プレイヤー経験値を取得
        /// </summary>
        public int GetPlayerExp()
        {
            return playerExp;
        }
        
        /// <summary>
        /// 次のレベルまでの経験値を取得
        /// </summary>
        public int GetPlayerExpToNext()
        {
            return playerExpToNext;
        }
        
        /// <summary>
        /// 経験値の割合を取得
        /// </summary>
        public float GetExpPercentage()
        {
            return playerExpToNext > 0 ? (float)playerExp / playerExpToNext : 0f;
        }
        
        /// <summary>
        /// スコアを取得
        /// </summary>
        public int GetScore()
        {
            return score;
        }
        
        /// <summary>
        /// 現在の階層を取得
        /// </summary>
        public int GetCurrentFloor()
        {
            return currentFloor;
        }
        
        /// <summary>
        /// ログメッセージ数を取得
        /// </summary>
        public int GetLogMessageCount()
        {
            return logMessages.Count;
        }
        
        /// <summary>
        /// 表示カード数を取得
        /// </summary>
        public int GetDisplayedCardCount()
        {
            return displayedCards.Count;
        }
        
        /// <summary>
        /// ポーズ中かチェック
        /// </summary>
        public bool IsPaused()
        {
            return isPaused;
        }
        
        /// <summary>
        /// ローディング中かチェック
        /// </summary>
        public bool IsLoading()
        {
            return isLoading;
        }
        
        /// <summary>
        /// ゲームオーバーかチェック
        /// </summary>
        public bool IsGameOver()
        {
            return isGameOver;
        }
        
        /// <summary>
        /// ゲームクリアかチェック
        /// </summary>
        public bool IsGameClear()
        {
            return isGameClear;
        }
        
        /// <summary>
        /// メニューが開いているかチェック
        /// </summary>
        public bool IsMenuOpen()
        {
            return isMenuOpen;
        }
        
        /// <summary>
        /// UIの情報を取得
        /// </summary>
        public string GetUIInfo()
        {
            return $"UI - HP: {currentHP}/{maxHP}, Level: {playerLevel}, " +
                   $"Score: {score}, Floor: {currentFloor}, " +
                   $"Logs: {logMessages.Count}, Cards: {displayedCards.Count}";
        }
        
        /// <summary>
        /// UIの詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== UI Data ===\n" +
                   $"Current HP: {currentHP}/{maxHP}\n" +
                   $"Player Level: {playerLevel}\n" +
                   $"Player Exp: {playerExp}/{playerExpToNext}\n" +
                   $"Score: {score}\n" +
                   $"Current Floor: {currentFloor}\n" +
                   $"Is Paused: {isPaused}\n" +
                   $"Is Loading: {isLoading}\n" +
                   $"Is Game Over: {isGameOver}\n" +
                   $"Is Game Clear: {isGameClear}\n" +
                   $"Is Menu Open: {isMenuOpen}\n" +
                   $"Show HUD: {showHUD}\n" +
                   $"Show Log: {showLog}\n" +
                   $"Show Cards: {showCards}\n" +
                   $"UI Scale: {uiScale}\n" +
                   $"Enable Animations: {enableAnimations}\n" +
                   $"Log Messages: {logMessages.Count}/{maxLogMessages}\n" +
                   $"Displayed Cards: {displayedCards.Count}/{maxDisplayedCards}\n" +
                   $"Total Log Messages: {totalLogMessages}\n" +
                   $"Total Card UIs Created: {totalCardUIsCreated}\n" +
                   $"Total Screen Transitions: {totalScreenTransitions}";
        }
    }
} 