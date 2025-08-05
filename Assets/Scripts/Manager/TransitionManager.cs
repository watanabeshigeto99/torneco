using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

[DefaultExecutionOrder(-50)]
public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance { get; private set; }
    
    [Header("Transition Settings")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI Settings")]
    [SerializeField] private Canvas transitionCanvas;
    [SerializeField] private bool disableInputDuringTransition = true;
    
    // 遷移状態
    private bool isTransitioning = false;
    private Coroutine currentTransitionCoroutine;
    
    // イベント
    public static event System.Action OnTransitionStarted;
    public static event System.Action OnTransitionCompleted;
    public static event System.Action<string> OnSceneLoaded;
    
    private void Awake()
    {
        Debug.Log("TransitionManager: Awake開始");
        
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("TransitionManager: 重複するTransitionManagerインスタンスを破棄");
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // DontDestroyOnLoadで永続化
        DontDestroyOnLoad(gameObject);
        
        // 初期化
        InitializeTransitionUI();
        
        Debug.Log("TransitionManager: Awake完了");
    }
    
    /// <summary>
    /// TransitionManagerインスタンスを取得（存在しない場合は作成）
    /// </summary>
    public static TransitionManager GetOrCreateInstance()
    {
        if (Instance == null)
        {
            Debug.Log("TransitionManager: インスタンスが存在しないため、新しく作成します");
            
            // シーン内でTransitionManagerを探す
            TransitionManager existingManager = FindObjectOfType<TransitionManager>();
            if (existingManager != null)
            {
                Instance = existingManager;
                Debug.Log("TransitionManager: 既存のTransitionManagerを見つけました");
            }
            else
            {
                // 新しいTransitionManagerを作成
                GameObject transitionManagerObj = new GameObject("TransitionManager");
                Instance = transitionManagerObj.AddComponent<TransitionManager>();
                Debug.Log("TransitionManager: 新しいTransitionManagerを作成しました");
            }
        }
        
        return Instance;
    }
    
    /// <summary>
    /// フェード演出付きでシーンをロード
    /// </summary>
    /// <param name="sceneName">ロードするシーン名</param>
    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning)
        {
            Debug.LogWarning($"TransitionManager: 既に遷移中のため、{sceneName}への遷移をスキップします");
            return;
        }
        
        // シーンが存在するかチェック
        if (!SceneExists(sceneName))
        {
            Debug.LogError($"TransitionManager: シーン '{sceneName}' がビルド設定に含まれていません。");
            return;
        }
        
        Debug.Log($"TransitionManager: {sceneName}への遷移を開始");
        StartCoroutine(TransitionRoutine(sceneName));
    }
    
    /// <summary>
    /// フェード演出付きでシーンをロード（ビルドインデックス版）
    /// </summary>
    /// <param name="sceneBuildIndex">ロードするシーンのビルドインデックス</param>
    public void LoadSceneWithFade(int sceneBuildIndex)
    {
        if (isTransitioning)
        {
            Debug.LogWarning($"TransitionManager: 既に遷移中のため、ビルドインデックス{sceneBuildIndex}への遷移をスキップします");
            return;
        }
        
        string sceneName = SceneUtility.GetScenePathByBuildIndex(sceneBuildIndex);
        sceneName = System.IO.Path.GetFileNameWithoutExtension(sceneName);
        
        Debug.Log($"TransitionManager: {sceneName}（ビルドインデックス{sceneBuildIndex}）への遷移を開始");
        StartCoroutine(TransitionRoutine(sceneName));
    }
    
    /// <summary>
    /// 遷移ルーチン
    /// </summary>
    private IEnumerator TransitionRoutine(string sceneName)
    {
        isTransitioning = true;
        OnTransitionStarted?.Invoke();
        
        // 入力無効化
        if (disableInputDuringTransition)
        {
            DisableInput();
        }
        
        // フェードイン
        yield return StartCoroutine(FadeInRoutine());
        
        // シーンロード
        yield return StartCoroutine(LoadSceneAsyncRoutine(sceneName));
        
        // フェードアウト
        yield return StartCoroutine(FadeOutRoutine());
        
        // 入力有効化
        if (disableInputDuringTransition)
        {
            EnableInput();
        }
        
        isTransitioning = false;
        OnTransitionCompleted?.Invoke();
        
        Debug.Log($"TransitionManager: {sceneName}への遷移完了");
    }
    
    /// <summary>
    /// フェードイン処理
    /// </summary>
    private IEnumerator FadeInRoutine()
    {
        Debug.Log("TransitionManager: フェードイン開始");
        
        float elapsedTime = 0f;
        float startAlpha = 0f;
        float targetAlpha = 1f;
        
        SetFadeAlpha(startAlpha);
        ShowTransitionUI();
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            
            SetFadeAlpha(currentAlpha);
            yield return null;
        }
        
        SetFadeAlpha(targetAlpha);
        Debug.Log("TransitionManager: フェードイン完了");
    }
    
    /// <summary>
    /// フェードアウト処理
    /// </summary>
    private IEnumerator FadeOutRoutine()
    {
        Debug.Log("TransitionManager: フェードアウト開始");
        
        float elapsedTime = 0f;
        float startAlpha = 1f;
        float targetAlpha = 0f;
        
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float normalizedTime = elapsedTime / fadeDuration;
            float curveValue = fadeCurve.Evaluate(normalizedTime);
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, curveValue);
            
            SetFadeAlpha(currentAlpha);
            yield return null;
        }
        
        SetFadeAlpha(targetAlpha);
        HideTransitionUI();
        Debug.Log("TransitionManager: フェードアウト完了");
    }
    
    /// <summary>
    /// 非同期シーンロード処理
    /// </summary>
    private IEnumerator LoadSceneAsyncRoutine(string sceneName)
    {
        Debug.Log($"TransitionManager: {sceneName}の非同期ロード開始");
        
        // 現在のシーンのUIや状態をクリーンアップ
        CleanupCurrentScene();
        
        // シーンロード開始（allowSceneActivation = falseで即座にアクティブ化しない）
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // シーンが存在しない場合のエラーハンドリング
        if (asyncLoad == null)
        {
            Debug.LogError($"TransitionManager: シーン '{sceneName}' が見つかりません。ビルド設定に追加されているか確認してください。");
            yield break;
        }
        
        asyncLoad.allowSceneActivation = false;
        
        // ロード進捗を監視
        while (asyncLoad.progress < 0.9f)
        {
            Debug.Log($"TransitionManager: ロード進捗 {asyncLoad.progress * 100:F1}%");
            yield return null;
        }
        
        // ロード完了後、明示的にアクティブ化
        asyncLoad.allowSceneActivation = true;
        
        // シーンアクティブ化完了まで待機
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        
        // 新しいシーンの初期化
        InitializeNewScene(sceneName);
        
        Debug.Log($"TransitionManager: {sceneName}のロード完了");
    }
    
    /// <summary>
    /// 現在のシーンのクリーンアップ
    /// </summary>
    private void CleanupCurrentScene()
    {
        Debug.Log("TransitionManager: 現在のシーンのクリーンアップ開始");
        
        // 現在のBGMを停止
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopBGM();
            Debug.Log("TransitionManager: 現在のBGMを停止しました");
        }
        
        // UIマネージャーのクリーンアップ
        if (UIManager.Instance != null)
        {
            UIManager.Instance.CleanupForSceneTransition();
        }
        
        // その他のマネージャークラスのクリーンアップ
        if (CardManager.Instance != null)
        {
            CardManager.Instance.CleanupForSceneTransition();
        }
        
        if (EnemyManager.Instance != null)
        {
            EnemyManager.Instance.CleanupForSceneTransition();
        }
        
        if (GridManager.Instance != null)
        {
            GridManager.Instance.CleanupForSceneTransition();
        }
        
        Debug.Log("TransitionManager: 現在のシーンのクリーンアップ完了");
    }
    
    /// <summary>
    /// 新しいシーンの初期化
    /// </summary>
    private void InitializeNewScene(string sceneName)
    {
        Debug.Log($"TransitionManager: {sceneName}の初期化開始");
        
        // BGM切り替え
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForScene(sceneName);
            Debug.Log($"TransitionManager: {sceneName}用のBGMに切り替えました");
        }
        else
        {
            Debug.LogWarning("TransitionManager: SoundManagerが見つかりません");
        }
        
        // シーン名に応じた初期化処理
        switch (sceneName)
        {
            case "MainScene":
                InitializeMainScene();
                break;
            case "DeckBuilderScene":
                InitializeDeckBuilderScene();
                break;
            default:
                Debug.Log($"TransitionManager: {sceneName}の初期化処理は未定義です");
                break;
        }
        
        OnSceneLoaded?.Invoke(sceneName);
        Debug.Log($"TransitionManager: {sceneName}の初期化完了");
    }
    
    /// <summary>
    /// メインシーンの初期化
    /// </summary>
    private void InitializeMainScene()
    {
        Debug.Log("TransitionManager: メインシーンの初期化");
        
        // ゲームマネージャーの初期化
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeForMainScene();
        }
        
        // グリッドマネージャーの初期化
        if (GridManager.Instance != null)
        {
            GridManager.Instance.InitializeForMainScene();
        }
        
        // UIマネージャーの初期化
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitializeForMainScene();
        }
    }
    
    /// <summary>
    /// デッキビルダーシーンの初期化
    /// </summary>
    private void InitializeDeckBuilderScene()
    {
        Debug.Log("TransitionManager: デッキビルダーシーンの初期化");
        
        // ゲームマネージャーの初期化
        if (GameManager.Instance != null)
        {
            GameManager.Instance.InitializeForDeckBuilderScene();
        }
        
        // デッキビルダーUIの初期化
        if (UIManager.Instance != null)
        {
            UIManager.Instance.InitializeForDeckBuilderScene();
        }
        
        // DeckBuilderUIの初期化はOnSceneLoadedイベントで行われる
        Debug.Log("TransitionManager: DeckBuilderUIの初期化はイベントで実行されます");
    }
    
    /// <summary>
    /// 遷移UIの初期化
    /// </summary>
    private void InitializeTransitionUI()
    {
        // 遷移用Canvasが設定されていない場合は作成
        if (transitionCanvas == null)
        {
            CreateTransitionCanvas();
        }
        
        // フェード用CanvasGroupが設定されていない場合は作成
        if (fadeCanvasGroup == null)
        {
            CreateFadeCanvasGroup();
        }
        
        // 初期状態では非表示
        HideTransitionUI();
    }
    
    /// <summary>
    /// 遷移用Canvasの作成
    /// </summary>
    private void CreateTransitionCanvas()
    {
        GameObject canvasObj = new GameObject("TransitionCanvas");
        canvasObj.transform.SetParent(transform);
        
        transitionCanvas = canvasObj.AddComponent<Canvas>();
        transitionCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        transitionCanvas.sortingOrder = 9999; // 最前面に表示
        
        // CanvasScalerを追加
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // GraphicRaycasterを追加
        canvasObj.AddComponent<GraphicRaycaster>();
        
        Debug.Log("TransitionManager: 遷移用Canvasを作成しました");
    }
    
    /// <summary>
    /// フェード用CanvasGroupの作成
    /// </summary>
    private void CreateFadeCanvasGroup()
    {
        if (transitionCanvas == null)
        {
            Debug.LogError("TransitionManager: 遷移用Canvasが存在しません");
            return;
        }
        
        GameObject fadeObj = new GameObject("FadePanel");
        fadeObj.transform.SetParent(transitionCanvas.transform);
        
        // Imageコンポーネントを追加
        Image fadeImage = fadeObj.AddComponent<Image>();
        fadeImage.color = Color.black;
        
        // RectTransformを設定
        RectTransform rectTransform = fadeObj.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // CanvasGroupを追加
        fadeCanvasGroup = fadeObj.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        
        Debug.Log("TransitionManager: フェード用CanvasGroupを作成しました");
    }
    
    /// <summary>
    /// フェードアルファ値の設定
    /// </summary>
    private void SetFadeAlpha(float alpha)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = alpha;
        }
    }
    
    /// <summary>
    /// 遷移UIの表示
    /// </summary>
    private void ShowTransitionUI()
    {
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(true);
        }
    }
    
    /// <summary>
    /// 遷移UIの非表示
    /// </summary>
    private void HideTransitionUI()
    {
        if (transitionCanvas != null)
        {
            transitionCanvas.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 入力の無効化
    /// </summary>
    private void DisableInput()
    {
        // 必要に応じて入力システムを無効化
        Debug.Log("TransitionManager: 入力を無効化しました");
    }
    
    /// <summary>
    /// 入力の有効化
    /// </summary>
    private void EnableInput()
    {
        // 必要に応じて入力システムを有効化
        Debug.Log("TransitionManager: 入力を有効化しました");
    }
    
    /// <summary>
    /// 遷移中かどうかを取得
    /// </summary>
    public bool IsTransitioning => isTransitioning;
    
    /// <summary>
    /// シーンが存在するかチェック
    /// </summary>
    /// <param name="sceneName">チェックするシーン名</param>
    /// <returns>シーンが存在する場合はtrue</returns>
    private bool SceneExists(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            string sceneNameFromPath = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            if (sceneNameFromPath == sceneName)
            {
                return true;
            }
        }
        return false;
    }
    
    /// <summary>
    /// 現在の遷移をキャンセル
    /// </summary>
    public void CancelTransition()
    {
        if (currentTransitionCoroutine != null)
        {
            StopCoroutine(currentTransitionCoroutine);
            currentTransitionCoroutine = null;
        }
        
        isTransitioning = false;
        HideTransitionUI();
        EnableInput();
        
        Debug.Log("TransitionManager: 遷移をキャンセルしました");
    }
} 