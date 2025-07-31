using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[CustomEditor(typeof(DeckBuilderUI))]
public class DeckBuilderUIEditor : Editor
{
    private DeckBuilderUI deckBuilderUI;
    private bool showCardListSetup = true;
    private bool showSelectedDeckSetup = true;
    private bool showButtonSetup = true;
    private bool showFilterSetup = true;
    private bool showPrefabSetup = true;
    
    private void OnEnable()
    {
        deckBuilderUI = (DeckBuilderUI)target;
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("=== DeckBuilderUI 自動セットアップ ===", EditorStyles.boldLabel);
        
        // カードリストエリアのセットアップ
        showCardListSetup = EditorGUILayout.Foldout(showCardListSetup, "カードリストエリア設定");
        if (showCardListSetup)
        {
            DrawCardListSetup();
        }
        
        // 選択デッキエリアのセットアップ
        showSelectedDeckSetup = EditorGUILayout.Foldout(showSelectedDeckSetup, "選択デッキエリア設定");
        if (showSelectedDeckSetup)
        {
            DrawSelectedDeckSetup();
        }
        
        // ボタン設定
        showButtonSetup = EditorGUILayout.Foldout(showButtonSetup, "ボタン設定");
        if (showButtonSetup)
        {
            DrawButtonSetup();
        }
        
        // フィルターボタン設定
        showFilterSetup = EditorGUILayout.Foldout(showFilterSetup, "フィルターボタン設定");
        if (showFilterSetup)
        {
            DrawFilterSetup();
        }
        
        // プレハブ設定
        showPrefabSetup = EditorGUILayout.Foldout(showPrefabSetup, "プレハブ設定");
        if (showPrefabSetup)
        {
            DrawPrefabSetup();
        }
        
        EditorGUILayout.Space();
        
        // 一括セットアップボタン
        if (GUILayout.Button("🎯 一括セットアップ実行", GUILayout.Height(40)))
        {
            SetupAllUIElements();
        }
        
        // プレハブ作成ボタン
        if (GUILayout.Button("📦 プレハブ作成", GUILayout.Height(30)))
        {
            CreatePrefabs();
        }
        
        // クリーンアップボタン
        if (GUILayout.Button("🧹 クリーンアップ", GUILayout.Height(30)))
        {
            CleanupUIElements();
        }
    }
    
