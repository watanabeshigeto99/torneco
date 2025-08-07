using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public class TechnicalDebtChecker : EditorWindow
{
    [MenuItem("Tools/Check Technical Debt")]
    public static void CheckTechnicalDebt()
    {
        Debug.Log("=== 技術負債チェック開始 ===");
        
        // 1. 非効率な処理のチェック
        CheckInefficientCode();
        
        // 2. Godクラスのチェック
        CheckGodClasses();
        
        // 3. 重複コードのチェック
        CheckDuplicateCode();
        
        // 4. 未使用コードのチェック
        CheckUnusedCode();
        
        // 5. 不適切な命名のチェック
        CheckNamingConventions();
        
        // 6. 固定的な設計のチェック
        CheckFixedDesign();
        
        // 7. スパゲッティコードのチェック
        CheckSpaghettiCode();
        
        Debug.Log("=== 技術負債チェック完了 ===");
    }
    
    private static void CheckInefficientCode()
    {
        Debug.Log("1. 非効率な処理チェック:");
        
        // Update()メソッドの使用状況をチェック
        var monoBehaviours = FindObjectsOfType<MonoBehaviour>();
        int updateCount = 0;
        
        foreach (var mb in monoBehaviours)
        {
            var type = mb.GetType();
            var updateMethod = type.GetMethod("Update", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (updateMethod != null)
            {
                updateCount++;
            }
        }
        
        if (updateCount > 20)
        {
            Debug.LogWarning($"  ⚠️ Update()メソッドが{updateCount}個存在します。パフォーマンスに注意してください。");
        }
        else
        {
            Debug.Log($"  ✅ Update()メソッド数: {updateCount} (適切)");
        }
    }
    
    private static void CheckGodClasses()
    {
        Debug.Log("2. Godクラスチェック:");
        
        // 大きなクラスのチェック
        var largeClasses = new List<string>();
        
        // GameManagerのチェック
        var gameManagerType = typeof(GameManager);
        if (gameManagerType != null)
        {
            var methods = gameManagerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methods.Length > 30)
            {
                Debug.LogWarning("  ⚠️ GameManagerが大きすぎます。責務を分離することを検討してください。");
                largeClasses.Add("GameManager");
            }
            else
            {
                Debug.Log($"  ✅ GameManager: {methods.Length}メソッド (適切)");
            }
        }
        
        // GridManagerのチェック
        var gridManagerType = typeof(GridManager);
        if (gridManagerType != null)
        {
            var methods = gridManagerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (methods.Length > 25)
            {
                Debug.LogWarning("  ⚠️ GridManagerが大きすぎます。視界管理などは分離済みです。");
                largeClasses.Add("GridManager");
            }
            else
            {
                Debug.Log($"  ✅ GridManager: {methods.Length}メソッド (適切)");
            }
        }
        
        if (largeClasses.Count == 0)
        {
            Debug.Log("  ✅ Godクラスは見つかりませんでした");
        }
    }
    
    private static void CheckDuplicateCode()
    {
        Debug.Log("3. 重複コードチェック:");
        
        // 類似メソッド名のチェック
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var methodNames = new Dictionary<string, List<string>>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (!methodNames.ContainsKey(method.Name))
                    {
                        methodNames[method.Name] = new List<string>();
                    }
                    methodNames[method.Name].Add(type.Name);
                }
            }
        }
        
        int duplicateCount = 0;
        foreach (var kvp in methodNames)
        {
            if (kvp.Value.Count > 1)
            {
                Debug.LogWarning($"  ⚠️ 重複メソッド名: {kvp.Key} ({string.Join(", ", kvp.Value)})");
                duplicateCount++;
            }
        }
        
        if (duplicateCount == 0)
        {
            Debug.Log("  ✅ 重複コードは見つかりませんでした");
        }
    }
    
    private static void CheckUnusedCode()
    {
        Debug.Log("4. 未使用コードチェック:");
        
        // 未使用のMonoBehaviourをチェック
        var allMonoBehaviours = FindObjectsOfType<MonoBehaviour>();
        var unusedComponents = new List<string>();
        
        foreach (var mb in allMonoBehaviours)
        {
            if (mb.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length == 0)
            {
                unusedComponents.Add(mb.GetType().Name);
            }
        }
        
        if (unusedComponents.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ 未使用コンポーネント: {string.Join(", ", unusedComponents)}");
        }
        else
        {
            Debug.Log("  ✅ 未使用コードは見つかりませんでした");
        }
    }
    
    private static void CheckNamingConventions()
    {
        Debug.Log("5. 命名規則チェック:");
        
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var namingIssues = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass)
            {
                // クラス名がManagerで終わるが、実際にManagerパターンを使用していない場合
                if (type.Name.EndsWith("Manager") && !typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    namingIssues.Add($"{type.Name} - Managerパターンを使用していません");
                }
                
                // メソッド名のチェック
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (method.Name.Length > 30)
                    {
                        namingIssues.Add($"{type.Name}.{method.Name} - メソッド名が長すぎます");
                    }
                }
            }
        }
        
        if (namingIssues.Count > 0)
        {
            foreach (var issue in namingIssues)
            {
                Debug.LogWarning($"  ⚠️ {issue}");
            }
        }
        else
        {
            Debug.Log("  ✅ 命名規則は適切です");
        }
    }
    
    private static void CheckFixedDesign()
    {
        Debug.Log("6. 固定的な設計チェック:");
        
        // ハードコードされた値のチェック
        var hardcodedIssues = new List<string>();
        
        // マジックナンバーのチェック
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        foreach (var type in allTypes)
        {
            if (type.IsClass)
            {
                var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var field in fields)
                {
                    if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
                    {
                        // 定数フィールドのチェック
                        if (field.IsLiteral && !field.Name.Contains("Default") && !field.Name.Contains("Max"))
                        {
                            hardcodedIssues.Add($"{type.Name}.{field.Name} - ハードコードされた値");
                        }
                    }
                }
            }
        }
        
        if (hardcodedIssues.Count > 0)
        {
            foreach (var issue in hardcodedIssues)
            {
                Debug.LogWarning($"  ⚠️ {issue}");
            }
        }
        else
        {
            Debug.Log("  ✅ ハードコードされた値は見つかりませんでした");
        }
    }
    
    private static void CheckSpaghettiCode()
    {
        Debug.Log("7. スパゲッティコードチェック:");
        
        // 長いメソッドのチェック
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var longMethods = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    // メソッドの行数を概算（実際の行数は取得困難なため、パラメータ数と戻り値で判断）
                    var parameters = method.GetParameters();
                    if (parameters.Length > 5)
                    {
                        longMethods.Add($"{type.Name}.{method.Name} - パラメータが多すぎます ({parameters.Length}個)");
                    }
                }
            }
        }
        
        if (longMethods.Count > 0)
        {
            foreach (var method in longMethods)
            {
                Debug.LogWarning($"  ⚠️ {method}");
            }
        }
        else
        {
            Debug.Log("  ✅ 複雑なメソッドは見つかりませんでした");
        }
    }
}
