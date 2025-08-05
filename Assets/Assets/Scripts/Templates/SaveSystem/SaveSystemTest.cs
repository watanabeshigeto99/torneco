using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace SaveSystem
{
    public class SaveSystemTest : MonoBehaviour
    {
        [Header("Save System Components")]
        public SaveLoader saveLoader;
        public SaveEventChannel saveEventChannel;
        
        [Header("UI Elements")]
        public Button saveButton;
        public Button loadButton;
        public Button newGameButton;
        public Button deleteSaveButton;
        public Button autoSaveToggleButton;
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI logText;
        public TextMeshProUGUI saveInfoText;
        
        [Header("Test Data")]
        public string testPlayerName = "TestPlayer";
        public int testPlayerLevel = 1;
        public int testPlayerHP = 100;
        public int testPlayerMaxHP = 100;
        public int testFloor = 1;
        public int testScore = 0;

        private bool autoSaveEnabled = true;
        private string logHistory = "";

        private void Start()
        {
            SetupTestUI();
            SubscribeToEvents();
            ValidateTestSetup();
            UpdateStatusDisplay();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// テスト用UIの設定
        /// </summary>
        private void SetupTestUI()
        {
            if (saveButton != null)
                saveButton.onClick.AddListener(OnSaveClicked);
            
            if (loadButton != null)
                loadButton.onClick.AddListener(OnLoadClicked);
            
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGameClicked);
            
            if (deleteSaveButton != null)
                deleteSaveButton.onClick.AddListener(OnDeleteSaveClicked);
            
            if (autoSaveToggleButton != null)
                autoSaveToggleButton.onClick.AddListener(OnAutoSaveToggleClicked);
        }

        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (saveEventChannel != null)
            {
                saveEventChannel.OnGameSaved.AddListener(OnGameSaved);
                saveEventChannel.OnGameLoaded.AddListener(OnGameLoaded);
                saveEventChannel.OnSaveFailed.AddListener(OnSaveFailed);
                saveEventChannel.OnLoadFailed.AddListener(OnLoadFailed);
                saveEventChannel.OnAutoSave.AddListener(OnAutoSave);
                saveEventChannel.OnSaveFileDeleted.AddListener(OnSaveFileDeleted);
                saveEventChannel.OnSaveError.AddListener(OnSaveError);
                saveEventChannel.OnLoadError.AddListener(OnLoadError);
            }
        }

        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (saveEventChannel != null)
            {
                saveEventChannel.OnGameSaved.RemoveListener(OnGameSaved);
                saveEventChannel.OnGameLoaded.RemoveListener(OnGameLoaded);
                saveEventChannel.OnSaveFailed.RemoveListener(OnSaveFailed);
                saveEventChannel.OnLoadFailed.RemoveListener(OnLoadFailed);
                saveEventChannel.OnAutoSave.RemoveListener(OnAutoSave);
                saveEventChannel.OnSaveFileDeleted.RemoveListener(OnSaveFileDeleted);
                saveEventChannel.OnSaveError.RemoveListener(OnSaveError);
                saveEventChannel.OnLoadError.RemoveListener(OnLoadError);
            }
        }

        /// <summary>
        /// テスト設定の検証
        /// </summary>
        private void ValidateTestSetup()
        {
            if (saveLoader == null)
            {
                Debug.LogError("[SaveSystemTest] SaveLoader is not assigned!");
                AddLog("ERROR: SaveLoader is not assigned");
            }

            if (saveEventChannel == null)
            {
                Debug.LogError("[SaveSystemTest] SaveEventChannel is not assigned!");
                AddLog("ERROR: SaveEventChannel is not assigned");
            }

            if (statusText == null)
            {
                Debug.LogWarning("[SaveSystemTest] StatusText is not assigned");
            }

            if (logText == null)
            {
                Debug.LogWarning("[SaveSystemTest] LogText is not assigned");
            }
        }

        /// <summary>
        /// セーブボタンクリック時の処理
        /// </summary>
        private void OnSaveClicked()
        {
            AddLog("Save button clicked");
            if (saveLoader != null)
            {
                saveLoader.SaveGame();
            }
            else
            {
                AddLog("ERROR: SaveLoader is null");
            }
        }

        /// <summary>
        /// ロードボタンクリック時の処理
        /// </summary>
        private void OnLoadClicked()
        {
            AddLog("Load button clicked");
            if (saveLoader != null)
            {
                saveLoader.LoadGame();
            }
            else
            {
                AddLog("ERROR: SaveLoader is null");
            }
        }

        /// <summary>
        /// ニューゲームボタンクリック時の処理
        /// </summary>
        private void OnNewGameClicked()
        {
            AddLog("New game button clicked");
            if (saveLoader != null)
            {
                saveLoader.CreateNewGameData();
                AddLog("New game data created");
            }
            else
            {
                AddLog("ERROR: SaveLoader is null");
            }
        }

        /// <summary>
        /// セーブ削除ボタンクリック時の処理
        /// </summary>
        private void OnDeleteSaveClicked()
        {
            AddLog("Delete save button clicked");
            if (saveLoader != null)
            {
                saveLoader.DeleteSaveFile();
            }
            else
            {
                AddLog("ERROR: SaveLoader is null");
            }
        }

        /// <summary>
        /// オートセーブトグルボタンクリック時の処理
        /// </summary>
        private void OnAutoSaveToggleClicked()
        {
            autoSaveEnabled = !autoSaveEnabled;
            if (saveLoader != null)
            {
                saveLoader.autoSave = autoSaveEnabled;
            }
            AddLog($"Auto save toggled: {(autoSaveEnabled ? "ON" : "OFF")}");
            UpdateStatusDisplay();
        }

        // イベントハンドラー
        private void OnGameSaved(GameData gameData)
        {
            AddLog($"Game saved successfully: Level {gameData?.playerLevel ?? 0}");
            UpdateStatusDisplay();
        }

        private void OnGameLoaded(GameData gameData)
        {
            AddLog($"Game loaded successfully: Level {gameData?.playerLevel ?? 0}");
            UpdateStatusDisplay();
        }

        private void OnSaveFailed()
        {
            AddLog("Save failed");
            UpdateStatusDisplay();
        }

        private void OnLoadFailed()
        {
            AddLog("Load failed");
            UpdateStatusDisplay();
        }

        private void OnAutoSave()
        {
            AddLog("Auto save triggered");
        }

        private void OnSaveFileDeleted()
        {
            AddLog("Save file deleted");
            UpdateStatusDisplay();
        }

        private void OnSaveError(string errorMessage)
        {
            AddLog($"Save error: {errorMessage}");
        }

        private void OnLoadError(string errorMessage)
        {
            AddLog($"Load error: {errorMessage}");
        }

        /// <summary>
        /// ステータス表示の更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText == null) return;

            string status = "";
            
            if (saveLoader != null)
            {
                bool hasSave = saveLoader.HasSaveFile();
                status += $"Save File: {(hasSave ? "Exists" : "None")}\n";
                status += $"Auto Save: {(autoSaveEnabled ? "ON" : "OFF")}\n";
                
                if (hasSave)
                {
                    var saveInfo = saveLoader.GetSaveInfo();
                    if (saveInfo != null)
                    {
                        status += $"Last Save: {saveInfo.saveTimestamp}\n";
                        status += $"Level: {saveInfo.playerLevel}\n";
                        status += $"Floor: {saveInfo.currentFloor}\n";
                        status += $"Score: {saveInfo.score}\n";
                    }
                }
            }
            else
            {
                status = "SaveLoader not assigned";
            }

            statusText.text = status;
        }

        /// <summary>
        /// ログの追加
        /// </summary>
        private void AddLog(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            string logEntry = $"[{timestamp}] {message}\n";
            logHistory += logEntry;
            
            if (logText != null)
            {
                logText.text = logHistory;
            }
            
            Debug.Log($"[SaveSystemTest] {message}");
        }

        /// <summary>
        /// テストデータの生成
        /// </summary>
        public GameData CreateTestGameData()
        {
            var gameData = new GameData();
            
            // プレイヤーデータ
            gameData.playerLevel = testPlayerLevel;
            gameData.playerCurrentHP = testPlayerHP;
            gameData.playerMaxHP = testPlayerMaxHP;
            gameData.playerExp = 0;
            
            // フロアデータ
            gameData.currentFloor = testFloor;
            gameData.score = testScore;
            
            // セーブ情報
            gameData.saveTimestamp = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            return gameData;
        }

        /// <summary>
        /// テストデータでセーブを実行
        /// </summary>
        public void TestSaveWithData()
        {
            var testData = CreateTestGameData();
            AddLog($"Testing save with data: Level {testData.playerLevel}");
            
            if (saveLoader != null)
            {
                // テストデータをセーブローダーに設定（実際の実装では適切な方法で）
                saveLoader.SaveGame(testData);
            }
        }

        /// <summary>
        /// テスト設定の情報を取得
        /// </summary>
        public string GetTestInfo()
        {
            return $"SaveSystemTest - Components: {(saveLoader != null ? "SaveLoader ✓" : "SaveLoader ✗")}, " +
                   $"{(saveEventChannel != null ? "EventChannel ✓" : "EventChannel ✗")}, " +
                   $"AutoSave: {(autoSaveEnabled ? "ON" : "OFF")}";
        }
    }
} 