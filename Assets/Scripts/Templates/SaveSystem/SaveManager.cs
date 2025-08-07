using UnityEngine;
using System;
using System.IO;

namespace SaveSystem
{
    /// <summary>
    /// セーブシステム管理専用コンポーネント
    /// 責務：セーブシステムの管理のみ
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }
        
        [Header("Save System Settings")]
        public SaveDataSO saveData;
        public SaveEventChannel saveEventChannel;
        
        [Header("Save Settings")]
        public string saveFileName = "gamesave.json";
        public string saveDirectory = "Saves";
        public bool autoSave = true;
        public float autoSaveInterval = 300f; // 5分
        
        private float lastAutoSaveTime;
        private string savePath;
        
        // セーブシステム変更イベント
        public static event Action<SaveDataSO> OnSaveCompleted;
        public static event Action<SaveDataSO> OnLoadCompleted;
        public static event Action<string> OnSaveFailed;
        public static event Action<string> OnLoadFailed;
        public static event Action OnAutoSaveTriggered;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeSaveSystem();
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void Update()
        {
            // 自動セーブの処理
            if (autoSave && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                AutoSave();
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// セーブシステムの初期化
        /// </summary>
        private void InitializeSaveSystem()
        {
            // セーブディレクトリの作成
            savePath = Path.Combine(Application.persistentDataPath, saveDirectory);
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            
            if (saveData == null)
            {
                Debug.LogError("SaveManager: saveDataが設定されていません");
                return;
            }
            
            saveData.Initialize();
            lastAutoSaveTime = Time.time;
            Debug.Log("SaveManager: セーブシステムを初期化しました");
        }

        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (saveEventChannel != null)
            {
                saveEventChannel.OnSaveRequested.AddListener(OnSaveRequested);
                saveEventChannel.OnLoadRequested.AddListener(OnLoadRequested);
                saveEventChannel.OnAutoSaveRequested.AddListener(OnAutoSaveRequested);
            }
        }

        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (saveEventChannel != null)
            {
                saveEventChannel.OnSaveRequested.RemoveListener(OnSaveRequested);
                saveEventChannel.OnLoadRequested.RemoveListener(OnLoadRequested);
                saveEventChannel.OnAutoSaveRequested.RemoveListener(OnAutoSaveRequested);
            }
        }

        // Event Handlers
        private void OnSaveRequested()
        {
            SaveGame();
        }

        private void OnLoadRequested()
        {
            LoadGame();
        }

        private void OnAutoSaveRequested()
        {
            AutoSave();
        }

        /// <summary>
        /// ゲームをセーブ
        /// </summary>
        public void SaveGame()
        {
            try
            {
                if (saveData == null)
                {
                    Debug.LogError("SaveManager: saveDataが設定されていません");
                    return;
                }

                // 現在のゲーム状態をセーブデータに反映
                CollectGameData();

                // JSONにシリアライズ
                string json = JsonUtility.ToJson(saveData, true);
                string filePath = Path.Combine(savePath, saveFileName);
                
                // ファイルに書き込み
                File.WriteAllText(filePath, json);
                
                Debug.Log($"SaveManager: ゲームをセーブしました - {filePath}");
                
                OnSaveCompleted?.Invoke(saveData);
                saveEventChannel?.SaveCompleted(saveData);
            }
            catch (Exception e)
            {
                string errorMessage = $"セーブに失敗しました: {e.Message}";
                Debug.LogError($"SaveManager: {errorMessage}");
                
                OnSaveFailed?.Invoke(errorMessage);
                saveEventChannel?.SaveFailed(errorMessage);
            }
        }

        /// <summary>
        /// ゲームをロード
        /// </summary>
        public void LoadGame()
        {
            try
            {
                if (saveData == null)
                {
                    Debug.LogError("SaveManager: saveDataが設定されていません");
                    return;
                }

                string filePath = Path.Combine(savePath, saveFileName);
                
                if (!File.Exists(filePath))
                {
                    string errorMessage = "セーブファイルが見つかりません";
                    Debug.LogWarning($"SaveManager: {errorMessage}");
                    
                    OnLoadFailed?.Invoke(errorMessage);
                    saveEventChannel?.LoadFailed(errorMessage);
                    return;
                }

                // ファイルから読み込み
                string json = File.ReadAllText(filePath);
                
                // JSONからデシリアライズ
                JsonUtility.FromJsonOverwrite(json, saveData);
                
                Debug.Log($"SaveManager: ゲームをロードしました - {filePath}");
                
                OnLoadCompleted?.Invoke(saveData);
                saveEventChannel?.LoadCompleted(saveData);
            }
            catch (Exception e)
            {
                string errorMessage = $"ロードに失敗しました: {e.Message}";
                Debug.LogError($"SaveManager: {errorMessage}");
                
                OnLoadFailed?.Invoke(errorMessage);
                saveEventChannel?.LoadFailed(errorMessage);
            }
        }

        /// <summary>
        /// 自動セーブ
        /// </summary>
        public void AutoSave()
        {
            if (!autoSave) return;
            
            Debug.Log("SaveManager: 自動セーブを実行します");
            
            OnAutoSaveTriggered?.Invoke();
            saveEventChannel?.AutoSaveTriggered();
            
            SaveGame();
            lastAutoSaveTime = Time.time;
        }

        /// <summary>
        /// ゲームデータを収集
        /// </summary>
        private void CollectGameData()
        {
            if (saveData == null) return;

            // GameManagerからデータを収集
            if (GameManager.Instance != null)
            {
                saveData.score = GameManager.Instance.score;
                saveData.currentFloor = GameManager.Instance.currentFloor;
                saveData.gameOver = GameManager.Instance.gameOver;
                saveData.gameClear = GameManager.Instance.gameClear;
            }
            
            // PlayerDataManagerからプレイヤーデータを収集
            if (PlayerDataManager.Instance != null)
            {
                saveData.playerData.playerLevel = PlayerDataManager.Instance.playerLevel;
                saveData.playerData.playerExp = PlayerDataManager.Instance.playerExp;
                saveData.playerData.playerExpToNext = PlayerDataManager.Instance.playerExpToNext;
                saveData.playerData.playerCurrentHP = PlayerDataManager.Instance.playerCurrentHP;
                saveData.playerData.playerMaxHP = PlayerDataManager.Instance.playerMaxHP;
            }

            // セーブ時刻を記録
            saveData.saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveData.saveVersion = "1.0.0";
        }

        /// <summary>
        /// セーブデータを適用
        /// </summary>
        public void ApplySaveData()
        {
            if (saveData == null) return;

            // GameManagerにデータを適用
            if (GameManager.Instance != null)
            {
                GameManager.Instance.score = saveData.score;
                GameManager.Instance.currentFloor = saveData.currentFloor;
                GameManager.Instance.gameOver = saveData.gameOver;
                GameManager.Instance.gameClear = saveData.gameClear;
            }
            
            // PlayerDataManagerにプレイヤーデータを適用
            if (PlayerDataManager.Instance != null)
            {
                PlayerDataManager.Instance.SetPlayerLevel(saveData.playerData.playerLevel);
                PlayerDataManager.Instance.SetPlayerHP(saveData.playerData.playerCurrentHP, saveData.playerData.playerMaxHP);
                // 経験値はPlayerDataManagerが自動的に管理するため、直接設定は不要
            }

            Debug.Log("SaveManager: セーブデータを適用しました");
        }

        /// <summary>
        /// セーブファイルの存在確認
        /// </summary>
        public bool HasSaveFile()
        {
            string filePath = Path.Combine(savePath, saveFileName);
            return File.Exists(filePath);
        }

        /// <summary>
        /// セーブファイルを削除
        /// </summary>
        public void DeleteSaveFile()
        {
            try
            {
                string filePath = Path.Combine(savePath, saveFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    Debug.Log("SaveManager: セーブファイルを削除しました");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveManager: セーブファイルの削除に失敗しました - {e.Message}");
            }
        }

        /// <summary>
        /// セーブデータを取得
        /// </summary>
        public SaveDataSO GetSaveData()
        {
            return saveData;
        }

        /// <summary>
        /// セーブシステムの情報を取得
        /// </summary>
        public string GetSaveSystemInfo()
        {
            return $"SaveManager - Data: {(saveData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(saveEventChannel != null ? "✓" : "✗")}, " +
                   $"AutoSave: {autoSave}, " +
                   $"HasSaveFile: {HasSaveFile()}";
        }
    }
} 