using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class DeckBuilderUISetup : EditorWindow
{
    [MenuItem("Tools/Setup DeckBuilderUI")]
    public static void SetupDeckBuilderUI()
    {
        // Find the DeckBuilderScene
        string scenePath = "Assets/Scenes/DeckBuilderScene.unity";
        if (!UnityEditor.EditorApplication.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scenePath);
        }
        
        // Find the DeckBuilderUI component in the scene
        DeckBuilderUI deckBuilderUI = FindObjectOfType<DeckBuilderUI>();
        
        if (deckBuilderUI == null)
        {
            Debug.LogError("DeckBuilderUI component not found in the scene!");
            return;
        }
        
        // Assign references
        AssignDeckBuilderUIReferences(deckBuilderUI);
        
        // Mark the scene as dirty
        EditorUtility.SetDirty(deckBuilderUI.gameObject);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
        
        Debug.Log("DeckBuilderUI setup completed successfully!");
    }
    
    private static void AssignDeckBuilderUIReferences(DeckBuilderUI deckBuilderUI)
    {
        // Find CardDatabase
        CardDatabase cardDatabase = FindObjectOfType<CardDatabase>();
        if (cardDatabase == null)
        {
            cardDatabase = AssetDatabase.LoadAssetAtPath<CardDatabase>("Assets/SO/CardDatabase/DefaultCardDatabase.asset");
        }
        
        if (cardDatabase != null)
        {
            deckBuilderUI.cardDatabase = cardDatabase;
            Debug.Log("CardDatabase assigned successfully");
        }
        else
        {
            Debug.LogError("CardDatabase not found!");
        }
        
        // Find UI elements by name
        Transform canvas = FindObjectOfType<Canvas>()?.transform;
        if (canvas == null)
        {
            Debug.LogError("Canvas not found in scene!");
            return;
        }
        
        // Find CardListContent
        Transform cardListContent = FindChildByName(canvas, "CardListContent");
        if (cardListContent != null)
        {
            deckBuilderUI.cardListContent = cardListContent;
            Debug.Log("CardListContent assigned successfully");
        }
        
        // Find CardListItemPrefab
        GameObject cardListItemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CardListItemUI.prefab");
        if (cardListItemPrefab != null)
        {
            deckBuilderUI.cardListItemPrefab = cardListItemPrefab;
            Debug.Log("CardListItemPrefab assigned successfully");
        }
        
        // Find CardListScrollView
        ScrollRect cardListScrollView = FindComponentInChildren<ScrollRect>(canvas, "CardListScrollView");
        if (cardListScrollView != null)
        {
            deckBuilderUI.cardListScrollView = cardListScrollView;
            Debug.Log("CardListScrollView assigned successfully");
        }
        
        // Find SelectedDeckContent
        Transform selectedDeckContent = FindChildByName(canvas, "SelectedDeckContent");
        if (selectedDeckContent != null)
        {
            deckBuilderUI.selectedDeckContent = selectedDeckContent;
            Debug.Log("SelectedDeckContent assigned successfully");
        }
        
        // Find SelectedCardPrefab
        GameObject selectedCardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SelectedCardUI.prefab");
        if (selectedCardPrefab != null)
        {
            deckBuilderUI.selectedCardPrefab = selectedCardPrefab;
            Debug.Log("SelectedCardPrefab assigned successfully");
        }
        
        // Find Text components
        TextMeshProUGUI deckSizeText = FindComponentInChildren<TextMeshProUGUI>(canvas, "DeckSizeText");
        if (deckSizeText != null)
        {
            deckBuilderUI.deckSizeText = deckSizeText;
            Debug.Log("DeckSizeText assigned successfully");
        }
        
        TextMeshProUGUI deckStatisticsText = FindComponentInChildren<TextMeshProUGUI>(canvas, "DeckStatisticsText");
        if (deckStatisticsText != null)
        {
            deckBuilderUI.deckStatisticsText = deckStatisticsText;
            Debug.Log("DeckStatisticsText assigned successfully");
        }
        
        // Find Buttons
        Button addCardButton = FindComponentInChildren<Button>(canvas, "AddCardButton");
        if (addCardButton != null)
        {
            deckBuilderUI.addCardButton = addCardButton;
            Debug.Log("AddCardButton assigned successfully");
        }
        
        Button removeCardButton = FindComponentInChildren<Button>(canvas, "RemoveCardButton");
        if (removeCardButton != null)
        {
            deckBuilderUI.removeCardButton = removeCardButton;
            Debug.Log("RemoveCardButton assigned successfully");
        }
        
        Button startBattleButton = FindComponentInChildren<Button>(canvas, "StartBattleButton");
        if (startBattleButton != null)
        {
            deckBuilderUI.startBattleButton = startBattleButton;
            Debug.Log("StartBattleButton assigned successfully");
        }
        
        Button clearDeckButton = FindComponentInChildren<Button>(canvas, "ClearDeckButton");
        if (clearDeckButton != null)
        {
            deckBuilderUI.clearDeckButton = clearDeckButton;
            Debug.Log("ClearDeckButton assigned successfully");
        }
        
        // Find Filter Buttons
        Button allCardsButton = FindComponentInChildren<Button>(canvas, "AllCardsButton");
        if (allCardsButton != null)
        {
            deckBuilderUI.allCardsButton = allCardsButton;
            Debug.Log("AllCardsButton assigned successfully");
        }
        
        Button attackCardsButton = FindComponentInChildren<Button>(canvas, "AttackCardsButton");
        if (attackCardsButton != null)
        {
            deckBuilderUI.attackCardsButton = attackCardsButton;
            Debug.Log("AttackCardsButton assigned successfully");
        }
        
        Button moveCardsButton = FindComponentInChildren<Button>(canvas, "MoveCardsButton");
        if (moveCardsButton != null)
        {
            deckBuilderUI.moveCardsButton = moveCardsButton;
            Debug.Log("MoveCardsButton assigned successfully");
        }
        
        Button healCardsButton = FindComponentInChildren<Button>(canvas, "HealCardsButton");
        if (healCardsButton != null)
        {
            deckBuilderUI.healCardsButton = healCardsButton;
            Debug.Log("HealCardsButton assigned successfully");
        }
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
            Debug.LogError("DeckBuilderUI component not found in scene!");
            return;
        }
        
        bool allAssigned = true;
        
        // Check required references
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
        
        if (deckBuilderUI.deckSizeText == null)
        {
            Debug.LogWarning("DeckSizeText is not assigned!");
        }
        
        if (deckBuilderUI.deckStatisticsText == null)
        {
            Debug.LogWarning("DeckStatisticsText is not assigned!");
        }
        
        if (deckBuilderUI.addCardButton == null)
        {
            Debug.LogWarning("AddCardButton is not assigned!");
        }
        
        if (deckBuilderUI.removeCardButton == null)
        {
            Debug.LogWarning("RemoveCardButton is not assigned!");
        }
        
        if (deckBuilderUI.startBattleButton == null)
        {
            Debug.LogError("StartBattleButton is not assigned!");
            allAssigned = false;
        }
        
        if (deckBuilderUI.clearDeckButton == null)
        {
            Debug.LogWarning("ClearDeckButton is not assigned!");
        }
        
        if (deckBuilderUI.allCardsButton == null)
        {
            Debug.LogWarning("AllCardsButton is not assigned!");
        }
        
        if (deckBuilderUI.attackCardsButton == null)
        {
            Debug.LogWarning("AttackCardsButton is not assigned!");
        }
        
        if (deckBuilderUI.moveCardsButton == null)
        {
            Debug.LogWarning("MoveCardsButton is not assigned!");
        }
        
        if (deckBuilderUI.healCardsButton == null)
        {
            Debug.LogWarning("HealCardsButton is not assigned!");
        }
        
        if (allAssigned)
        {
            Debug.Log("All essential DeckBuilderUI references are properly assigned!");
        }
        else
        {
            Debug.LogError("Some essential DeckBuilderUI references are missing!");
        }
    }
} 