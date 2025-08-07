using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

public class CodeQualityAnalyzer : EditorWindow
{
    [MenuItem("Tools/Analyze Code Quality")]
    public static void AnalyzeCodeQuality()
    {
        Debug.Log("=== コード品質分析開始 ===");
        
        // 1. 循環複雑度の分析
        AnalyzeCyclomaticComplexity();
        
        // 2. 依存関係の分析
        AnalyzeDependencies();
        
        // 3. テストカバレッジの推定
        AnalyzeTestCoverage();
        
        // 4. パフォーマンスの分析
        AnalyzePerformance();
        
        // 5. 保守性の分析
        AnalyzeMaintainability();
        
        Debug.Log("=== コード品質分析完了 ===");
    }
    
    private static void AnalyzeCyclomaticComplexity()
    {
        Debug.Log("1. 循環複雑度分析:");
        
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var complexMethods = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    // メソッドの複雑度を推定（パラメータ数、戻り値の型などから）
                    int complexity = EstimateComplexity(method);
                    if (complexity > 10)
                    {
                        complexMethods.Add($"{type.Name}.{method.Name} (複雑度: {complexity})");
                    }
                }
            }
        }
        
        if (complexMethods.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ 複雑なメソッド ({complexMethods.Count}個):");
            foreach (var method in complexMethods.Take(5)) // 上位5個のみ表示
            {
                Debug.LogWarning($"    - {method}");
            }
        }
        else
        {
            Debug.Log("  ✅ 複雑なメソッドは見つかりませんでした");
        }
    }
    
    private static int EstimateComplexity(MethodInfo method)
    {
        int complexity = 1; // 基本複雑度
        
        // パラメータ数による複雑度
        complexity += method.GetParameters().Length;
        
        // 戻り値の型による複雑度
        if (method.ReturnType != typeof(void))
        {
            complexity += 1;
        }
        
        // メソッド名による推定
        if (method.Name.Contains("Handle") || method.Name.Contains("Process"))
        {
            complexity += 2;
        }
        
        return complexity;
    }
    
    private static void AnalyzeDependencies()
    {
        Debug.Log("2. 依存関係分析:");
        
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var highDependencyClasses = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                // 依存関係の数を推定
                int dependencyCount = EstimateDependencies(type);
                if (dependencyCount > 8)
                {
                    highDependencyClasses.Add($"{type.Name} (依存関係: {dependencyCount}個)");
                }
            }
        }
        
        if (highDependencyClasses.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ 高依存クラス ({highDependencyClasses.Count}個):");
            foreach (var className in highDependencyClasses.Take(5))
            {
                Debug.LogWarning($"    - {className}");
            }
        }
        else
        {
            Debug.Log("  ✅ 高依存クラスは見つかりませんでした");
        }
    }
    
    private static int EstimateDependencies(System.Type type)
    {
        int dependencies = 0;
        
        // フィールドの数
        dependencies += type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length;
        
        // プロパティの数
        dependencies += type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length;
        
        // メソッドの数（重みを下げる）
        dependencies += type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length / 2;
        
        return dependencies;
    }
    
    private static void AnalyzeTestCoverage()
    {
        Debug.Log("3. テストカバレッジ推定:");
        
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var testableClasses = new List<string>();
        var testedClasses = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass && !type.IsAbstract && !type.Name.Contains("Editor"))
            {
                // テスト可能なクラスを特定
                if (typeof(MonoBehaviour).IsAssignableFrom(type) || type.Name.EndsWith("Manager"))
                {
                    testableClasses.Add(type.Name);
                    
                    // テストクラスの存在を確認
                    var testClassName = type.Name + "Test";
                    var testType = allTypes.FirstOrDefault(t => t.Name == testClassName);
                    if (testType != null)
                    {
                        testedClasses.Add(type.Name);
                    }
                }
            }
        }
        
        int coverage = testableClasses.Count > 0 ? (testedClasses.Count * 100) / testableClasses.Count : 0;
        
        if (coverage < 50)
        {
            Debug.LogWarning($"  ⚠️ テストカバレッジ: {coverage}% (低い)");
            Debug.LogWarning($"    テスト対象クラス: {testableClasses.Count}個");
            Debug.LogWarning($"    テスト済みクラス: {testedClasses.Count}個");
        }
        else
        {
            Debug.Log($"  ✅ テストカバレッジ: {coverage}% (良好)");
        }
    }
    
    private static void AnalyzePerformance()
    {
        Debug.Log("4. パフォーマンス分析:");
        
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var performanceIssues = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    // Update()メソッドの存在チェック
                    if (method.Name == "Update" && method.GetParameters().Length == 0)
                    {
                        performanceIssues.Add($"{type.Name}.Update() - 毎フレーム実行される可能性");
                    }
                    
                    // 重い処理の推定
                    if (method.Name.Contains("Find") || method.Name.Contains("GetComponents"))
                    {
                        performanceIssues.Add($"{type.Name}.{method.Name} - 重い処理の可能性");
                    }
                }
            }
        }
        
        if (performanceIssues.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ パフォーマンス問題 ({performanceIssues.Count}個):");
            foreach (var issue in performanceIssues.Take(5))
            {
                Debug.LogWarning($"    - {issue}");
            }
        }
        else
        {
            Debug.Log("  ✅ パフォーマンス問題は見つかりませんでした");
        }
    }
    
    private static void AnalyzeMaintainability()
    {
        Debug.Log("5. 保守性分析:");
        
        var allTypes = Assembly.GetExecutingAssembly().GetTypes();
        var maintainabilityIssues = new List<string>();
        
        foreach (var type in allTypes)
        {
            if (type.IsClass && !type.IsAbstract)
            {
                // クラスサイズのチェック
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (methods.Length > 20)
                {
                    maintainabilityIssues.Add($"{type.Name} - クラスが大きすぎます ({methods.Length}メソッド)");
                }
                
                // 責務の分散チェック
                if (type.Name.EndsWith("Manager") && methods.Length < 5)
                {
                    maintainabilityIssues.Add($"{type.Name} - Managerクラスが小さすぎます");
                }
            }
        }
        
        if (maintainabilityIssues.Count > 0)
        {
            Debug.LogWarning($"  ⚠️ 保守性問題 ({maintainabilityIssues.Count}個):");
            foreach (var issue in maintainabilityIssues.Take(5))
            {
                Debug.LogWarning($"    - {issue}");
            }
        }
        else
        {
            Debug.Log("  ✅ 保守性問題は見つかりませんでした");
        }
    }
}
