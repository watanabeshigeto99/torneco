using UnityEngine;
using UnityEngine.Events;

namespace UISystem
{
    /// <summary>
    /// UIシステムイベント管理用ScriptableObject
    /// 責務：UIシステムのイベント管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "UIEventChannel", menuName = "UI System/UI Event Channel")]
    public class UIEventChannel : ScriptableObject
    {
        [Header("UI Data Events")]
        public UnityEvent<UIDataSO> OnUIDataChanged;
        public UnityEvent<string> OnLogAdded;
        public UnityEvent OnLogCleared;
        public UnityEvent OnUIStateChanged;
        public UnityEvent OnScreenTransition;
        
        [Header("Player Status Events")]
        public UnityEvent<int, int> OnHPChanged;
        public UnityEvent<int, int, int> OnLevelChanged;
        public UnityEvent<int> OnScoreChanged;
        public UnityEvent<int> OnFloorChanged;
        
        [Header("UI State Events")]
        public UnityEvent<bool> OnPauseStateChanged;
        public UnityEvent<bool> OnLoadingStateChanged;
        public UnityEvent<bool> OnGameOverStateChanged;
        public UnityEvent<bool> OnGameClearStateChanged;
        public UnityEvent<bool> OnMenuStateChanged;
        
        [Header("Screen Events")]
        public UnityEvent OnLoadingScreenShown;
        public UnityEvent OnLoadingScreenHidden;
        public UnityEvent OnPauseMenuShown;
        public UnityEvent OnPauseMenuHidden;
        public UnityEvent OnGameOverScreenShown;
        public UnityEvent OnGameClearScreenShown;
        
        [Header("Card UI Events")]
        public UnityEvent<CardDataSO> OnCardUICreated;
        public UnityEvent OnCardUIRemoved;
        public UnityEvent OnAllCardUICleared;
        public UnityEvent<int> OnCardCountChanged;
        
        [Header("UI Settings Events")]
        public UnityEvent<float> OnUIScaleChanged;
        public UnityEvent<bool> OnAnimationEnabledChanged;
        public UnityEvent<bool> OnHUDVisibilityChanged;
        public UnityEvent<bool> OnLogVisibilityChanged;
        public UnityEvent<bool> OnCardVisibilityChanged;
        
        /// <summary>
        /// UIデータ変更イベントを発生
        /// </summary>
        public void RaiseUIDataChanged(UIDataSO uiData)
        {
            OnUIDataChanged?.Invoke(uiData);
            Debug.Log($"UIEventChannel: UIデータ変更イベントを発生しました - {uiData?.GetUIInfo() ?? "Unknown"}");
        }
        
        /// <summary>
        /// ログ追加イベントを発生
        /// </summary>
        public void RaiseLogAdded(string message)
        {
            OnLogAdded?.Invoke(message);
            Debug.Log($"UIEventChannel: ログ追加イベントを発生しました - {message}");
        }
        
        /// <summary>
        /// ログクリアイベントを発生
        /// </summary>
        public void RaiseLogCleared()
        {
            OnLogCleared?.Invoke();
            Debug.Log("UIEventChannel: ログクリアイベントを発生しました");
        }
        
        /// <summary>
        /// UI状態変更イベントを発生
        /// </summary>
        public void RaiseUIStateChanged()
        {
            OnUIStateChanged?.Invoke();
            Debug.Log("UIEventChannel: UI状態変更イベントを発生しました");
        }
        
        /// <summary>
        /// 画面遷移イベントを発生
        /// </summary>
        public void RaiseScreenTransition()
        {
            OnScreenTransition?.Invoke();
            Debug.Log("UIEventChannel: 画面遷移イベントを発生しました");
        }
        
        /// <summary>
        /// HP変更イベントを発生
        /// </summary>
        public void RaiseHPChanged(int currentHP, int maxHP)
        {
            OnHPChanged?.Invoke(currentHP, maxHP);
            Debug.Log($"UIEventChannel: HP変更イベントを発生しました - {currentHP}/{maxHP}");
        }
        
