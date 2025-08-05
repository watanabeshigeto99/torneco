using UnityEngine;
using System;

namespace SaveSystem
{
    /// <summary>
    /// セーブデータ管理用ScriptableObject
    /// 責務：セーブデータの状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "SaveData", menuName = "Save System/Save Data")]
    public class SaveDataSO : ScriptableObject
    {
        [Header("Save Information")]
        public string saveDateTime = "";
        public string saveVersion = "1.0.0";
        public bool isNewGame = true;
        
        [Header("Game State")]
        public int score = 0;
        public int currentFloor = 1;
        public bool gameOver = false;
        public bool gameClear = false;
        
        [Header("Player Data")]
        public PlayerSaveData playerData = new PlayerSaveData();
        
        [Header("Deck Data")]
        public DeckSaveData deckData = new DeckSaveData();
        
        [Header("Floor Data")]
        public FloorSaveData floorData = new FloorSaveData();
        
        [Header("UI Data")]
        public UISaveData uiData = new UISaveData();
        
        [Header("Save Statistics")]
        public int totalSaveCount = 0;
        public int totalLoadCount = 0;
        public float totalPlayTime = 0f;
        public string lastSaveDateTime = "";
        
        // イベント
        public event Action<SaveDataSO> OnDataChanged;
        public event Action OnSaveDataInitialized;
        public event Action OnSaveDataLoaded;
        public event Action OnSaveDataSaved;

        /// <summary>
        /// セーブデータの初期化
        /// </summary>
        public void Initialize()
        {
            saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveVersion = "1.0.0";
            isNewGame = true;
            
            score = 0;
            currentFloor = 1;
            gameOver = false;
            gameClear = false;
            
            playerData.Initialize();
            deckData.Initialize();
            floorData.Initialize();
            uiData.Initialize();
            
            totalSaveCount = 0;
            totalLoadCount = 0;
            totalPlayTime = 0f;
            lastSaveDateTime = "";
            
            OnSaveDataInitialized?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log("SaveDataSO: セーブデータを初期化しました");
        }

        /// <summary>
        /// セーブデータを保存
        /// </summary>
        public void Save()
        {
            saveDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            lastSaveDateTime = saveDateTime;
            totalSaveCount++;
            isNewGame = false;
            
            OnSaveDataSaved?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: セーブデータを保存しました - {saveDateTime}");
        }

        /// <summary>
        /// セーブデータをロード
        /// </summary>
        public void Load()
        {
            totalLoadCount++;
            
            OnSaveDataLoaded?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: セーブデータをロードしました - {saveDateTime}");
        }

        /// <summary>
        /// ゲーム状態を設定
        /// </summary>
        public void SetGameState(int score, int currentFloor, bool gameOver, bool gameClear)
        {
            this.score = score;
            this.currentFloor = currentFloor;
            this.gameOver = gameOver;
            this.gameClear = gameClear;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: ゲーム状態を設定しました - Score: {score}, Floor: {currentFloor}");
        }

        /// <summary>
        /// プレイヤーデータを設定
        /// </summary>
        public void SetPlayerData(PlayerSaveData playerData)
        {
            this.playerData = playerData;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: プレイヤーデータを設定しました - Level: {playerData.playerLevel}");
        }

        /// <summary>
        /// デッキデータを設定
        /// </summary>
        public void SetDeckData(DeckSaveData deckData)
        {
            this.deckData = deckData;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: デッキデータを設定しました - DeckSize: {deckData.deckSize}");
        }

        /// <summary>
        /// 階層データを設定
        /// </summary>
        public void SetFloorData(FloorSaveData floorData)
        {
            this.floorData = floorData;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: 階層データを設定しました - CurrentFloor: {floorData.currentFloor}");
        }

        /// <summary>
        /// UIデータを設定
        /// </summary>
        public void SetUIData(UISaveData uiData)
        {
            this.uiData = uiData;
            
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"SaveDataSO: UIデータを設定しました");
        }

        /// <summary>
        /// プレイ時間を更新
        /// </summary>
        public void UpdatePlayTime(float playTime)
        {
            totalPlayTime = playTime;
            
            OnDataChanged?.Invoke(this);
        }

        /// <summary>
        /// セーブ情報を取得
        /// </summary>
        public string GetSaveInfo()
        {
            return $"Save: {saveDateTime}, Version: {saveVersion}, " +
                   $"Score: {score}, Floor: {currentFloor}, " +
                   $"Player Level: {playerData.playerLevel}, " +
                   $"Play Time: {totalPlayTime:F1}s";
        }

        /// <summary>
        /// 詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== Save Data ===\n" +
                   $"Save DateTime: {saveDateTime}\n" +
                   $"Save Version: {saveVersion}\n" +
                   $"Is New Game: {isNewGame}\n" +
                   $"Score: {score}\n" +
                   $"Current Floor: {currentFloor}\n" +
                   $"Game Over: {gameOver}\n" +
                   $"Game Clear: {gameClear}\n" +
                   $"Player Level: {playerData.playerLevel}\n" +
                   $"Player Exp: {playerData.playerExp}\n" +
                   $"Player HP: {playerData.playerCurrentHP}/{playerData.playerMaxHP}\n" +
                   $"Deck Size: {deckData.deckSize}\n" +
                   $"Hand Size: {deckData.handSize}\n" +
                   $"Total Save Count: {totalSaveCount}\n" +
                   $"Total Load Count: {totalLoadCount}\n" +
                   $"Total Play Time: {totalPlayTime:F1}s\n" +
                   $"Last Save DateTime: {lastSaveDateTime}";
        }
    }

    /// <summary>
    /// プレイヤーセーブデータ
    /// </summary>
    [System.Serializable]
    public class PlayerSaveData
    {
        public int playerLevel = 1;
        public int playerExp = 0;
        public int playerExpToNext = 10;
        public int playerCurrentHP = 20;
        public int playerMaxHP = 20;
        public int playerMaxLevel = 10;

        public void Initialize()
        {
            playerLevel = 1;
            playerExp = 0;
            playerExpToNext = 10;
            playerCurrentHP = 20;
            playerMaxHP = 20;
            playerMaxLevel = 10;
        }
    }

    /// <summary>
    /// デッキセーブデータ
    /// </summary>
    [System.Serializable]
    public class DeckSaveData
    {
        public int deckSize = 0;
        public int handSize = 0;
        public int discardSize = 0;
        public string[] cardNames = new string[0];

        public void Initialize()
        {
            deckSize = 0;
            handSize = 0;
            discardSize = 0;
            cardNames = new string[0];
        }
    }

    /// <summary>
    /// 階層セーブデータ
    /// </summary>
    [System.Serializable]
    public class FloorSaveData
    {
        public int currentFloor = 1;
        public int maxFloor = 10;
        public bool isFloorCleared = false;

        public void Initialize()
        {
            currentFloor = 1;
            maxFloor = 10;
            isFloorCleared = false;
        }
    }

    /// <summary>
    /// UIセーブデータ
    /// </summary>
    [System.Serializable]
    public class UISaveData
    {
        public bool showHUD = true;
        public bool showLog = true;
        public float uiScale = 1.0f;
        public string[] logMessages = new string[0];

        public void Initialize()
        {
            showHUD = true;
            showLog = true;
            uiScale = 1.0f;
            logMessages = new string[0];
        }
    }
} 