using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

namespace SaveSystem
{
    /// <summary>
    /// セーブロード専用コンポーネント
    /// 責務：セーブ・ロード処理のみ
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class SaveLoader : MonoBehaviour
    {
        public static SaveLoader Instance { get; private set; }
        
        [Header("Save Settings")]
        public string saveFileName = "gamesave.json";
        public bool autoSave = true;
        public float autoSaveInterval = 60f; // 秒
        
        [Header("Event Channels")]
        public SaveEventChannel saveEventChannel;
        
        // セーブロードイベント
        public static event Action<GameData> OnGameSaved;
        public static event Action<GameData> OnGameLoaded;
        public static event Action OnSaveFailed;
        public static event Action OnLoadFailed;
        
        private string saveFilePath;
        private float lastAutoSaveTime;
        
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
            // 自動セーブの開始
            if (autoSave)
            {
                lastAutoSaveTime = Time.time;
            }
        }
        
        private void Update()
        {
            // 自動セーブの処理
            if (autoSave && Time.time - lastAutoSaveTime >= autoSaveInterval)
            {
                AutoSave();
                lastAutoSaveTime = Time.time;
            }
        }
        
        /// <summary>
        /// セーブシステムの初期化
        /// </summary>
        private void InitializeSaveSystem()
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, saveFileName);
            Debug.Log($"SaveLoader: セーブファイルパスを設定しました - {saveFilePath}");
        }
        
        /// <summary>
        /// ゲームデータの保存
        /// </summary>
        public void SaveGame(GameData gameData = null)
        {
            if (gameData == null)
            {
                gameData = CollectCurrentGameData();
            }
            
            try
            {
                if (gameData == null)
                {
                    Debug.LogError("SaveLoader: gameDataがnullです");
                    OnSaveFailed?.Invoke();
                    return;
                }
                
                // セーブ時刻を記録
                gameData.saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                
                // JSONにシリアライズ
                string jsonData = JsonUtility.ToJson(gameData, true);
                
                // ファイルに書き込み
                File.WriteAllText(saveFilePath, jsonData);
                
                OnGameSaved?.Invoke(gameData);
                
                if (saveEventChannel != null)
                {
                    saveEventChannel.RaiseGameSaved(gameData);
                }
                
                Debug.Log($"SaveLoader: ゲームを保存しました - {saveFilePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveLoader: セーブに失敗しました - {e.Message}");
                OnSaveFailed?.Invoke();
                
                if (saveEventChannel != null)
                {
                    saveEventChannel.RaiseSaveFailed();
                }
            }
        }
        
        /// <summary>
        /// ゲームデータの読み込み
        /// </summary>
        public GameData LoadGame()
        {
            try
            {
                if (!File.Exists(saveFilePath))
                {
                    Debug.LogWarning("SaveLoader: セーブファイルが見つかりません");
                    return CreateNewGameData();
                }
                
                // ファイルから読み込み
                string jsonData = File.ReadAllText(saveFilePath);
                
                // JSONからデシリアライズ
                GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
                
                if (gameData == null)
                {
                    Debug.LogError("SaveLoader: セーブデータの解析に失敗しました");
                    OnLoadFailed?.Invoke();
                    return CreateNewGameData();
                }
                
                OnGameLoaded?.Invoke(gameData);
                
                if (saveEventChannel != null)
                {
                    saveEventChannel.RaiseGameLoaded(gameData);
                }
                
                Debug.Log($"SaveLoader: ゲームを読み込みました - {gameData.saveTimestamp}");
                return gameData;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveLoader: ロードに失敗しました - {e.Message}");
                OnLoadFailed?.Invoke();
                
                if (saveEventChannel != null)
                {
                    saveEventChannel.RaiseLoadFailed();
                }
                
                return CreateNewGameData();
            }
        }
        
        /// <summary>
        /// 新規ゲームデータの作成
        /// </summary>
        public GameData CreateNewGameData()
        {
            GameData newData = new GameData
            {
                playerLevel = 1,
                playerExp = 0,
                playerMaxHP = 20,
                playerCurrentHP = 20,
                currentFloor = 1,
                score = 0,
                saveTimestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            
            Debug.Log("SaveLoader: 新規ゲームデータを作成しました");
            return newData;
        }
        
        /// <summary>
        /// 自動セーブ
        /// </summary>
        public void AutoSave()
        {
            GameData currentData = CollectCurrentGameData();
            SaveGame(currentData);
            Debug.Log("SaveLoader: 自動セーブを実行しました");
        }
        
        /// <summary>
        /// 現在のゲームデータを収集
        /// </summary>
        public GameData CollectCurrentGameData()
        {
            GameData data = new GameData();
            
            // GameManagerからデータを収集
            if (GameManager.Instance != null)
            {
                data.playerLevel = GameManager.Instance.playerLevel;
                data.playerExp = GameManager.Instance.playerExp;
                data.playerMaxHP = GameManager.Instance.playerMaxHP;
                data.playerCurrentHP = GameManager.Instance.playerCurrentHP;
                data.currentFloor = GameManager.Instance.currentFloor;
                data.score = GameManager.Instance.score;
            }
            
            // Playerからデータを収集
            if (Player.Instance != null)
            {
                data.playerLevel = Player.Instance.level;
                data.playerExp = Player.Instance.exp;
                data.playerMaxHP = Player.Instance.maxHP;
                data.playerCurrentHP = Player.Instance.currentHP;
            }
            
            return data;
        }
        
        /// <summary>
        /// セーブファイルの存在確認
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(saveFilePath);
        }
        
        /// <summary>
        /// セーブファイルの削除
        /// </summary>
        public void DeleteSaveFile()
        {
            try
            {
                if (File.Exists(saveFilePath))
                {
                    File.Delete(saveFilePath);
                    Debug.Log("SaveLoader: セーブファイルを削除しました");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveLoader: セーブファイルの削除に失敗しました - {e.Message}");
            }
        }
        
        /// <summary>
        /// セーブデータの情報取得
        /// </summary>
        public SaveInfo GetSaveInfo()
        {
            if (!HasSaveFile())
            {
                return null;
            }
            
            try
            {
                string jsonData = File.ReadAllText(saveFilePath);
                GameData gameData = JsonUtility.FromJson<GameData>(jsonData);
                
                return new SaveInfo
                {
                    saveTimestamp = gameData.saveTimestamp,
                    playerLevel = gameData.playerLevel,
                    currentFloor = gameData.currentFloor,
                    score = gameData.score
                };
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// セーブシステムの情報を取得
        /// </summary>
        public string GetSaveSystemInfo()
        {
            return $"SaveLoader - File: {saveFileName}, AutoSave: {(autoSave ? "ON" : "OFF")}, " +
                   $"Interval: {autoSaveInterval}s, HasSave: {HasSaveFile()}";
        }
    }
    
    /// <summary>
    /// ゲームデータ構造体
    /// </summary>
    [System.Serializable]
    public class GameData
    {
        [Header("Player Data")]
        public int playerLevel = 1;
        public int playerExp = 0;
        public int playerMaxHP = 20;
        public int playerCurrentHP = 20;
        
        [Header("Game Progress")]
        public int currentFloor = 1;
        public int score = 0;
        
        [Header("Save Info")]
        public string saveTimestamp = "";
    }
    
    /// <summary>
    /// セーブ情報構造体
    /// </summary>
    [System.Serializable]
    public class SaveInfo
    {
        public string saveTimestamp;
        public int playerLevel;
        public int currentFloor;
        public int score;
    }
} 