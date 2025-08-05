using UnityEngine;
using System;

namespace FloorSystem
{
    /// <summary>
    /// 階層システム管理専用コンポーネント
    /// 責務：階層システムの管理のみ
    /// </summary>
    [DefaultExecutionOrder(-200)]
    public class FloorManager : MonoBehaviour
    {
        public static FloorManager Instance { get; private set; }
        
        [Header("Floor System Settings")]
        public FloorDataSO floorData;
        public FloorEventChannel floorEventChannel;
        
        // 階層システム変更イベント
        public static event Action<int> OnFloorChanged;
        public static event Action<int> OnFloorCompleted;
        public static event Action OnGameClear;
        public static event Action OnFloorGenerationStarted;
        public static event Action OnFloorGenerationCompleted;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeFloorSystem();
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }
        
        /// <summary>
        /// 階層システムの初期化
        /// </summary>
        private void InitializeFloorSystem()
        {
            if (floorData == null)
            {
                Debug.LogError("FloorManager: floorDataが設定されていません");
                return;
            }
            
            floorData.Initialize();
            Debug.Log("FloorManager: 階層システムを初期化しました");
        }
        
        /// <summary>
        /// イベントの購読
        /// </summary>
        private void SubscribeToEvents()
        {
            if (floorEventChannel != null)
            {
                floorEventChannel.OnFloorChanged.AddListener(OnFloorChangedHandler);
                floorEventChannel.OnFloorCompleted.AddListener(OnFloorCompletedHandler);
                floorEventChannel.OnGameClear.AddListener(OnGameClearHandler);
                floorEventChannel.OnFloorGenerationStarted.AddListener(OnFloorGenerationStartedHandler);
                floorEventChannel.OnFloorGenerationCompleted.AddListener(OnFloorGenerationCompletedHandler);
            }
        }
        
        /// <summary>
        /// 階層変更ハンドラー
        /// </summary>
        private void OnFloorChangedHandler(int newFloor)
        {
            OnFloorChanged?.Invoke(newFloor);
            Debug.Log($"FloorManager: 階層が変更されました - 階層{newFloor}");
        }
        
        /// <summary>
        /// 階層完了ハンドラー
        /// </summary>
        private void OnFloorCompletedHandler(int completedFloor)
        {
            OnFloorCompleted?.Invoke(completedFloor);
            Debug.Log($"FloorManager: 階層が完了しました - 階層{completedFloor}");
        }
        
        /// <summary>
        /// ゲームクリアハンドラー
        /// </summary>
        private void OnGameClearHandler()
        {
            OnGameClear?.Invoke();
            Debug.Log("FloorManager: ゲームクリア！");
        }
        
        /// <summary>
        /// 階層生成開始ハンドラー
        /// </summary>
        private void OnFloorGenerationStartedHandler()
        {
            OnFloorGenerationStarted?.Invoke();
            Debug.Log("FloorManager: 階層生成を開始しました");
        }
        
        /// <summary>
        /// 階層生成完了ハンドラー
        /// </summary>
        private void OnFloorGenerationCompletedHandler()
        {
            OnFloorGenerationCompleted?.Invoke();
            Debug.Log("FloorManager: 階層生成が完了しました");
        }
        
        /// <summary>
        /// 次の階層に進む
        /// </summary>
        public void GoToNextFloor()
        {
            if (floorData == null) return;
            
            if (floorData.IsGameClear())
            {
                if (floorEventChannel != null)
                {
                    floorEventChannel.RaiseGameClear();
                }
                return;
            }
            
            floorData.AdvanceToNextFloor();
            
            if (floorEventChannel != null)
            {
                floorEventChannel.RaiseFloorChanged(floorData.currentFloor);
            }
        }
        
        /// <summary>
        /// 階層を完了する
        /// </summary>
        public void CompleteCurrentFloor()
        {
            if (floorData == null) return;
            
            floorData.CompleteCurrentFloor();
            
            if (floorEventChannel != null)
            {
                floorEventChannel.RaiseFloorCompleted(floorData.currentFloor);
            }
        }
        
        /// <summary>
        /// 階層を設定する
        /// </summary>
        public void SetFloor(int floorNumber)
        {
            if (floorData == null) return;
            
            floorData.SetFloor(floorNumber);
            
            if (floorEventChannel != null)
            {
                floorEventChannel.RaiseFloorChanged(floorData.currentFloor);
            }
        }
        
        /// <summary>
        /// 階層データを取得
        /// </summary>
        public FloorDataSO GetFloorData()
        {
            return floorData;
        }
        
        /// <summary>
        /// 階層生成を開始
        /// </summary>
        public void StartFloorGeneration()
        {
            if (floorEventChannel != null)
            {
                floorEventChannel.RaiseFloorGenerationStarted();
            }
            
            // 実際の階層生成処理
            StartCoroutine(GenerateFloorCoroutine());
        }
        
        /// <summary>
        /// 階層生成コルーチン
        /// </summary>
        private System.Collections.IEnumerator GenerateFloorCoroutine()
        {
            if (GridManager.Instance != null)
            {
                GridManager.Instance.GenerateNewFloor();
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                Debug.LogError("FloorManager: GridManager.Instanceが見つかりません");
            }
            
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.RespawnEnemies();
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                Debug.LogError("FloorManager: EnemyManager.Instanceが見つかりません");
            }
            
            if (Player.Instance != null)
            {
                Player.Instance.ResetPlayerPosition();
                yield return new WaitForSeconds(0.2f);
            }
            else
            {
                Debug.LogError("FloorManager: Player.Instanceが見つかりません");
            }
            
            if (floorEventChannel != null)
            {
                floorEventChannel.RaiseFloorGenerationCompleted();
            }
            
            Debug.Log($"FloorManager: 階層 {floorData?.currentFloor ?? 0} の生成が完了しました");
        }
        
        /// <summary>
        /// デッキビルダーシーンに遷移
        /// </summary>
        public void GoToDeckBuilderScene()
        {
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"階層 {floorData?.currentFloor ?? 0} に進みます。デッキを再構築してください。");
            }
            
            if (TransitionManager.Instance != null)
            {
                TransitionManager.Instance.LoadSceneWithFade("DeckBuilderScene");
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            }
        }
        
        /// <summary>
        /// 階層システムの情報を取得
        /// </summary>
        public string GetFloorSystemInfo()
        {
            return $"FloorManager - Data: {(floorData != null ? "✓" : "✗")}, " +
                   $"EventChannel: {(floorEventChannel != null ? "✓" : "✗")}";
        }
    }
} 