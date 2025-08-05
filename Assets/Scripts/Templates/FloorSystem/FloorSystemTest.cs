using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace FloorSystem
{
    /// <summary>
    /// 階層システムテンプレートのテスト用コンポーネント
    /// 責務：階層システムのテストのみ
    /// </summary>
    public class FloorSystemTest : MonoBehaviour
    {
        [Header("Floor System Components")]
        public FloorManager floorManager;
        public FloorDataSO floorData;
        public FloorEventChannel floorEventChannel;
        
        [Header("UI Elements")]
        public Button nextFloorButton;
        public Button completeFloorButton;
        public Button setFloorButton;
        public Button resetButton;
        public Button generateFloorButton;
        public Button goToDeckBuilderButton;
        
        [Header("Status Display")]
        public TextMeshProUGUI statusText;
        public TextMeshProUGUI floorInfoText;
        public TextMeshProUGUI eventLogText;
        
        [Header("Input Fields")]
        public TMP_InputField floorNumberInput;
        public TMP_InputField timeInput;
        
        private string eventLog = "";
        private const int MAX_LOG_LINES = 10;
        
        private void Start()
        {
            InitializeUI();
            SubscribeToEvents();
            UpdateStatusDisplay();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
        
        /// <summary>
        /// UIの初期化
        /// </summary>
        private void InitializeUI()
        {
            if (nextFloorButton != null)
                nextFloorButton.onClick.AddListener(OnNextFloorClicked);
            
            if (completeFloorButton != null)
                completeFloorButton.onClick.AddListener(OnCompleteFloorClicked);
            
            if (setFloorButton != null)
                setFloorButton.onClick.AddListener(OnSetFloorClicked);
            
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
            
            if (generateFloorButton != null)
                generateFloorButton.onClick.AddListener(OnGenerateFloorClicked);
            
            if (goToDeckBuilderButton != null)
                goToDeckBuilderButton.onClick.AddListener(OnGoToDeckBuilderClicked);
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (floorEventChannel != null)
            {
                floorEventChannel.OnFloorChanged.AddListener(OnFloorChanged);
                floorEventChannel.OnFloorCompleted.AddListener(OnFloorCompleted);
                floorEventChannel.OnGameClear.AddListener(OnGameClear);
                floorEventChannel.OnGameOver.AddListener(OnGameOver);
                floorEventChannel.OnFloorGenerationStarted.AddListener(OnFloorGenerationStarted);
                floorEventChannel.OnFloorGenerationCompleted.AddListener(OnFloorGenerationCompleted);
                floorEventChannel.OnTotalFloorsCompletedChanged.AddListener(OnTotalFloorsCompletedChanged);
                floorEventChannel.OnTotalTimeSpentChanged.AddListener(OnTotalTimeSpentChanged);
                floorEventChannel.OnDifficultyMultiplierChanged.AddListener(OnDifficultyMultiplierChanged);
            }
        }
        
        /// <summary>
        /// イベントの購読解除
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (floorEventChannel != null)
            {
                floorEventChannel.OnFloorChanged.RemoveListener(OnFloorChanged);
                floorEventChannel.OnFloorCompleted.RemoveListener(OnFloorCompleted);
                floorEventChannel.OnGameClear.RemoveListener(OnGameClear);
                floorEventChannel.OnGameOver.RemoveListener(OnGameOver);
                floorEventChannel.OnFloorGenerationStarted.RemoveListener(OnFloorGenerationStarted);
                floorEventChannel.OnFloorGenerationCompleted.RemoveListener(OnFloorGenerationCompleted);
                floorEventChannel.OnTotalFloorsCompletedChanged.RemoveListener(OnTotalFloorsCompletedChanged);
                floorEventChannel.OnTotalTimeSpentChanged.RemoveListener(OnTotalTimeSpentChanged);
                floorEventChannel.OnDifficultyMultiplierChanged.RemoveListener(OnDifficultyMultiplierChanged);
            }
        }
        
        // UI Button Event Handlers
        private void OnNextFloorClicked()
        {
            if (floorManager != null)
            {
                floorManager.GoToNextFloor();
                AddToEventLog("Next Floor Button Clicked");
            }
        }
        
        private void OnCompleteFloorClicked()
        {
            if (floorManager != null)
            {
                floorManager.CompleteCurrentFloor();
                AddToEventLog("Complete Floor Button Clicked");
            }
        }
        
        private void OnSetFloorClicked()
        {
            if (floorManager != null && floorNumberInput != null)
            {
                if (int.TryParse(floorNumberInput.text, out int floorNumber))
                {
                    floorManager.SetFloor(floorNumber);
                    AddToEventLog($"Set Floor Button Clicked - Floor {floorNumber}");
                }
            }
        }
        
        private void OnResetClicked()
        {
            if (floorData != null)
            {
                floorData.Initialize();
                AddToEventLog("Reset Button Clicked");
                UpdateStatusDisplay();
            }
        }
        
        private void OnGenerateFloorClicked()
        {
            if (floorManager != null)
            {
                floorManager.StartFloorGeneration();
                AddToEventLog("Generate Floor Button Clicked");
            }
        }
        
        private void OnGoToDeckBuilderClicked()
        {
            if (floorManager != null)
            {
                floorManager.GoToDeckBuilderScene();
                AddToEventLog("Go To Deck Builder Button Clicked");
            }
        }
        
        // Event Handlers
        private void OnFloorChanged(int newFloor)
        {
            AddToEventLog($"Floor Changed: {newFloor}");
            UpdateStatusDisplay();
        }
        
        private void OnFloorCompleted(int completedFloor)
        {
            AddToEventLog($"Floor Completed: {completedFloor}");
            UpdateStatusDisplay();
        }
        
        private void OnGameClear()
        {
            AddToEventLog("Game Clear!");
            UpdateStatusDisplay();
        }
        
        private void OnGameOver()
        {
            AddToEventLog("Game Over!");
            UpdateStatusDisplay();
        }
        
        private void OnFloorGenerationStarted()
        {
            AddToEventLog("Floor Generation Started");
        }
        
        private void OnFloorGenerationCompleted()
        {
            AddToEventLog("Floor Generation Completed");
        }
        
        private void OnTotalFloorsCompletedChanged(int totalCompleted)
        {
            AddToEventLog($"Total Floors Completed: {totalCompleted}");
            UpdateStatusDisplay();
        }
        
        private void OnTotalTimeSpentChanged(int totalTime)
        {
            AddToEventLog($"Total Time Spent: {totalTime}s");
            UpdateStatusDisplay();
        }
        
        private void OnDifficultyMultiplierChanged(float multiplier)
        {
            AddToEventLog($"Difficulty Multiplier: {multiplier:F2}");
            UpdateStatusDisplay();
        }
        
        /// <summary>
        /// イベントログに追加
        /// </summary>
        private void AddToEventLog(string message)
        {
            eventLog = $"[{System.DateTime.Now:HH:mm:ss}] {message}\n{eventLog}";
            
            // 最大行数を制限
            string[] lines = eventLog.Split('\n');
            if (lines.Length > MAX_LOG_LINES)
            {
                eventLog = string.Join("\n", lines, 0, MAX_LOG_LINES);
            }
            
            if (eventLogText != null)
            {
                eventLogText.text = eventLog;
            }
        }
        
        /// <summary>
        /// ステータス表示を更新
        /// </summary>
        private void UpdateStatusDisplay()
        {
            if (statusText != null)
            {
                string status = "=== Floor System Status ===\n";
                
                if (floorManager != null)
                {
                    status += $"FloorManager: ✓\n";
                    status += floorManager.GetFloorSystemInfo() + "\n";
                }
                else
                {
                    status += $"FloorManager: ✗\n";
                }
                
                if (floorData != null)
                {
                    status += $"FloorData: ✓\n";
                    status += floorData.GetFloorInfo() + "\n";
                }
                else
                {
                    status += $"FloorData: ✗\n";
                }
                
                if (floorEventChannel != null)
                {
                    status += $"EventChannel: ✓\n";
                    status += floorEventChannel.GetEventChannelInfo() + "\n";
                }
                else
                {
                    status += $"EventChannel: ✗\n";
                }
                
                statusText.text = status;
            }
            
            if (floorInfoText != null && floorData != null)
            {
                floorInfoText.text = floorData.GetDetailedInfo();
            }
        }
        
        /// <summary>
        /// 時間を追加（テスト用）
        /// </summary>
        public void AddTime()
        {
            if (floorData != null && timeInput != null)
            {
                if (int.TryParse(timeInput.text, out int time))
                {
                    floorData.AddTime(time);
                    AddToEventLog($"Added Time: {time}s");
                }
            }
        }
        
        /// <summary>
        /// 階層システムの情報を取得
        /// </summary>
        public string GetFloorSystemTestInfo()
        {
            return $"FloorSystemTest - Manager: {(floorManager != null ? "✓" : "✗")}, " +
                   $"Data: {(floorData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(floorEventChannel != null ? "✓" : "✗")}";
        }
    }
} 