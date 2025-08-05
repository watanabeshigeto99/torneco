using UnityEngine;
using UnityEngine.Events;

namespace FloorSystem
{
    /// <summary>
    /// 階層システム専用イベントチャンネル
    /// 責務：階層関連のイベント管理のみ
    /// </summary>
    [CreateAssetMenu(fileName = "FloorEventChannel", menuName = "Floor System/Floor Event Channel")]
    public class FloorEventChannel : ScriptableObject
    {
        // 階層変更イベント
        [Header("Floor Change Events")]
        public UnityEvent<int> OnFloorChanged;
        public UnityEvent<int> OnFloorCompleted;
        public UnityEvent OnGameClear;
        public UnityEvent OnGameOver;
        
        // 階層生成イベント
        [Header("Floor Generation Events")]
        public UnityEvent OnFloorGenerationStarted;
        public UnityEvent OnFloorGenerationCompleted;
        public UnityEvent OnFloorReset;
        
        // 階層統計イベント
        [Header("Floor Statistics Events")]
        public UnityEvent<int> OnTotalFloorsCompletedChanged;
        public UnityEvent<int> OnTotalTimeSpentChanged;
        public UnityEvent<float> OnAverageTimePerFloorChanged;
        
        // 階層難易度イベント
        [Header("Floor Difficulty Events")]
        public UnityEvent<float> OnDifficultyMultiplierChanged;
        public UnityEvent<int> OnEnemyCountMultiplierChanged;
        public UnityEvent<float> OnEnemyHealthMultiplierChanged;
        
        /// <summary>
        /// 階層変更イベントを発火
        /// </summary>
        public void RaiseFloorChanged(int newFloor)
        {
            OnFloorChanged?.Invoke(newFloor);
            Debug.Log($"[FloorEventChannel] Floor changed to {newFloor}");
        }
        
        /// <summary>
        /// 階層完了イベントを発火
        /// </summary>
        public void RaiseFloorCompleted(int completedFloor)
        {
            OnFloorCompleted?.Invoke(completedFloor);
            Debug.Log($"[FloorEventChannel] Floor {completedFloor} completed");
        }
        
        /// <summary>
        /// ゲームクリアイベントを発火
        /// </summary>
        public void RaiseGameClear()
        {
            OnGameClear?.Invoke();
            Debug.Log("[FloorEventChannel] Game clear!");
        }
        
        /// <summary>
        /// ゲームオーバーイベントを発火
        /// </summary>
        public void RaiseGameOver()
        {
            OnGameOver?.Invoke();
            Debug.Log("[FloorEventChannel] Game over!");
        }
        
        /// <summary>
        /// 階層生成開始イベントを発火
        /// </summary>
        public void RaiseFloorGenerationStarted()
        {
            OnFloorGenerationStarted?.Invoke();
            Debug.Log("[FloorEventChannel] Floor generation started");
        }
        
        /// <summary>
        /// 階層生成完了イベントを発火
        /// </summary>
        public void RaiseFloorGenerationCompleted()
        {
            OnFloorGenerationCompleted?.Invoke();
            Debug.Log("[FloorEventChannel] Floor generation completed");
        }
        
        /// <summary>
        /// 階層リセットイベントを発火
        /// </summary>
        public void RaiseFloorReset()
        {
            OnFloorReset?.Invoke();
            Debug.Log("[FloorEventChannel] Floor reset");
        }
        
        /// <summary>
        /// 総階層完了数変更イベントを発火
        /// </summary>
        public void RaiseTotalFloorsCompletedChanged(int totalCompleted)
        {
            OnTotalFloorsCompletedChanged?.Invoke(totalCompleted);
            Debug.Log($"[FloorEventChannel] Total floors completed changed to {totalCompleted}");
        }
        
        /// <summary>
        /// 総時間変更イベントを発火
        /// </summary>
        public void RaiseTotalTimeSpentChanged(int totalTime)
        {
            OnTotalTimeSpentChanged?.Invoke(totalTime);
            Debug.Log($"[FloorEventChannel] Total time spent changed to {totalTime}s");
        }
        
        /// <summary>
        /// 平均時間変更イベントを発火
        /// </summary>
        public void RaiseAverageTimePerFloorChanged(float averageTime)
        {
            OnAverageTimePerFloorChanged?.Invoke(averageTime);
            Debug.Log($"[FloorEventChannel] Average time per floor changed to {averageTime:F1}s");
        }
        
        /// <summary>
        /// 難易度倍率変更イベントを発火
        /// </summary>
        public void RaiseDifficultyMultiplierChanged(float multiplier)
        {
            OnDifficultyMultiplierChanged?.Invoke(multiplier);
            Debug.Log($"[FloorEventChannel] Difficulty multiplier changed to {multiplier:F2}");
        }
        
        /// <summary>
        /// 敵数倍率変更イベントを発火
        /// </summary>
        public void RaiseEnemyCountMultiplierChanged(int multiplier)
        {
            OnEnemyCountMultiplierChanged?.Invoke(multiplier);
            Debug.Log($"[FloorEventChannel] Enemy count multiplier changed to {multiplier}");
        }
        
        /// <summary>
        /// 敵HP倍率変更イベントを発火
        /// </summary>
        public void RaiseEnemyHealthMultiplierChanged(float multiplier)
        {
            OnEnemyHealthMultiplierChanged?.Invoke(multiplier);
            Debug.Log($"[FloorEventChannel] Enemy health multiplier changed to {multiplier:F2}");
        }
        
        /// <summary>
        /// イベントチャンネルの情報を取得
        /// </summary>
        public string GetEventChannelInfo()
        {
            return $"FloorEventChannel - Events: {OnFloorChanged?.GetPersistentEventCount() ?? 0} floor, " +
                   $"{OnFloorCompleted?.GetPersistentEventCount() ?? 0} completed, " +
                   $"{OnGameClear?.GetPersistentEventCount() ?? 0} clear listeners";
        }
    }
} 