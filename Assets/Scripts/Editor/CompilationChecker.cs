using UnityEngine;
using UnityEditor;

public class CompilationChecker : EditorWindow
{
    [MenuItem("Tools/Check Compilation")]
    public static void CheckCompilation()
    {
        Debug.Log("=== コンパイルエラーチェック ===");
        
        // 主要なクラスの存在確認
        CheckClassExists("CardExecutor");
        CheckClassExists("GameStateManager");
        CheckClassExists("EnemyManager");
        CheckClassExists("CardManager");
        
        // CardType enumの確認
        CheckCardTypeEnum();
        
        // メソッドの存在確認
        CheckMethodExists("EnemyManager", "ExecuteEnemyTurns");
        CheckMethodExists("CardExecutor", "ExecuteCardEffect");
        
        Debug.Log("=== コンパイルエラーチェック完了 ===");
    }
    
    private static void CheckClassExists(string className)
    {
        System.Type type = System.Type.GetType(className);
        if (type != null)
        {
            Debug.Log($"✅ {className}クラスが存在します");
        }
        else
        {
            Debug.LogError($"❌ {className}クラスが見つかりません");
        }
    }
    
    private static void CheckCardTypeEnum()
    {
        System.Type cardTypeEnum = System.Type.GetType("CardType");
        if (cardTypeEnum != null && cardTypeEnum.IsEnum)
        {
            var values = System.Enum.GetValues(cardTypeEnum);
            bool hasSpecial = false;
            foreach (var value in values)
            {
                if (value.ToString() == "Special")
                {
                    hasSpecial = true;
                    break;
                }
            }
            
            if (hasSpecial)
            {
                Debug.Log("✅ CardType enumにSpecialが含まれています");
            }
            else
            {
                Debug.LogError("❌ CardType enumにSpecialが含まれていません");
            }
        }
        else
        {
            Debug.LogError("❌ CardType enumが見つかりません");
        }
    }
    
    private static void CheckMethodExists(string className, string methodName)
    {
        System.Type type = System.Type.GetType(className);
        if (type != null)
        {
            var method = type.GetMethod(methodName);
            if (method != null)
            {
                Debug.Log($"✅ {className}.{methodName}メソッドが存在します");
            }
            else
            {
                Debug.LogError($"❌ {className}.{methodName}メソッドが見つかりません");
            }
        }
        else
        {
            Debug.LogError($"❌ {className}クラスが見つからないため、{methodName}メソッドを確認できません");
        }
    }
} 