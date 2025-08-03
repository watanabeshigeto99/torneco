using UnityEngine;
using UnityEditor;
using System.Reflection;

public class SerializationChecker : EditorWindow
{
    [MenuItem("Tools/Check Serialization Issues")]
    public static void CheckSerializationIssues()
    {
        // Check Player script
        var playerType = typeof(Player);
        var playerFields = playerType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        Debug.Log("=== Player.cs Fields ===");
        foreach (var field in playerFields)
        {
            Debug.Log($"Field: {field.Name} ({field.FieldType.Name}) - Access: {field.IsPublic}");
        }
        
        // Check Enemy script
        var enemyType = typeof(Enemy);
        var enemyFields = enemyType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        Debug.Log("=== Enemy.cs Fields ===");
        foreach (var field in enemyFields)
        {
            Debug.Log($"Field: {field.Name} ({field.FieldType.Name}) - Access: {field.IsPublic}");
        }
        
        // Check Unit script
        var unitType = typeof(Unit);
        var unitFields = unitType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        
        Debug.Log("=== Unit.cs Fields ===");
        foreach (var field in unitFields)
        {
            Debug.Log($"Field: {field.Name} ({field.FieldType.Name}) - Access: {field.IsPublic}");
        }
        
        Debug.Log("Serialization check complete. Check the console for field information.");
    }
    
    [MenuItem("Tools/Force Recompile")]
    public static void ForceRecompile()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        Debug.Log("Forced recompile completed. Check for serialization errors.");
    }
} 