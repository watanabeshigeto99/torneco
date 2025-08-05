using UnityEngine;
using UnityEngine.Events;
using System;

namespace SaveSystem
{
    [CreateAssetMenu(fileName = "SaveEventChannel", menuName = "Save System/Save Event Channel")]
    public class SaveEventChannel : ScriptableObject
    {
        // セーブ関連イベント
        public UnityEvent<GameData> OnGameSaved;
        public UnityEvent<GameData> OnGameLoaded;
        public UnityEvent OnSaveFailed;
        public UnityEvent OnLoadFailed;
        public UnityEvent OnAutoSave;
        public UnityEvent OnSaveFileDeleted;
        
        // ゲームデータ変更イベント
        public UnityEvent<GameData> OnPlayerDataChanged;
        public UnityEvent<GameData> OnDeckDataChanged;
        public UnityEvent<GameData> OnFloorDataChanged;
        public UnityEvent<GameData> OnBattleDataChanged;
        
        // システムイベント
        public UnityEvent OnSaveSystemInitialized;
        public UnityEvent<string> OnSaveError;
        public UnityEvent<string> OnLoadError;

        /// <summary>
        /// ゲームセーブイベントを発火
        /// </summary>
        public void RaiseGameSaved(GameData gameData)
        {
            OnGameSaved?.Invoke(gameData);
            Debug.Log($"[SaveEventChannel] Game saved: Level {gameData?.playerLevel ?? 0}");
        }

        /// <summary>
        /// ゲームロードイベントを発火
        /// </summary>
        public void RaiseGameLoaded(GameData gameData)
        {
            OnGameLoaded?.Invoke(gameData);
            Debug.Log($"[SaveEventChannel] Game loaded: Level {gameData?.playerLevel ?? 0}");
        }

        /// <summary>
        /// セーブ失敗イベントを発火
        /// </summary>
        public void RaiseSaveFailed()
        {
            OnSaveFailed?.Invoke();
            Debug.LogWarning("[SaveEventChannel] Save failed");
        }

        /// <summary>
        /// ロード失敗イベントを発火
        /// </summary>
        public void RaiseLoadFailed()
        {
            OnLoadFailed?.Invoke();
            Debug.LogWarning("[SaveEventChannel] Load failed");
        }

        /// <summary>
        /// オートセーブイベントを発火
        /// </summary>
        public void RaiseAutoSave()
        {
            OnAutoSave?.Invoke();
            Debug.Log("[SaveEventChannel] Auto save triggered");
        }

        /// <summary>
        /// セーブファイル削除イベントを発火
        /// </summary>
        public void RaiseSaveFileDeleted()
        {
            OnSaveFileDeleted?.Invoke();
            Debug.Log("[SaveEventChannel] Save file deleted");
        }

        /// <summary>
        /// プレイヤーデータ変更イベントを発火
        /// </summary>
        public void RaisePlayerDataChanged(GameData gameData)
        {
            OnPlayerDataChanged?.Invoke(gameData);
            Debug.Log("[SaveEventChannel] Player data changed");
        }

        /// <summary>
        /// デッキデータ変更イベントを発火
        /// </summary>
        public void RaiseDeckDataChanged(GameData gameData)
        {
            OnDeckDataChanged?.Invoke(gameData);
            Debug.Log("[SaveEventChannel] Deck data changed");
        }

        /// <summary>
        /// フロアデータ変更イベントを発火
        /// </summary>
        public void RaiseFloorDataChanged(GameData gameData)
        {
            OnFloorDataChanged?.Invoke(gameData);
            Debug.Log("[SaveEventChannel] Floor data changed");
        }

        /// <summary>
        /// バトルデータ変更イベントを発火
        /// </summary>
        public void RaiseBattleDataChanged(GameData gameData)
        {
            OnBattleDataChanged?.Invoke(gameData);
            Debug.Log("[SaveEventChannel] Battle data changed");
        }

        /// <summary>
        /// セーブシステム初期化イベントを発火
        /// </summary>
        public void RaiseSaveSystemInitialized()
        {
            OnSaveSystemInitialized?.Invoke();
            Debug.Log("[SaveEventChannel] Save system initialized");
        }

        /// <summary>
        /// セーブエラーイベントを発火
        /// </summary>
        public void RaiseSaveError(string errorMessage)
        {
            OnSaveError?.Invoke(errorMessage);
            Debug.LogError($"[SaveEventChannel] Save error: {errorMessage}");
        }

        /// <summary>
        /// ロードエラーイベントを発火
        /// </summary>
        public void RaiseLoadError(string errorMessage)
        {
            OnLoadError?.Invoke(errorMessage);
            Debug.LogError($"[SaveEventChannel] Load error: {errorMessage}");
        }

        /// <summary>
        /// イベントチャンネルの情報を取得
        /// </summary>
        public string GetEventChannelInfo()
        {
            return $"SaveEventChannel - Events: {OnGameSaved?.GetPersistentEventCount() ?? 0} save, {OnGameLoaded?.GetPersistentEventCount() ?? 0} load listeners";
        }
    }
} 