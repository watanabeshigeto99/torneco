using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DeckBuilderUISetup : EditorWindow
{
    [MenuItem("Tools/Setup DeckBuilderUI")]
    public static void SetupDeckBuilderUI()
    {
        // DeckBuilderSceneを開く
        if (UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().name != "DeckBuilderScene")
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene("Assets/Scenes/DeckBuilderScene.unity");
        }
        
        // DeckBuilderUIコンポーネントを探す
        DeckBuilderUI deckBuilderUI = FindObjectOfType<DeckBuilderUI>();
        
        if (deckBuilderUI == null)
        {
            Debug.LogError("DeckBuilderUI component not found in the scene!");
            return;
        }
        
        // 参照を自動設定
        AssignDeckBuilderUIReferences(deckBuilderUI);
        
        // 階層情報表示用のUIを追加
        AddFloorInfoUI(deckBuilderUI);
        
        // SoundManagerを追加
        AddSoundManager();
        
        // SoundManagerの設定も実行
        SoundManagerSetup.SetupSoundManager();
        
        // SceneBGMControllerも追加
        SceneBGMControllerSetup.SetupSceneBGMController();
        
        Debug.Log("DeckBuilderUI setup completed successfully!");
    }
    
    private static void AssignDeckBuilderUIReferences(DeckBuilderUI deckBuilderUI)
    {
        // CardDatabaseの設定
        if (deckBuilderUI.cardDatabase == null)
        {
            deckBuilderUI.cardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/SO/CardDatabase/DefaultCardDatabase.asset");
            Debug.Log("CardDatabase assigned");
        }
        
        // カードリスト関連の設定
        if (deckBuilderUI.cardListContent == null)
        {
            deckBuilderUI.cardListContent = FindChildByName(deckBuilderUI.transform, "CardListContent");
            Debug.Log("CardListContent assigned");
        }
        
        if (deckBuilderUI.cardListItemPrefab == null)
        {
            deckBuilderUI.cardListItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CardListItemUI.prefab");
            Debug.Log("CardListItemPrefab assigned");
        }
        
        if (deckBuilderUI.cardListScrollView == null)
        {
            deckBuilderUI.cardListScrollView = FindComponentInChildren<ScrollRect>(deckBuilderUI.transform, "CardListScrollView");
            Debug.Log("CardListScrollView assigned");
        }
        
        // 選択デッキ関連の設定
        if (deckBuilderUI.selectedDeckContent == null)
        {
            deckBuilderUI.selectedDeckContent = FindChildByName(deckBuilderUI.transform, "SelectedDeckContent");
            Debug.Log("SelectedDeckContent assigned");
        }
        
        if (deckBuilderUI.selectedCardPrefab == null)
        {
            deckBuilderUI.selectedCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SelectedCardUI.prefab");
            Debug.Log("SelectedCardPrefab assigned");
        }
        
        if (deckBuilderUI.deckSizeText == null)
        {
            deckBuilderUI.deckSizeText = FindComponentInChildren<TextMeshProUGUI>(deckBuilderUI.transform, "DeckSizeText");
            Debug.Log("DeckSizeText assigned");
        }
        
        if (deckBuilderUI.deckStatisticsText == null)
        {
            deckBuilderUI.deckStatisticsText = FindComponentInChildren<TextMeshProUGUI>(deckBuilderUI.transform, "DeckStatisticsText");
            Debug.Log("DeckStatisticsText assigned");
        }
        
        // ボタンの設定
        if (deckBuilderUI.addCardButton == null)
        {
            deckBuilderUI.addCardButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "AddCardButton");
            Debug.Log("AddCardButton assigned");
        }
        
        if (deckBuilderUI.removeCardButton == null)
        {
            deckBuilderUI.removeCardButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "RemoveCardButton");
            Debug.Log("RemoveCardButton assigned");
        }
        
        if (deckBuilderUI.startBattleButton == null)
        {
            deckBuilderUI.startBattleButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "StartBattleButton");
            Debug.Log("StartBattleButton assigned");
        }
        
        if (deckBuilderUI.clearDeckButton == null)
        {
            deckBuilderUI.clearDeckButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "ClearDeckButton");
            Debug.Log("ClearDeckButton assigned");
        }
        
        // フィルターボタンの設定
        if (deckBuilderUI.allCardsButton == null)
        {
            deckBuilderUI.allCardsButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "AllCardsButton");
            Debug.Log("AllCardsButton assigned");
        }
        
        if (deckBuilderUI.attackCardsButton == null)
        {
            deckBuilderUI.attackCardsButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "AttackCardsButton");
            Debug.Log("AttackCardsButton assigned");
        }
        
        if (deckBuilderUI.moveCardsButton == null)
        {
            deckBuilderUI.moveCardsButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "MoveCardsButton");
            Debug.Log("MoveCardsButton assigned");
        }
        
        if (deckBuilderUI.healCardsButton == null)
        {
            deckBuilderUI.healCardsButton = FindComponentInChildren<Button>(deckBuilderUI.transform, "HealCardsButton");
            Debug.Log("HealCardsButton assigned");
        }
    }
    
    private static void AddFloorInfoUI(DeckBuilderUI deckBuilderUI)
    {
        // 階層情報表示用のUIが既に存在するかチェック
        if (deckBuilderUI.floorInfoText != null)
        {
            Debug.Log("FloorInfoText already exists");
            return;
        }
        
        // Canvasを探す
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in the scene!");
            return;
        }
        
        // 階層情報表示用のUIを作成
        GameObject floorInfoObj = new GameObject("FloorInfo");
        floorInfoObj.transform.SetParent(canvas.transform, false);
        
        // RectTransformを設定
        RectTransform rectTransform = floorInfoObj.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0, 1);
        rectTransform.anchorMax = new Vector2(0, 1);
        rectTransform.pivot = new Vector2(0, 1);
        rectTransform.anchoredPosition = new Vector2(20, -20);
        rectTransform.sizeDelta = new Vector2(200, 50);
        
        // 背景画像を追加
        Image backgroundImage = floorInfoObj.AddComponent<Image>();
        backgroundImage.color = new Color(0, 0, 0, 0.8f);
        
        // テキストを追加
        GameObject textObj = new GameObject("FloorInfoText");
        textObj.transform.SetParent(floorInfoObj.transform, false);
        
        RectTransform textRectTransform = textObj.AddComponent<RectTransform>();
        textRectTransform.anchorMin = Vector2.zero;
        textRectTransform.anchorMax = Vector2.one;
        textRectTransform.offsetMin = Vector2.zero;
        textRectTransform.offsetMax = Vector2.zero;
        
        TextMeshProUGUI floorInfoText = textObj.AddComponent<TextMeshProUGUI>();
        floorInfoText.text = "階層 1";
        floorInfoText.fontSize = 18;
        floorInfoText.color = Color.white;
        floorInfoText.alignment = TextAlignmentOptions.Center;
        
        // DeckBuilderUIに参照を設定
        deckBuilderUI.floorInfoText = floorInfoText;
        
        Debug.Log("FloorInfoUI created and assigned");
    }
    
    private static void AddSoundManager()
    {
        // SoundManagerが既に存在するかチェック
        SoundManager existingSoundManager = FindObjectOfType<SoundManager>();
        if (existingSoundManager != null)
        {
            Debug.Log("SoundManager already exists in the scene");
            return;
        }
        
        // SoundManagerを作成
        GameObject soundManagerObj = new GameObject("SoundManager");
        SoundManager soundManager = soundManagerObj.AddComponent<SoundManager>();
        
        Debug.Log("SoundManager created and added to the scene");
    }
    
    private static Transform FindChildByName(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;
            
            Transform result = FindChildByName(child, name);
            if (result != null)
                return result;
        }
        return null;
    }
    
    private static T FindComponentInChildren<T>(Transform parent, string name) where T : Component
    {
        Transform child = FindChildByName(parent, name);
        if (child != null)
        {
            return child.GetComponent<T>();
        }
        return null;
    }
    
    [MenuItem("Tools/Verify DeckBuilderUI Setup")]
    public static void VerifyDeckBuilderUISetup()
    {
        DeckBuilderUI deckBuilderUI = FindObjectOfType<DeckBuilderUI>();
        
        if (deckBuilderUI == null)
        {
            Debug.LogError("DeckBuilderUI component not found!");
            return;
        }
        
        bool allAssigned = true;
        
        if (deckBuilderUI.cardDatabase == null)
        {
            Debug.LogError("CardDatabase is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.cardListContent == null)
        {
            Debug.LogError("CardListContent is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.cardListItemPrefab == null)
        {
            Debug.LogError("CardListItemPrefab is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.selectedDeckContent == null)
        {
            Debug.LogError("SelectedDeckContent is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.selectedCardPrefab == null)
        {
            Debug.LogError("SelectedCardPrefab is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.startBattleButton == null)
        {
            Debug.LogError("StartBattleButton is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.floorInfoText == null)
        {
            Debug.LogWarning("FloorInfoText is not assigned (optional)");
        }
        
        if (allAssigned)
        {
            Debug.Log("All DeckBuilderUI references are properly assigned!");
        }
    }
} 