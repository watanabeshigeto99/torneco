using UnityEngine;
using UnityEngine.Events;

namespace SaveSystem
{
    /// <summary>
    /// セーブシステムのイベントチャンネル
    /// 責務：セーブシステム関連のイベント管理
    /// </summary>
    [CreateAssetMenu(fileName = "SaveEventChannel", menuName = "SaveSystem/SaveEventChannel")]
    public class SaveEventChannel : ScriptableObject
    {
        [Header("Save Events")]
        public UnityEvent OnSaveRequested = new UnityEvent();
        public UnityEvent OnLoadRequested = new UnityEvent();
        public UnityEvent OnAutoSaveRequested = new UnityEvent();
        
        [Header("Save Data Events")]
        public UnityEvent<SaveDataSO> OnSaveCompleted = new UnityEvent<SaveDataSO>();
        public UnityEvent<SaveDataSO> OnLoadCompleted = new UnityEvent<SaveDataSO>();
        public UnityEvent<string> OnSaveFailed = new UnityEvent<string>();
        public UnityEvent<string> OnLoadFailed = new UnityEvent<string>();
        public UnityEvent OnAutoSaveTriggered = new UnityEvent();

        /// <summary>
        /// セーブ要求を発火
        /// </summary>
        public void RequestSave()
        {
            OnSaveRequested?.Invoke();
        }

        /// <summary>
        /// ロード要求を発火
        /// </summary>
        public void RequestLoad()
        {
            OnLoadRequested?.Invoke();
        }

        /// <summary>
        /// 自動セーブ要求を発火
        /// </summary>
        public void RequestAutoSave()
        {
            OnAutoSaveRequested?.Invoke();
        }

        /// <summary>
        /// セーブ完了を発火
        /// </summary>
        /// <param name="saveData">セーブデータ</param>
        public void SaveCompleted(SaveDataSO saveData)
        {
            OnSaveCompleted?.Invoke(saveData);
        }

        /// <summary>
        /// ロード完了を発火
        /// </summary>
        /// <param name="saveData">セーブデータ</param>
        public void LoadCompleted(SaveDataSO saveData)
        {
            OnLoadCompleted?.Invoke(saveData);
        }

        /// <summary>
        /// セーブ失敗を発火
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        public void SaveFailed(string errorMessage)
        {
            OnSaveFailed?.Invoke(errorMessage);
        }

        /// <summary>
        /// ロード失敗を発火
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        public void LoadFailed(string errorMessage)
        {
            OnLoadFailed?.Invoke(errorMessage);
        }

        /// <summary>
        /// 自動セーブ実行を発火
        /// </summary>
        public void AutoSaveTriggered()
        {
            OnAutoSaveTriggered?.Invoke();
        }
    }
} 