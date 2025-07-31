using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEditor.SceneManagement;

public class DeckBuilderSceneSetup
{
    [MenuItem("Tools/Deck Builder/Setup Deck Builder Scene")]
    public static void SetupDeckBuilderScene()
    {
        // 新しいシーンを作成
        Scene scene = SceneManager.GetActiveScene();
        if (scene.name != "DeckBuilderScene")
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        }
        
        // メインカメラを設定
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
        }
        
        // Canvas作成
        GameObject canvasObj = new GameObject("DeckBuilderCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // EventSystem作成
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        
        // DeckBuilderUI作成
        GameObject deckBuilderObj = new GameObject("DeckBuilderUI");
        deckBuilderObj.transform.SetParent(canvasObj.transform);
        DeckBuilderUI deckBuilderUI = deckBuilderObj.AddComponent<DeckBuilderUI>();
        
        // カードデータベースを自動設定
        CardDatabase cardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/SO/CardDatabase/DefaultCardDatabase.asset");
        if (cardDatabase != null)
        {
            deckBuilderUI.cardDatabase = cardDatabase;
        }
        
        // シーンを保存
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/DeckBuilderScene.unity");
        
        // DeckBuilderUIを選択
        Selection.activeGameObject = deckBuilderObj;
        
        Debug.Log("🎯 DeckBuilderScene: シーンセットアップ完了");
        Debug.Log("📝 次の手順:");
        Debug.Log("1. DeckBuilderUIを選択");
        Debug.Log("2. Inspectorで「🎯 一括セットアップ実行」をクリック");
        Debug.Log("3. UI要素とプレハブが自動生成されます");
    }
    
    [MenuItem("Tools/Deck Builder/Create Deck Builder Scene")]
    public static void CreateDeckBuilderScene()
    {
        // 新しいシーンを作成
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        
        // シーン名を設定
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/DeckBuilderScene.unity");
        
        // セットアップを実行
        SetupDeckBuilderScene();
    }
    
    [MenuItem("Tools/Deck Builder/Setup Current Scene")]
    public static void SetupCurrentScene()
    {
        // 現在のシーンにDeckBuilderUIを追加
        Canvas canvas = Object.FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }
        
        // EventSystem確認
        if (Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        
        // DeckBuilderUI作成
        GameObject deckBuilderObj = new GameObject("DeckBuilderUI");
        deckBuilderObj.transform.SetParent(canvas.transform);
        DeckBuilderUI deckBuilderUI = deckBuilderObj.AddComponent<DeckBuilderUI>();
        
        // カードデータベースを自動設定
        CardDatabase cardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/SO/CardDatabase/DefaultCardDatabase.asset");
        if (cardDatabase != null)
        {
            deckBuilderUI.cardDatabase = cardDatabase;
        }
        
        // DeckBuilderUIを選択
        Selection.activeGameObject = deckBuilderObj;
        
        Debug.Log("🎯 現在のシーンにDeckBuilderUIを追加しました");
        Debug.Log("📝 Inspectorで「🎯 一括セットアップ実行」をクリックしてください");
    }
} 