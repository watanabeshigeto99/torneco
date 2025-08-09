using UnityEngine;
using UnityEditor;

/// <summary>
/// カード強化システムのテスト用エディタースクリプト
/// </summary>
public class CardUpgradeSystemTest : EditorWindow
{
    [MenuItem("Tools/Card Upgrade System Test")]
    public static void ShowWindow()
    {
        GetWindow<CardUpgradeSystemTest>("Card Upgrade Test");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("カード強化システム テスト", EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        // フィーチャーフラグの状態を表示
        GUILayout.Label($"強化機能有効: {CardUpgradeFeature.IsEnabled}");
        
        GUILayout.Space(10);
        
        // テストボタン
        if (GUILayout.Button("強化サービステスト"))
        {
            TestUpgradeService();
        }
        
        if (GUILayout.Button("カードデータテスト"))
        {
            TestCardData();
        }
        
        if (GUILayout.Button("プレイヤーデータテスト"))
        {
            TestPlayerData();
        }
    }
    
    private void TestUpgradeService()
    {
        Debug.Log("=== 強化サービステスト開始 ===");
        
        if (UpgradeService.Instance == null)
        {
            Debug.LogError("UpgradeServiceが見つかりません");
            return;
        }
        
        // テスト用カードID
        string testCardId = "Attack";
        
        // プレビューを取得
        var preview = UpgradeService.Instance.GetPreview(testCardId);
        if (preview != null)
        {
            Debug.Log($"プレビュー取得成功: {preview.cardId} Lv.{preview.currentLevel} → Lv.{preview.nextLevel}");
            Debug.Log($"必要リソース: Gold={preview.requiredGold}, Shards={preview.requiredShards}");
            Debug.Log($"成功率: {preview.successRate * 100:F0}%");
        }
        else
        {
            Debug.LogWarning("プレビューの取得に失敗しました");
        }
        
        Debug.Log("=== 強化サービステスト完了 ===");
    }
    
    private void TestCardData()
    {
        Debug.Log("=== カードデータテスト開始 ===");
        
        // カードデータを取得
        var cardData = Resources.Load<CardDataSO>("SO/Card/Attack");
        if (cardData != null)
        {
            Debug.Log($"カードデータ取得成功: {cardData.cardName}");
            Debug.Log($"レア度: {cardData.rarity}");
            Debug.Log($"基本パワー: {cardData.power}");
            Debug.Log($"効果的パワー: {cardData.GetEffectivePower()}");
        }
        else
        {
            Debug.LogWarning("カードデータの取得に失敗しました");
        }
        
        Debug.Log("=== カードデータテスト完了 ===");
    }
    
    private void TestPlayerData()
    {
        Debug.Log("=== プレイヤーデータテスト開始 ===");
        
        if (PlayerDataSystem.PlayerDataManager.Instance != null && 
            PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData() != null)
        {
            var playerData = PlayerDataSystem.PlayerDataManager.Instance.GetPlayerData();
            Debug.Log($"プレイヤーデータ取得成功: {playerData.GetPlayerInfo()}");
            Debug.Log($"所持カード数: {playerData.ownedCards.Count}");
            Debug.Log($"ゴールド: {playerData.gold}, シャード: {playerData.shards}");
        }
        else
        {
            Debug.LogWarning("プレイヤーデータの取得に失敗しました");
        }
        
        Debug.Log("=== プレイヤーデータテスト完了 ===");
    }
}
