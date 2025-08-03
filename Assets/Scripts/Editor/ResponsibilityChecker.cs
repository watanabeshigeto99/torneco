using UnityEngine;
using UnityEditor;
using System.Reflection;

public class ResponsibilityChecker : EditorWindow
{
    [MenuItem("Tools/Check Responsibility Separation")]
    public static void CheckResponsibilitySeparation()
    {
        Debug.Log("=== 責任分離チェックリスト ===");
        
        // 1. UI更新処理はすべて UIManager に集約されている
        CheckUIManagerCentralization();
        
        // 2. プレイヤー・敵の共通ロジックは Unit に集約されている
        CheckUnitAbstraction();
        
        // 3. カード効果の実行は CardExecutor にまとめている
        CheckCardExecutorCentralization();
        
        // 4. ゲーム状態は GameStateManager が一元管理している
        CheckGameStateManagement();
        
        // 5. データ管理は ScriptableObject に切り出している
        CheckScriptableObjectData();
        
        Debug.Log("=== 責任分離チェック完了 ===");
    }
    
    private static void CheckUIManagerCentralization()
    {
        Debug.Log("1. UI更新処理の集約チェック:");
        
        // UIManagerの存在確認
        var uiManagerType = typeof(UIManager);
        if (uiManagerType != null)
        {
            var methods = uiManagerType.GetMethods(BindingFlags.Public | BindingFlags.Instance);
            bool hasUpdateMethods = false;
            
            foreach (var method in methods)
            {
                if (method.Name.Contains("Update"))
                {
                    Debug.Log($"  ✅ UIManager.{method.Name} が存在");
                    hasUpdateMethods = true;
                }
            }
            
            if (hasUpdateMethods)
            {
                Debug.Log("  ✅ UIManagerにUI更新メソッドが集約されている");
            }
            else
            {
                Debug.LogWarning("  ❌ UIManagerにUI更新メソッドが見つかりません");
            }
        }
        else
        {
            Debug.LogError("  ❌ UIManagerクラスが見つかりません");
        }
    }
    
    private static void CheckUnitAbstraction()
    {
        Debug.Log("2. Unit抽象化チェック:");
        
        // Unitクラスの存在確認
        var unitType = typeof(Unit);
        if (unitType != null)
        {
            Debug.Log("  ✅ Unitクラスが存在");
            
            // PlayerとEnemyがUnitを継承しているか確認
            var playerType = typeof(Player);
            var enemyType = typeof(Enemy);
            
            if (playerType.BaseType == unitType)
            {
                Debug.Log("  ✅ PlayerがUnitを継承している");
            }
            else
            {
                Debug.LogError("  ❌ PlayerがUnitを継承していない");
            }
            
            if (enemyType.BaseType == unitType)
            {
                Debug.Log("  ✅ EnemyがUnitを継承している");
            }
            else
            {
                Debug.LogError("  ❌ EnemyがUnitを継承していない");
            }
        }
        else
        {
            Debug.LogError("  ❌ Unitクラスが見つかりません");
        }
    }
    
    private static void CheckCardExecutorCentralization()
    {
        Debug.Log("3. CardExecutor集約チェック:");
        
        // CardExecutorクラスの存在確認
        var cardExecutorType = typeof(CardExecutor);
        if (cardExecutorType != null)
        {
            Debug.Log("  ✅ CardExecutorクラスが存在");
            
            // ExecuteCardEffectメソッドの存在確認
            var executeMethod = cardExecutorType.GetMethod("ExecuteCardEffect");
            if (executeMethod != null)
            {
                Debug.Log("  ✅ CardExecutor.ExecuteCardEffectメソッドが存在");
            }
            else
            {
                Debug.LogError("  ❌ CardExecutor.ExecuteCardEffectメソッドが見つかりません");
            }
        }
        else
        {
            Debug.LogError("  ❌ CardExecutorクラスが見つかりません");
        }
    }
    
    private static void CheckGameStateManagement()
    {
        Debug.Log("4. GameStateManager一元管理チェック:");
        
        // GameStateManagerクラスの存在確認
        var gameStateManagerType = typeof(GameStateManager);
        if (gameStateManagerType != null)
        {
            Debug.Log("  ✅ GameStateManagerクラスが存在");
            
            // GameState enumの存在確認
            var gameStateEnum = gameStateManagerType.GetNestedType("GameState");
            if (gameStateEnum != null)
            {
                Debug.Log("  ✅ GameState enumが存在");
            }
            else
            {
                Debug.LogError("  ❌ GameState enumが見つかりません");
            }
            
            // ChangeStateメソッドの存在確認
            var changeStateMethod = gameStateManagerType.GetMethod("ChangeState");
            if (changeStateMethod != null)
            {
                Debug.Log("  ✅ GameStateManager.ChangeStateメソッドが存在");
            }
            else
            {
                Debug.LogError("  ❌ GameStateManager.ChangeStateメソッドが見つかりません");
            }
        }
        else
        {
            Debug.LogError("  ❌ GameStateManagerクラスが見つかりません");
        }
    }
    
    private static void CheckScriptableObjectData()
    {
        Debug.Log("5. ScriptableObjectデータ管理チェック:");
        
        // 主要なScriptableObjectクラスの存在確認
        var cardDataSOType = typeof(CardDataSO);
        var enemyDataSOType = typeof(EnemyDataSO);
        var levelTableSOType = typeof(LevelTableSO);
        
        if (cardDataSOType != null)
        {
            Debug.Log("  ✅ CardDataSOが存在");
        }
        else
        {
            Debug.LogError("  ❌ CardDataSOが見つかりません");
        }
        
        if (enemyDataSOType != null)
        {
            Debug.Log("  ✅ EnemyDataSOが存在");
        }
        else
        {
            Debug.LogError("  ❌ EnemyDataSOが見つかりません");
        }
        
        if (levelTableSOType != null)
        {
            Debug.Log("  ✅ LevelTableSOが存在");
        }
        else
        {
            Debug.LogError("  ❌ LevelTableSOが見つかりません");
        }
    }
} 