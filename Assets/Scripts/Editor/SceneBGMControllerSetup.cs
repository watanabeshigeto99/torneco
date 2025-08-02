using UnityEngine;
using UnityEditor;

public class SceneBGMControllerSetup : EditorWindow
{
    [MenuItem("Tools/Setup SceneBGMController")]
    public static void SetupSceneBGMController()
    {
        // SceneBGMControllerが既に存在するかチェック
        SceneBGMController existingController = FindObjectOfType<SceneBGMController>();
        if (existingController != null)
        {
            Debug.Log("SceneBGMController already exists in the scene");
            return;
        }
        
        // SceneBGMControllerを作成
        GameObject controllerObj = new GameObject("SceneBGMController");
        SceneBGMController controller = controllerObj.AddComponent<SceneBGMController>();
        
        Debug.Log("SceneBGMController created and added to the scene");
    }
    
    [MenuItem("Tools/Verify SceneBGMController Setup")]
    public static void VerifySceneBGMControllerSetup()
    {
        SceneBGMController controller = FindObjectOfType<SceneBGMController>();
        
        if (controller == null)
        {
            Debug.LogError("SceneBGMController not found in the scene!");
        }
        else
        {
            Debug.Log("SceneBGMController is properly set up!");
        }
    }
} 