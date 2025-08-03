using UnityEngine;
using UnityEditor;

public class ScriptRefresher : EditorWindow
{
    [MenuItem("Tools/Refresh Script References")]
    public static void RefreshScriptReferences()
    {
        // Force Unity to refresh all assets
        AssetDatabase.Refresh();
        
        // Clear any cached data
        EditorUtility.SetDirty(Selection.activeGameObject);
        
        Debug.Log("Script references refreshed. Please check the console for any remaining serialization errors.");
    }
    
    [MenuItem("Tools/Clear All Script Caches")]
    public static void ClearAllCaches()
    {
        // Force a complete refresh
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        
        // Clear any cached serialization data
        EditorUtility.SetDirty(Selection.activeGameObject);
        
        Debug.Log("All script caches cleared. Unity will recompile all scripts.");
    }
} 