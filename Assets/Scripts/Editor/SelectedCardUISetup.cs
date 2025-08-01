using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class SelectedCardUISetup : EditorWindow
{
    [MenuItem("Tools/Setup SelectedCardUI")]
    public static void SetupSelectedCardUI()
    {
        // Find the SelectedCardUI prefab
        GameObject selectedCardUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SelectedCardUI.prefab");
        
        if (selectedCardUIPrefab == null)
        {
            Debug.LogError("SelectedCardUI prefab not found at Assets/Prefabs/UI/SelectedCardUI.prefab");
            return;
        }
        
        // Get the SelectedCardUI component
        SelectedCardUI selectedCardUI = selectedCardUIPrefab.GetComponent<SelectedCardUI>();
        
        if (selectedCardUI == null)
        {
            Debug.LogError("SelectedCardUI component not found on the prefab");
            return;
        }
        
        // Find and assign references
        AssignReferences(selectedCardUIPrefab, selectedCardUI);
        
        // Mark the prefab as dirty to save changes
        EditorUtility.SetDirty(selectedCardUIPrefab);
        AssetDatabase.SaveAssets();
        
        Debug.Log("SelectedCardUI setup completed successfully!");
    }
    
    private static void AssignReferences(GameObject prefab, SelectedCardUI selectedCardUI)
    {
        // Find CardIcon (Image component)
        Transform cardIconTransform = prefab.transform.Find("CardIcon");
        if (cardIconTransform != null)
        {
            Image cardIcon = cardIconTransform.GetComponent<Image>();
            if (cardIcon != null)
            {
                selectedCardUI.cardIcon = cardIcon;
                Debug.Log("CardIcon assigned successfully");
            }
        }
        
        // Find CardName (TextMeshProUGUI component)
        Transform cardNameTransform = prefab.transform.Find("CardName");
        if (cardNameTransform != null)
        {
            TextMeshProUGUI cardNameText = cardNameTransform.GetComponent<TextMeshProUGUI>();
            if (cardNameText != null)
            {
                selectedCardUI.cardNameText = cardNameText;
                Debug.Log("CardNameText assigned successfully");
            }
        }
        
        // Find CardType (TextMeshProUGUI component)
        Transform cardTypeTransform = prefab.transform.Find("CardType");
        if (cardTypeTransform != null)
        {
            TextMeshProUGUI cardTypeText = cardTypeTransform.GetComponent<TextMeshProUGUI>();
            if (cardTypeText != null)
            {
                selectedCardUI.cardTypeText = cardTypeText;
                Debug.Log("CardTypeText assigned successfully");
            }
        }
        
        // Find RemoveButton (Button component)
        Transform removeButtonTransform = prefab.transform.Find("RemoveButton");
        if (removeButtonTransform != null)
        {
            Button removeButton = removeButtonTransform.GetComponent<Button>();
            if (removeButton != null)
            {
                selectedCardUI.removeButton = removeButton;
                Debug.Log("RemoveButton assigned successfully");
            }
        }
        
        // Find Background (Image component)
        Transform backgroundTransform = prefab.transform.Find("Background");
        if (backgroundTransform != null)
        {
            Image backgroundImage = backgroundTransform.GetComponent<Image>();
            if (backgroundImage != null)
            {
                selectedCardUI.backgroundImage = backgroundImage;
                Debug.Log("BackgroundImage assigned successfully");
            }
        }
    }
    
    [MenuItem("Tools/Verify SelectedCardUI Setup")]
    public static void VerifySelectedCardUISetup()
    {
        GameObject selectedCardUIPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/SelectedCardUI.prefab");
        
        if (selectedCardUIPrefab == null)
        {
            Debug.LogError("SelectedCardUI prefab not found!");
            return;
        }
        
        SelectedCardUI selectedCardUI = selectedCardUIPrefab.GetComponent<SelectedCardUI>();
        
        if (selectedCardUI == null)
        {
            Debug.LogError("SelectedCardUI component not found!");
            return;
        }
        
        // Check all references
        bool allAssigned = true;
        
        if (selectedCardUI.cardIcon == null)
        {
            Debug.LogError("CardIcon is not assigned!");
            allAssigned = false;
        }
        else
        {
            Debug.Log($"CardIcon assigned: {selectedCardUI.cardIcon.name}");
        }
        
        if (selectedCardUI.cardNameText == null)
        {
            Debug.LogError("CardNameText is not assigned!");
            allAssigned = false;
        }
        else
        {
            Debug.Log($"CardNameText assigned: {selectedCardUI.cardNameText.name}");
        }
        
        if (selectedCardUI.cardTypeText == null)
        {
            Debug.LogError("CardTypeText is not assigned!");
            allAssigned = false;
        }
        else
        {
            Debug.Log($"CardTypeText assigned: {selectedCardUI.cardTypeText.name}");
        }
        
        if (selectedCardUI.removeButton == null)
        {
            Debug.LogError("RemoveButton is not assigned!");
            allAssigned = false;
        }
        else
        {
            Debug.Log($"RemoveButton assigned: {selectedCardUI.removeButton.name}");
        }
        
        if (selectedCardUI.backgroundImage == null)
        {
            Debug.LogWarning("BackgroundImage is not assigned (optional)");
        }
        else
        {
            Debug.Log($"BackgroundImage assigned: {selectedCardUI.backgroundImage.name}");
        }
        
        if (allAssigned)
        {
            Debug.Log("All SelectedCardUI references are properly assigned!");
        }
        
        // Test with sample card data
        TestWithSampleCard(selectedCardUI);
    }
    
    private static void TestWithSampleCard(SelectedCardUI selectedCardUI)
    {
        // Load a sample card to test the display
        CardDataSO sampleCard = AssetDatabase.LoadAssetAtPath<CardDataSO>("Assets/SO/Card/Attack.asset");
        
        if (sampleCard != null)
        {
            Debug.Log($"Testing with sample card: {sampleCard.cardName}");
            Debug.Log($"Sample card icon: {(sampleCard.icon != null ? sampleCard.icon.name : "null")}");
            
            // Create a temporary instance to test
            GameObject tempInstance = PrefabUtility.InstantiatePrefab(selectedCardUI.gameObject) as GameObject;
            SelectedCardUI tempSelectedCardUI = tempInstance.GetComponent<SelectedCardUI>();
            
            if (tempSelectedCardUI != null)
            {
                tempSelectedCardUI.Setup(sampleCard, (card) => Debug.Log($"Remove callback for {card.cardName}"));
                Debug.Log("Sample card test completed successfully!");
            }
            
            // Clean up
            DestroyImmediate(tempInstance);
        }
        else
        {
            Debug.LogError("Sample card not found for testing!");
        }
    }
} 