        /// <summary>
        /// レベル変更イベントを発生
        /// </summary>
        public void RaiseLevelChanged(int level, int exp, int expToNext)
        {
            OnLevelChanged?.Invoke(level, exp, expToNext);
            Debug.Log($"UIEventChannel: レベル変更イベントを発生しました - {level}, Exp: {exp}/{expToNext}");
        }
        
        /// <summary>
        /// スコア変更イベントを発生
        /// </summary>
        public void RaiseScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
            Debug.Log($"UIEventChannel: スコア変更イベントを発生しました - {score}");
        }
        
        /// <summary>
        /// 階層変更イベントを発生
        /// </summary>
        public void RaiseFloorChanged(int floor)
        {
            OnFloorChanged?.Invoke(floor);
            Debug.Log($"UIEventChannel: 階層変更イベントを発生しました - {floor}");
        }
        
        /// <summary>
        /// ポーズ状態変更イベントを発生
        /// </summary>
        public void RaisePauseStateChanged(bool isPaused)
        {
            OnPauseStateChanged?.Invoke(isPaused);
            Debug.Log($"UIEventChannel: ポーズ状態変更イベントを発生しました - {isPaused}");
        }
        
        /// <summary>
        /// ローディング状態変更イベントを発生
        /// </summary>
        public void RaiseLoadingStateChanged(bool isLoading)
        {
            OnLoadingStateChanged?.Invoke(isLoading);
            Debug.Log($"UIEventChannel: ローディング状態変更イベントを発生しました - {isLoading}");
        }
        
        /// <summary>
        /// ゲームオーバー状態変更イベントを発生
        /// </summary>
        public void RaiseGameOverStateChanged(bool isGameOver)
        {
            OnGameOverStateChanged?.Invoke(isGameOver);
            Debug.Log($"UIEventChannel: ゲームオーバー状態変更イベントを発生しました - {isGameOver}");
        }
        
        /// <summary>
        /// ゲームクリア状態変更イベントを発生
        /// </summary>
        public void RaiseGameClearStateChanged(bool isGameClear)
        {
            OnGameClearStateChanged?.Invoke(isGameClear);
            Debug.Log($"UIEventChannel: ゲームクリア状態変更イベントを発生しました - {isGameClear}");
        }
        
        /// <summary>
        /// メニュー状態変更イベントを発生
        /// </summary>
        public void RaiseMenuStateChanged(bool isMenuOpen)
        {
            OnMenuStateChanged?.Invoke(isMenuOpen);
            Debug.Log($"UIEventChannel: メニュー状態変更イベントを発生しました - {isMenuOpen}");
        }
        
        /// <summary>
        /// ローディング画面表示イベントを発生
        /// </summary>
        public void RaiseLoadingScreenShown()
        {
            OnLoadingScreenShown?.Invoke();
            Debug.Log("UIEventChannel: ローディング画面表示イベントを発生しました");
        }
        
        /// <summary>
        /// ローディング画面非表示イベントを発生
        /// </summary>
        public void RaiseLoadingScreenHidden()
        {
            OnLoadingScreenHidden?.Invoke();
            Debug.Log("UIEventChannel: ローディング画面非表示イベントを発生しました");
        }
        
        /// <summary>
        /// ポーズメニュー表示イベントを発生
        /// </summary>
        public void RaisePauseMenuShown()
        {
            OnPauseMenuShown?.Invoke();
            Debug.Log("UIEventChannel: ポーズメニュー表示イベントを発生しました");
        }
        
        /// <summary>
        /// ポーズメニュー非表示イベントを発生
        /// </summary>
        public void RaisePauseMenuHidden()
        {
            OnPauseMenuHidden?.Invoke();
            Debug.Log("UIEventChannel: ポーズメニュー非表示イベントを発生しました");
        }
        
        /// <summary>
        /// ゲームオーバー画面表示イベントを発生
        /// </summary>
        public void RaiseGameOverScreenShown()
        {
            OnGameOverScreenShown?.Invoke();
            Debug.Log("UIEventChannel: ゲームオーバー画面表示イベントを発生しました");
        }
        
