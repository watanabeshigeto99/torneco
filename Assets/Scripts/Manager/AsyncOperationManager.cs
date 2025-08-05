using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsyncOperationManager : MonoBehaviour
{
    public static AsyncOperationManager Instance { get; private set; }

    [System.Serializable]
    public class AsyncOperationRequest
    {
        public string operationId;
        public Func<IEnumerator> operation;
        public float timeout;
        public Action onSuccess;
        public Action<string> onFailure;
        public bool isPriority;
        public DateTime queueTime;
        public OperationStatus status;
        public string errorMessage;
        public float startTime;
        public float completionTime;
    }

    public enum OperationStatus
    {
        Queued,
        Running,
        Completed,
        Failed,
        TimedOut
    }

    private Queue<AsyncOperationRequest> operationQueue = new Queue<AsyncOperationRequest>();
    private Dictionary<string, AsyncOperationRequest> activeOperations = new Dictionary<string, AsyncOperationRequest>();
    private bool isProcessingQueue = false;
    private int totalOperationsCompleted = 0;
    private int totalOperationsFailed = 0;

    // Events
    public static event Action<string> OnOperationStarted;
    public static event Action<string> OnOperationCompleted;
    public static event Action<string, string> OnOperationFailed;
    public static event Action OnAllOperationsCompleted;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void QueueOperation(string operationId, Func<IEnumerator> operation, float timeout = -1, Action onSuccess = null, Action<string> failure = null, bool isPriority = false)
    {
        var request = new AsyncOperationRequest
        {
            operationId = operationId,
            operation = operation,
            timeout = timeout,
            onSuccess = onSuccess,
            onFailure = failure,
            isPriority = isPriority,
            queueTime = DateTime.Now,
            status = OperationStatus.Queued
        };

        if (isPriority)
        {
            // Insert at the front of the queue
            var tempQueue = new Queue<AsyncOperationRequest>();
            tempQueue.Enqueue(request);
            while (operationQueue.Count > 0)
            {
                tempQueue.Enqueue(operationQueue.Dequeue());
            }
            operationQueue = tempQueue;
        }
        else
        {
            operationQueue.Enqueue(request);
        }

        Debug.Log($"AsyncOperationManager: 操作をキューに追加 - {operationId}");

        if (!isProcessingQueue)
        {
            StartCoroutine(ProcessOperationQueue());
        }
    }

    private IEnumerator ProcessOperationQueue()
    {
        isProcessingQueue = true;
        while (operationQueue.Count > 0)
        {
            var request = operationQueue.Dequeue();
            request.status = OperationStatus.Running;
            request.startTime = Time.time;
            activeOperations[request.operationId] = request;

            OnOperationStarted?.Invoke(request.operationId);
            Debug.Log($"AsyncOperationManager: 操作開始 - {request.operationId}");

            // Start timeout monitoring if specified
            Coroutine timeoutCoroutine = null;
            if (request.timeout > 0)
            {
                timeoutCoroutine = StartCoroutine(MonitorTimeout(request));
            }

            // Execute the operation
            var operationCoroutine = StartCoroutine(ExecuteOperation(request));

            // Wait for operation to complete
            yield return operationCoroutine;

            // Wait for timeout coroutine if it exists
            if (timeoutCoroutine != null)
            {
                yield return timeoutCoroutine;
            }

            // Remove from active operations
            activeOperations.Remove(request.operationId);
        }
        isProcessingQueue = false;
        OnAllOperationsCompleted?.Invoke();
    }

    private IEnumerator ExecuteOperation(AsyncOperationRequest request)
    {
        request.status = OperationStatus.Running;
        request.startTime = Time.time;
        activeOperations[request.operationId] = request;

        OnOperationStarted?.Invoke(request.operationId);
        Debug.Log($"AsyncOperationManager: 操作開始 - {request.operationId}");

        // Start timeout monitoring
        var timeoutCoroutine = MonitorTimeout(request);
        StartCoroutine(timeoutCoroutine);

        // Execute the operation function
        var operationEnumerator = request.operation();
        bool operationCompleted = false;
        System.Exception operationException = null;

        // Execute the enumerator without try-catch to avoid CS1626
        while (operationEnumerator.MoveNext())
        {
            yield return operationEnumerator.Current;
        }

        // Operation completed successfully if we reach here
        operationCompleted = true;

        // Handle operation result
        if (operationCompleted)
        {
            // Operation completed successfully
            request.status = OperationStatus.Completed;
            request.completionTime = Time.time;
            totalOperationsCompleted++;

            OnOperationCompleted?.Invoke(request.operationId);
            request.onSuccess?.Invoke();

            Debug.Log($"AsyncOperationManager: 操作完了 - {request.operationId} (実行時間: {request.completionTime - request.startTime:F2}秒)");
        }
        else
        {
            // Operation failed
            request.status = OperationStatus.Failed;
            request.errorMessage = operationException?.Message ?? "Unknown error";
            totalOperationsFailed++;

            OnOperationFailed?.Invoke(request.operationId, request.errorMessage);
            request.onFailure?.Invoke(request.errorMessage);

            Debug.LogError($"AsyncOperationManager: 操作失敗 - {request.operationId}: {request.errorMessage}");
        }

        // Remove from active operations
        activeOperations.Remove(request.operationId);
    }

    private IEnumerator MonitorTimeout(AsyncOperationRequest request)
    {
        yield return new WaitForSeconds(request.timeout);

        if (request.status == OperationStatus.Running)
        {
            request.status = OperationStatus.TimedOut;
            request.errorMessage = "操作がタイムアウトしました";
            totalOperationsFailed++;

            OnOperationFailed?.Invoke(request.operationId, "タイムアウト");
            request.onFailure?.Invoke("タイムアウト");

            Debug.LogWarning($"AsyncOperationManager: 操作タイムアウト - {request.operationId}");
        }
    }

    public OperationStatus GetOperationStatus(string operationId)
    {
        if (activeOperations.TryGetValue(operationId, out var request))
        {
            return request.status;
        }

        // Check if it's in the queue
        foreach (var queuedRequest in operationQueue)
        {
            if (queuedRequest.operationId == operationId)
            {
                return OperationStatus.Queued;
            }
        }

        return OperationStatus.Completed; // Assume completed if not found
    }

    public bool IsOperationActive(string operationId)
    {
        return activeOperations.ContainsKey(operationId);
    }

    public int GetQueueLength()
    {
        return operationQueue.Count;
    }

    public int GetActiveOperationCount()
    {
        return activeOperations.Count;
    }

    public void ClearQueue()
    {
        operationQueue.Clear();
        Debug.Log("AsyncOperationManager: キューをクリアしました");
    }

    public void CancelOperation(string operationId)
    {
        if (activeOperations.TryGetValue(operationId, out var request))
        {
            request.status = OperationStatus.Failed;
            request.errorMessage = "操作がキャンセルされました";
            activeOperations.Remove(operationId);

            OnOperationFailed?.Invoke(operationId, "キャンセル");
            request.onFailure?.Invoke("キャンセル");

            Debug.Log($"AsyncOperationManager: 操作をキャンセル - {operationId}");
        }
    }

    public Dictionary<string, object> GetDebugInfo()
    {
        return new Dictionary<string, object>
        {
            { "QueueLength", operationQueue.Count },
            { "ActiveOperations", activeOperations.Count },
            { "TotalCompleted", totalOperationsCompleted },
            { "TotalFailed", totalOperationsFailed },
            { "IsProcessing", isProcessingQueue }
        };
    }

    public void ResetStatistics()
    {
        totalOperationsCompleted = 0;
        totalOperationsFailed = 0;
        Debug.Log("AsyncOperationManager: 統計をリセットしました");
    }
} 