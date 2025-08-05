# UI System テンプレート移行ガイド

## 概要

このガイドは、既存の`UIManager.cs`からUIシステム機能を分離し、独立したUI Systemテンプレートに移行するための手順を説明します。

## 目的

### 問題点（Before）
- `UIManager.cs`がUI表示、イベント処理、ログ管理、シーン遷移を混在（単一責任原則違反）
- UI機能が他のシステムと密結合
- UIシステムのテストが困難
- UI機能の拡張が困難

### 解決策（After）
- **UIManager.cs**: UIシステム管理専用コンポーネント
- **UIDataSO.cs**: UIデータ管理用ScriptableObject
- **UIEventChannel.cs**: UIシステムイベント管理用ScriptableObject
- **UISystemTest.cs**: UIシステムテスト用コンポーネント

## 新しいコンポーネント

### 1. UIManager.cs
```csharp
namespace UISystem
{
    [DefaultExecutionOrder(-300)]
    public class UIManager : MonoBehaviour
    {
        // UIシステム管理専用
        // HUD表示、ログ管理、画面遷移、カードUI管理
    }
}
```

### 2. UIDataSO.cs
```csharp
namespace UISystem
{
    [CreateAssetMenu(fileName = "UIData", menuName = "UI System/UI Data")]
    public class UIDataSO : ScriptableObject
    {
        // UIデータ管理
        // プレイヤー状態、UI設定、ログシステム、カードUI
    }
}
```

### 3. UIEventChannel.cs
```csharp
namespace UISystem
{
    [CreateAssetMenu(fileName = "UIEventChannel", menuName = "UI System/UI Event Channel")]
    public class UIEventChannel : ScriptableObject
    {
        // UIシステムイベント管理
        // 疎結合のためのイベントチャンネル
    }
}
```

### 4. UISystemTest.cs
```csharp
namespace UISystem
{
    public class UISystemTest : MonoBehaviour
    {
        // UIシステムテスト用
        // UI操作とイベントログ機能
    }
}
```

## 移行手順

### Phase 1: テスト環境の準備

1. **テストシーンの作成**
   ```
   Assets/Scenes/UISystemTestScene.unity
   ```

2. **ScriptableObjectの作成**
   ```
   Assets/SO/UISystem/
   ├── DefaultUIData.asset
   ├── DefaultUIEventChannel.asset
   └── TestCards/
       ├── AttackCard.asset
       ├── DefenseCard.asset
       └── HealCard.asset
   ```

3. **テストコンポーネントの配置**
   - UIManager
   - UISystemTest
   - UI要素（ボタン、テキスト、スライダー）

### Phase 2: GameManager統合

1. **GameManager.csにUI System統合を追加**
   ```csharp
   [Header("UI System Integration")]
   public UISystem.UIManager uiManager;
   public UISystem.UIDataSO uiData;
   public UISystem.UIEventChannel uiEventChannel;
   ```

2. **イベント購読の追加**
   ```csharp
   private void SubscribeToUIEvents()
   {
       if (uiEventChannel != null)
       {
           uiEventChannel.OnUIDataChanged.AddListener(OnUISystemChanged);
           uiEventChannel.OnLogAdded.AddListener(OnUILogAdded);
       }
   }
   ```

3. **既存メソッドの条件分岐追加**
   ```csharp
   public void AddLog(string message)
   {
       // UIシステムが利用可能な場合はそちらを使用
       if (uiManager != null)
       {
           uiManager.AddLog(message);
       }
       else
       {
           // 従来の処理（後方互換性のため）
       }
   }
   ```

### Phase 3: 機能移行

1. **HUD表示機能の移行**
   - `UpdateHP()` → `UIManager.UpdateHP()`
   - `UpdateLevel()` → `UIManager.UpdateLevelDisplay()`
   - `UpdateScore()` → `UIManager.UpdateScore()`

2. **ログ管理機能の移行**
   - `AddLog()` → `UIManager.AddLog()`
   - `ClearLog()` → `UIManager.ClearLog()`

3. **画面遷移機能の移行**
   - `ShowPauseMenu()` → `UIManager.ShowPauseMenu()`
   - `HidePauseMenu()` → `UIManager.HidePauseMenu()`
   - `ShowLoadingScreen()` → `UIManager.ShowLoadingScreen()`

4. **カードUI機能の移行**
   - `CreateCardUI()` → `UIManager.CreateCardUI()`
   - `RemoveCardUI()` → `UIManager.RemoveCardUI()`
   - `ClearAllCardUI()` → `UIManager.ClearAllCardUI()`

### Phase 4: テストと検証

1. **単体テスト**
   - UIシステムテンプレートの動作確認
   - イベントチャンネルの動作確認
   - コンポーネント間の連携確認

2. **統合テスト**
   - GameManagerとUI Systemの連携確認
   - 既存機能の動作確認
   - パフォーマンステスト

3. **後方互換性テスト**
   - 既存のUI機能が正常に動作することを確認
   - 段階的移行の安全性確認

## 移行時の注意点

### 1. 後方互換性の維持
- 既存のUI機能との互換性を維持
- 段階的移行のための条件分岐を実装

### 2. イベントシステムの活用
- 疎結合のためのイベントチャンネルを使用
- 既存システムとの連携をイベントで実現

### 3. データの永続化
- ScriptableObjectによるデータの永続化
- セーブ/ロード機能との連携

### 4. パフォーマンスの考慮
- UI要素の表示/非表示制御
- メモリ使用量の最適化

## 期待される効果

### 1. 保守性の向上
- UIシステムが独立したコンポーネントに
- 単一責任原則の実現
- コードの可読性向上

### 2. 拡張性の向上
- 新しいUI要素の追加が容易
- UIシステムの機能拡張が簡単
- 他のシステムとの連携が柔軟

### 3. テスト容易性の向上
- UIシステムの単体テストが可能
- 独立したテスト環境の構築
- デバッグの効率化

### 4. 再利用性の向上
- 他のプロジェクトでの再利用が可能
- モジュール化されたUIシステム
- 標準化されたインターフェース

## 次のステップ

1. **最終的な統合テスト**
   - 全システムテンプレートの連携確認
   - パフォーマンステスト
   - 安定性テスト

2. **段階的な旧コード削除**
   - 新しいシステムの安定化後
   - 既存機能の完全移行後
   - テスト完了後の安全な削除

3. **プロジェクト再構築の完了**
   - すべてのシステムテンプレートの統合
   - 最終的な安定性確認
   - ドキュメントの更新

---

**注意**: この移行は段階的に行い、各段階で十分なテストを行ってください。既存の機能を損なわないよう、慎重に進めてください。 