        /// <summary>
        /// ゲームクリア画面表示イベントを発生
        /// </summary>
        public void RaiseGameClearScreenShown()
        {
            OnGameClearScreenShown?.Invoke();
            Debug.Log("UIEventChannel: ゲームクリア画面表示イベントを発生しました");
        }
        
        /// <summary>
        /// カードUI作成イベントを発生
        /// </summary>
        public void RaiseCardUICreated(CardDataSO cardData)
        {
            OnCardUICreated?.Invoke(cardData);
            Debug.Log($"UIEventChannel: カードUI作成イベントを発生しました - {cardData?.cardName ?? "Unknown"}");
        }
        
        /// <summary>
        /// カードUI削除イベントを発生
        /// </summary>
        public void RaiseCardUIRemoved()
        {
            OnCardUIRemoved?.Invoke();
            Debug.Log("UIEventChannel: カードUI削除イベントを発生しました");
        }
        
        /// <summary>
        /// すべてのカードUIクリアイベントを発生
        /// </summary>
        public void RaiseAllCardUICleared()
        {
            OnAllCardUICleared?.Invoke();
            Debug.Log("UIEventChannel: すべてのカードUIクリアイベントを発生しました");
        }
        
        /// <summary>
        /// カード数変更イベントを発生
        /// </summary>
        public void RaiseCardCountChanged(int cardCount)
        {
            OnCardCountChanged?.Invoke(cardCount);
            Debug.Log($"UIEventChannel: カード数変更イベントを発生しました - {cardCount}");
        }
        
        /// <summary>
        /// UIスケール変更イベントを発生
        /// </summary>
        public void RaiseUIScaleChanged(float scale)
        {
            OnUIScaleChanged?.Invoke(scale);
            Debug.Log($"UIEventChannel: UIスケール変更イベントを発生しました - {scale}");
        }
        
        /// <summary>
        /// アニメーション有効化変更イベントを発生
        /// </summary>
        public void RaiseAnimationEnabledChanged(bool enabled)
        {
            OnAnimationEnabledChanged?.Invoke(enabled);
            Debug.Log($"UIEventChannel: アニメーション有効化変更イベントを発生しました - {enabled}");
        }
        
        /// <summary>
        /// HUD表示変更イベントを発生
        /// </summary>
        public void RaiseHUDVisibilityChanged(bool visible)
        {
            OnHUDVisibilityChanged?.Invoke(visible);
            Debug.Log($"UIEventChannel: HUD表示変更イベントを発生しました - {visible}");
        }
        
        /// <summary>
        /// ログ表示変更イベントを発生
        /// </summary>
        public void RaiseLogVisibilityChanged(bool visible)
        {
            OnLogVisibilityChanged?.Invoke(visible);
            Debug.Log($"UIEventChannel: ログ表示変更イベントを発生しました - {visible}");
        }
        
        /// <summary>
        /// カード表示変更イベントを発生
        /// </summary>
        public void RaiseCardVisibilityChanged(bool visible)
        {
            OnCardVisibilityChanged?.Invoke(visible);
            Debug.Log($"UIEventChannel: カード表示変更イベントを発生しました - {visible}");
        }
        
        /// <summary>
        /// UIシステムの情報を取得
        /// </summary>
        public string GetUIEventChannelInfo()
        {
            return $"UIEventChannel - UIDataChanged: {(OnUIDataChanged != null ? "✓" : "✗")}, " +
                   $"LogAdded: {(OnLogAdded != null ? "✓" : "✗")}, " +
                   $"HPChanged: {(OnHPChanged != null ? "✓" : "✗")}, " +
                   $"LevelChanged: {(OnLevelChanged != null ? "✓" : "✗")}, " +
                   $"ScoreChanged: {(OnScoreChanged != null ? "✓" : "✗")}, " +
                   $"FloorChanged: {(OnFloorChanged != null ? "✓" : "✗")}, " +
                   $"PauseStateChanged: {(OnPauseStateChanged != null ? "✓" : "✗")}, " +
                   $"LoadingStateChanged: {(OnLoadingStateChanged != null ? "✓" : "✗")}";
        }
    }
} 