    private void DrawCardListSetup()
    {
        EditorGUILayout.BeginVertical("box");
        
        if (GUILayout.Button("📋 カードリストエリア作成"))
        {
            CreateCardListArea();
        }
        
        EditorGUILayout.HelpBox("カードリストエリアを作成します。\n- ScrollRect\n- Content\n- カードアイテム表示エリア", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawSelectedDeckSetup()
    {
        EditorGUILayout.BeginVertical("box");
        
        if (GUILayout.Button("🎴 選択デッキエリア作成"))
        {
            CreateSelectedDeckArea();
        }
        
        EditorGUILayout.HelpBox("選択デッキエリアを作成します。\n- 選択カード表示エリア\n- デッキサイズ表示\n- 統計情報表示", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawButtonSetup()
    {
        EditorGUILayout.BeginVertical("box");
        
        if (GUILayout.Button("🔘 コントロールボタン作成"))
        {
            CreateControlButtons();
        }
        
        EditorGUILayout.HelpBox("コントロールボタンを作成します。\n- カード追加ボタン\n- カード削除ボタン\n- バトル開始ボタン\n- デッキクリアボタン", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawFilterSetup()
    {
        EditorGUILayout.BeginVertical("box");
        
        if (GUILayout.Button("🔍 フィルターボタン作成"))
        {
            CreateFilterButtons();
        }
        
        EditorGUILayout.HelpBox("フィルターボタンを作成します。\n- 全カード\n- 攻撃カード\n- 移動カード\n- 回復カード", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void DrawPrefabSetup()
    {
        EditorGUILayout.BeginVertical("box");
        
        if (GUILayout.Button("📦 CardListItemUI プレハブ作成"))
        {
            CreateCardListItemPrefab();
        }
        
        if (GUILayout.Button("📦 SelectedCardUI プレハブ作成"))
        {
            CreateSelectedCardPrefab();
        }
        
        EditorGUILayout.HelpBox("カードUIプレハブを作成します。\n- CardListItemUI: カードリスト用\n- SelectedCardUI: 選択デッキ用", MessageType.Info);
        
        EditorGUILayout.EndVertical();
    }
    
    private void SetupAllUIElements()
    {
        Undo.RecordObject(deckBuilderUI, "Setup All UI Elements");
        
        // メインキャンバス作成
        Canvas canvas = CreateMainCanvas();
        
        // 各エリアを作成
        CreateCardListArea();
        CreateSelectedDeckArea();
        CreateControlButtons();
        CreateFilterButtons();
        
        // プレハブ作成
        CreateCardListItemPrefab();
        CreateSelectedCardPrefab();
        
        // 参照を自動設定
        AutoAssignReferences();
        
        EditorUtility.SetDirty(deckBuilderUI);
        Debug.Log("🎯 DeckBuilderUI: 一括セットアップ完了");
    }
    
    private Canvas CreateMainCanvas()
    {
        Canvas canvas = deckBuilderUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("DeckBuilderCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
            
            // DeckBuilderUIをCanvasの子に移動
            deckBuilderUI.transform.SetParent(canvasObj.transform);
        }
        
        return canvas;
    }
    
    private void CreateCardListArea()
    {
        // カードリストエリアの親オブジェクト
        GameObject cardListArea = CreateUIElement("CardListArea", deckBuilderUI.transform);
        
        // タイトル
        CreateTextElement("Title", cardListArea.transform, "利用可能カード", 24);
        
        // ScrollRect作成
        GameObject scrollView = CreateScrollView("CardListScrollView", cardListArea.transform);
        deckBuilderUI.cardListScrollView = scrollView.GetComponent<ScrollRect>();
        
        // Content作成
        GameObject content = CreateUIElement("CardListContent", scrollView.transform);
        content.AddComponent<VerticalLayoutGroup>();
        content.AddComponent<ContentSizeFitter>();
        deckBuilderUI.cardListContent = content.transform;
        
        // レイアウト設定
        var layoutGroup = content.GetComponent<VerticalLayoutGroup>();
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        
        var contentFitter = content.GetComponent<ContentSizeFitter>();
        contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        
        Debug.Log("📋 カードリストエリア作成完了");
    }
    
    private void CreateSelectedDeckArea()
    {
        // 選択デッキエリアの親オブジェクト
        GameObject selectedDeckArea = CreateUIElement("SelectedDeckArea", deckBuilderUI.transform);
        
        // タイトル
        CreateTextElement("Title", selectedDeckArea.transform, "選択デッキ", 24);
        
        // デッキ情報表示エリア
        GameObject deckInfoArea = CreateUIElement("DeckInfoArea", selectedDeckArea.transform);
        deckInfoArea.AddComponent<HorizontalLayoutGroup>();
        
        // デッキサイズテキスト
        GameObject deckSizeObj = CreateTextElement("DeckSizeText", deckInfoArea.transform, "デッキ: 0/10", 18);
        deckBuilderUI.deckSizeText = deckSizeObj.GetComponent<TextMeshProUGUI>();
        
        // 統計情報テキスト
        GameObject statsObj = CreateTextElement("DeckStatisticsText", deckInfoArea.transform, "攻撃: 0 移動: 0 回復: 0", 16);
        deckBuilderUI.deckStatisticsText = statsObj.GetComponent<TextMeshProUGUI>();
        
        // 選択カード表示エリア
        GameObject selectedCardsArea = CreateUIElement("SelectedCardsArea", selectedDeckArea.transform);
        selectedCardsArea.AddComponent<HorizontalLayoutGroup>();
        deckBuilderUI.selectedDeckContent = selectedCardsArea.transform;
        
        var layoutGroup = selectedCardsArea.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        layoutGroup.childControlHeight = true;
        layoutGroup.childControlWidth = true;
        
        Debug.Log("🎴 選択デッキエリア作成完了");
    }
    
    private void CreateControlButtons()
    {
        // ボタンエリアの親オブジェクト
        GameObject buttonArea = CreateUIElement("ControlButtonArea", deckBuilderUI.transform);
        buttonArea.AddComponent<HorizontalLayoutGroup>();
        
        var layoutGroup = buttonArea.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 10;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        
        // カード追加ボタン
        GameObject addButton = CreateButton("AddCardButton", buttonArea.transform, "カード追加");
        deckBuilderUI.addCardButton = addButton.GetComponent<Button>();
        
        // カード削除ボタン
        GameObject removeButton = CreateButton("RemoveCardButton", buttonArea.transform, "カード削除");
        deckBuilderUI.removeCardButton = removeButton.GetComponent<Button>();
        
        // デッキクリアボタン
        GameObject clearButton = CreateButton("ClearDeckButton", buttonArea.transform, "デッキクリア");
        deckBuilderUI.clearDeckButton = clearButton.GetComponent<Button>();
        
        // バトル開始ボタン
        GameObject startButton = CreateButton("StartBattleButton", buttonArea.transform, "バトル開始");
        startButton.GetComponent<Image>().color = Color.green;
        deckBuilderUI.startBattleButton = startButton.GetComponent<Button>();
        
        Debug.Log("🔘 コントロールボタン作成完了");
    }
    
    private void CreateFilterButtons()
    {
        // フィルターボタンエリアの親オブジェクト
        GameObject filterArea = CreateUIElement("FilterButtonArea", deckBuilderUI.transform);
        filterArea.AddComponent<HorizontalLayoutGroup>();
        
        var layoutGroup = filterArea.GetComponent<HorizontalLayoutGroup>();
        layoutGroup.spacing = 5;
        layoutGroup.padding = new RectOffset(10, 10, 10, 10);
        
        // 全カードボタン
        GameObject allButton = CreateButton("AllCardsButton", filterArea.transform, "全カード");
        deckBuilderUI.allCardsButton = allButton.GetComponent<Button>();
        
        // 攻撃カードボタン
        GameObject attackButton = CreateButton("AttackCardsButton", filterArea.transform, "攻撃");
        deckBuilderUI.attackCardsButton = attackButton.GetComponent<Button>();
        
        // 移動カードボタン
        GameObject moveButton = CreateButton("MoveCardsButton", filterArea.transform, "移動");
        deckBuilderUI.moveCardsButton = moveButton.GetComponent<Button>();
        
        // 回復カードボタン
        GameObject healButton = CreateButton("HealCardsButton", filterArea.transform, "回復");
        deckBuilderUI.healCardsButton = healButton.GetComponent<Button>();
        
        Debug.Log("🔍 フィルターボタン作成完了");
    }
    
    private void CreateCardListItemPrefab()
    {
        // プレハブ作成
        GameObject cardListItem = CreateUIElement("CardListItem", null);
        cardListItem.AddComponent<CardListItemUI>();
        
        // 背景
        GameObject background = CreateUIElement("Background", cardListItem.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.white;
        
        // レイアウト
        cardListItem.AddComponent<LayoutElement>();
        var layoutElement = cardListItem.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 200;
        layoutElement.preferredHeight = 80;
        
        // カードアイコン
        GameObject iconObj = CreateUIElement("CardIcon", cardListItem.transform);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.gray;
        var iconLayout = iconObj.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 60;
        iconLayout.preferredHeight = 60;
        
        // カード情報エリア
        GameObject infoArea = CreateUIElement("CardInfo", cardListItem.transform);
        infoArea.AddComponent<VerticalLayoutGroup>();
        
        // カード名
        GameObject nameObj = CreateTextElement("CardName", infoArea.transform, "カード名", 16);
        nameObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        // カードタイプ
        GameObject typeObj = CreateTextElement("CardType", infoArea.transform, "タイプ", 14);
        
        // カード効果
        GameObject powerObj = CreateTextElement("CardPower", infoArea.transform, "効果", 14);
        
        // ボタン
        Button cardButton = cardListItem.AddComponent<Button>();
        
        // 参照を設定
        var cardListItemUI = cardListItem.GetComponent<CardListItemUI>();
        cardListItemUI.cardIcon = iconImage;
        cardListItemUI.cardNameText = nameObj.GetComponent<TextMeshProUGUI>();
        cardListItemUI.cardTypeText = typeObj.GetComponent<TextMeshProUGUI>();
        cardListItemUI.cardPowerText = powerObj.GetComponent<TextMeshProUGUI>();
        cardListItemUI.cardButton = cardButton;
        cardListItemUI.backgroundImage = bgImage;
        
        // プレハブとして保存
        string prefabPath = "Assets/Prefabs/UI/CardListItemUI.prefab";
        CreatePrefabDirectory();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(cardListItem, prefabPath);
        deckBuilderUI.cardListItemPrefab = prefab;
        
        DestroyImmediate(cardListItem);
        
        Debug.Log("📦 CardListItemUI プレハブ作成完了");
    }
    
    private void CreateSelectedCardPrefab()
    {
        // プレハブ作成
        GameObject selectedCard = CreateUIElement("SelectedCard", null);
        selectedCard.AddComponent<SelectedCardUI>();
        
        // 背景
        GameObject background = CreateUIElement("Background", selectedCard.transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = Color.white;
        
        // レイアウト
        selectedCard.AddComponent<LayoutElement>();
        var layoutElement = selectedCard.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = 150;
        layoutElement.preferredHeight = 60;
        
        // カードアイコン
        GameObject iconObj = CreateUIElement("CardIcon", selectedCard.transform);
        Image iconImage = iconObj.AddComponent<Image>();
        iconImage.color = Color.gray;
        var iconLayout = iconObj.AddComponent<LayoutElement>();
        iconLayout.preferredWidth = 40;
        iconLayout.preferredHeight = 40;
        
        // カード情報エリア
        GameObject infoArea = CreateUIElement("CardInfo", selectedCard.transform);
        infoArea.AddComponent<VerticalLayoutGroup>();
        
        // カード名
        GameObject nameObj = CreateTextElement("CardName", infoArea.transform, "カード名", 14);
        nameObj.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
        
        // カードタイプ
        GameObject typeObj = CreateTextElement("CardType", infoArea.transform, "タイプ", 12);
        
        // 削除ボタン
        GameObject removeObj = CreateButton("RemoveButton", selectedCard.transform, "×");
        removeObj.GetComponent<Image>().color = Color.red;
        var removeLayout = removeObj.AddComponent<LayoutElement>();
        removeLayout.preferredWidth = 30;
        removeLayout.preferredHeight = 30;
        
        // 参照を設定
        var selectedCardUI = selectedCard.GetComponent<SelectedCardUI>();
        selectedCardUI.cardIcon = iconImage;
        selectedCardUI.cardNameText = nameObj.GetComponent<TextMeshProUGUI>();
        selectedCardUI.cardTypeText = typeObj.GetComponent<TextMeshProUGUI>();
        selectedCardUI.removeButton = removeObj.GetComponent<Button>();
        selectedCardUI.backgroundImage = bgImage;
        
        // プレハブとして保存
        string prefabPath = "Assets/Prefabs/UI/SelectedCardUI.prefab";
        CreatePrefabDirectory();
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(selectedCard, prefabPath);
        deckBuilderUI.selectedCardPrefab = prefab;
        
        DestroyImmediate(selectedCard);
        
        Debug.Log("📦 SelectedCardUI プレハブ作成完了");
    }
    
    private void CreatePrefabs()
    {
        CreateCardListItemPrefab();
        CreateSelectedCardPrefab();
    }
    
    private void AutoAssignReferences()
    {
        // CardDatabaseの自動設定
        if (deckBuilderUI.cardDatabase == null)
        {
            deckBuilderUI.cardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/SO/CardDatabase/DefaultCardDatabase.asset");
        }
        
        Debug.Log("🔗 参照自動設定完了");
    }
    
    private void CleanupUIElements()
    {
        // 不要なUI要素を削除
        Transform[] children = deckBuilderUI.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child != deckBuilderUI.transform && child.name.Contains("Temp"))
            {
                DestroyImmediate(child.gameObject);
            }
        }
        
        Debug.Log("🧹 クリーンアップ完了");
    }
    
    // ヘルパーメソッド
    private GameObject CreateUIElement(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.AddComponent<RectTransform>();
        return obj;
    }
    
    private GameObject CreateTextElement(string name, Transform parent, string text, int fontSize)
    {
        GameObject obj = CreateUIElement(name, parent);
        TextMeshProUGUI textComponent = obj.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.color = Color.black;
        return obj;
    }
    
    private GameObject CreateButton(string name, Transform parent, string text)
    {
        GameObject obj = CreateUIElement(name, parent);
        Image image = obj.AddComponent<Image>();
        image.color = Color.white;
        
        Button button = obj.AddComponent<Button>();
        
        GameObject textObj = CreateTextElement("Text", obj.transform, text, 16);
        textObj.GetComponent<TextMeshProUGUI>().color = Color.black;
        
        return obj;
    }
    
    private GameObject CreateScrollView(string name, Transform parent)
    {
        GameObject obj = CreateUIElement(name, parent);
        
        // ScrollRect
        ScrollRect scrollRect = obj.AddComponent<ScrollRect>();
        
        // Image
        Image image = obj.AddComponent<Image>();
        image.color = new Color(0.9f, 0.9f, 0.9f, 0.5f);
        
        // Viewport
        GameObject viewport = CreateUIElement("Viewport", obj.transform);
        viewport.AddComponent<Image>();
        viewport.AddComponent<Mask>();
        viewport.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        viewport.GetComponent<RectTransform>().anchorMax = Vector2.one;
        viewport.GetComponent<RectTransform>().sizeDelta = Vector2.zero;
        
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        
        return obj;
    }
    
    private void CreatePrefabDirectory()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        {
            AssetDatabase.CreateFolder("Assets", "Prefabs");
        }
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs/UI"))
        {
            AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
        }
    }
} 