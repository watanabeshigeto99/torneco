using UnityEngine;
using System;

namespace FloorSystem
{
    /// <summary>
    /// 階層データ管理用ScriptableObject
    /// 責務：階層データの状態管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "FloorData", menuName = "Floor System/Floor Data")]
    public class FloorDataSO : ScriptableObject
    {
        [Header("Floor Settings")]
        public int currentFloor = 1;
        public int maxFloor = 10;
        public int minFloor = 1;
        
        [Header("Floor Progress")]
        public bool isFloorCompleted = false;
        public bool isGameClear = false;
        public bool isGameOver = false;
        
        [Header("Floor Statistics")]
        public int totalFloorsCompleted = 0;
        public int totalTimeSpent = 0;
        public float averageTimePerFloor = 0f;
        
        [Header("Floor Difficulty")]
        public float difficultyMultiplier = 1.0f;
        public int enemyCountMultiplier = 1;
        public float enemyHealthMultiplier = 1.0f;
        
        // イベント
        public event Action<FloorDataSO> OnDataChanged;
        public event Action<int> OnFloorChanged;
        public event Action<int> OnFloorCompleted;
        public event Action OnGameClear;
        public event Action OnGameOver;
        
        /// <summary>
        /// 階層データの初期化
        /// </summary>
        public void Initialize()
        {
            currentFloor = minFloor;
            maxFloor = 10;
            isFloorCompleted = false;
            isGameClear = false;
            isGameOver = false;
            totalFloorsCompleted = 0;
            totalTimeSpent = 0;
            averageTimePerFloor = 0f;
            difficultyMultiplier = 1.0f;
            enemyCountMultiplier = 1;
            enemyHealthMultiplier = 1.0f;
            
            Debug.Log("FloorDataSO: 階層データを初期化しました");
        }
        
        /// <summary>
        /// 次の階層に進む
        /// </summary>
        public void AdvanceToNextFloor()
        {
            if (isGameClear || isGameOver) return;
            
            currentFloor++;
            
            if (currentFloor > maxFloor)
            {
                SetGameClear();
                return;
            }
            
            // 難易度の調整
            UpdateDifficulty();
            
            OnFloorChanged?.Invoke(currentFloor);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"FloorDataSO: 階層 {currentFloor} に進みました");
        }
        
        /// <summary>
        /// 現在の階層を完了する
        /// </summary>
        public void CompleteCurrentFloor()
        {
            if (isFloorCompleted) return;
            
            isFloorCompleted = true;
            totalFloorsCompleted++;
            
            OnFloorCompleted?.Invoke(currentFloor);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"FloorDataSO: 階層 {currentFloor} を完了しました");
        }
        
        /// <summary>
        /// 階層を設定する
        /// </summary>
        public void SetFloor(int floorNumber)
        {
            if (floorNumber < minFloor || floorNumber > maxFloor) return;
            
            currentFloor = floorNumber;
            isFloorCompleted = false;
            
            // 難易度の調整
            UpdateDifficulty();
            
            OnFloorChanged?.Invoke(currentFloor);
            OnDataChanged?.Invoke(this);
            
            Debug.Log($"FloorDataSO: 階層を {currentFloor} に設定しました");
        }
        
        /// <summary>
        /// ゲームクリアを設定
        /// </summary>
        public void SetGameClear()
        {
            isGameClear = true;
            OnGameClear?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log("FloorDataSO: ゲームクリア！");
        }
        
        /// <summary>
        /// ゲームオーバーを設定
        /// </summary>
        public void SetGameOver()
        {
            isGameOver = true;
            OnGameOver?.Invoke();
            OnDataChanged?.Invoke(this);
            
            Debug.Log("FloorDataSO: ゲームオーバー！");
        }
        
        /// <summary>
        /// 階層完了状態をリセット
        /// </summary>
        public void ResetFloorCompletion()
        {
            isFloorCompleted = false;
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// 難易度を更新
        /// </summary>
        private void UpdateDifficulty()
        {
            // 階層に応じて難易度を調整
            difficultyMultiplier = 1.0f + (currentFloor - 1) * 0.1f;
            enemyCountMultiplier = 1 + (currentFloor - 1) / 3;
            enemyHealthMultiplier = 1.0f + (currentFloor - 1) * 0.05f;
            
            Debug.Log($"FloorDataSO: 難易度を更新しました - 階層{currentFloor}, 難易度{difficultyMultiplier:F2}");
        }
        
        /// <summary>
        /// 時間を追加
        /// </summary>
        public void AddTime(int seconds)
        {
            totalTimeSpent += seconds;
            
            if (totalFloorsCompleted > 0)
            {
                averageTimePerFloor = (float)totalTimeSpent / totalFloorsCompleted;
            }
            
            OnDataChanged?.Invoke(this);
        }
        
        /// <summary>
        /// ゲームクリアかチェック
        /// </summary>
        public bool IsGameClear()
        {
            return isGameClear || currentFloor > maxFloor;
        }
        
        /// <summary>
        /// ゲームオーバーかチェック
        /// </summary>
        public bool IsGameOver()
        {
            return isGameOver;
        }
        
        /// <summary>
        /// 階層完了かチェック
        /// </summary>
        public bool IsFloorCompleted()
        {
            return isFloorCompleted;
        }
        
        /// <summary>
        /// 最終階層かチェック
        /// </summary>
        public bool IsFinalFloor()
        {
            return currentFloor >= maxFloor;
        }
        
        /// <summary>
        /// 最初の階層かチェック
        /// </summary>
        public bool IsFirstFloor()
        {
            return currentFloor <= minFloor;
        }
        
        /// <summary>
        /// 階層の進捗率を取得
        /// </summary>
        public float GetFloorProgress()
        {
            return (float)(currentFloor - minFloor) / (maxFloor - minFloor);
        }
        
        /// <summary>
        /// 残り階層数を取得
        /// </summary>
        public int GetRemainingFloors()
        {
            return Mathf.Max(0, maxFloor - currentFloor);
        }
        
        /// <summary>
        /// 階層データの情報を取得
        /// </summary>
        public string GetFloorInfo()
        {
            return $"Floor: {currentFloor}/{maxFloor}, Completed: {isFloorCompleted}, " +
                   $"Total Completed: {totalFloorsCompleted}, Progress: {GetFloorProgress():P1}";
        }
        
        /// <summary>
        /// 階層データの詳細情報を取得
        /// </summary>
        public string GetDetailedInfo()
        {
            return $"=== Floor Data ===\n" +
                   $"Current Floor: {currentFloor}/{maxFloor}\n" +
                   $"Floor Completed: {isFloorCompleted}\n" +
                   $"Game Clear: {isGameClear}\n" +
                   $"Game Over: {isGameOver}\n" +
                   $"Total Floors Completed: {totalFloorsCompleted}\n" +
                   $"Total Time Spent: {totalTimeSpent}s\n" +
                   $"Average Time Per Floor: {averageTimePerFloor:F1}s\n" +
                   $"Difficulty Multiplier: {difficultyMultiplier:F2}\n" +
                   $"Enemy Count Multiplier: {enemyCountMultiplier}\n" +
                   $"Enemy Health Multiplier: {enemyHealthMultiplier:F2}\n" +
                   $"Progress: {GetFloorProgress():P1}\n" +
                   $"Remaining Floors: {GetRemainingFloors()}";
        }
    }
} 