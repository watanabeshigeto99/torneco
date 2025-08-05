# Deck System テンプレート移行ガイド

## 概要

このガイドは、既存の`GameManager.cs`からデッキシステム機能を分離し、独立したDeck Systemテンプレートに移行するための手順を説明します。

## 目的

### 問題点（Before）
- `GameManager.cs`がデッキ管理も担当（単一責任原則違反）
- デッキ機能が他のシステムと密結合
- デッキシステムのテストが困難
- デッキ機能の拡張が困難

### 解決策（After）
- **DeckManager.cs**: デッキシステム管理専用コンポーネント
- **CardDataSO.cs**: カードデータ管理用ScriptableObject
- **DeckDataSO.cs**: デッキデータ管理用ScriptableObject
- **DeckEventChannel.cs**: デッキシステムイベント管理用ScriptableObject
- **DeckSystemTest.cs**: デッキシステムテスト用コンポーネント

## 新しいコンポーネント

### 1. DeckManager.cs
```csharp
namespace DeckSystem
{
    [DefaultExecutionOrder(-250)]
    public class DeckManager : MonoBehaviour
    {
        // デッキシステム管理専用
        // カード追加/削除、ドロー、シャッフル、リセット機能
    }
}
```

### 2. CardDataSO.cs
```csharp
namespace DeckSystem
{
    [CreateAssetMenu(fileName = "CardData", menuName = "Deck System/Card Data")]
    public class CardDataSO : ScriptableObject
    {
        // カードデータ管理
        // カードタイプ、効果、アップグレード機能
    }
}
```

### 3. DeckDataSO.cs
```csharp
namespace DeckSystem
{
    [CreateAssetMenu(fileName = "DeckData", menuName = "Deck System/Deck Data")]
    public class DeckDataSO : ScriptableObject
    {
        // デッキデータ管理
        // デッキ、手札、捨て札の管理
    }
}
```

### 4. DeckEventChannel.cs
```csharp
namespace DeckSystem
{
    [CreateAssetMenu(fileName = "DeckEventChannel", menuName = "Deck System/Deck Event Channel")]
    public class DeckEventChannel : ScriptableObject
    {
        // デッキシステムイベント管理
        // 疎結合のためのイベントチャンネル
    }
}
```

### 5. DeckSystemTest.cs
```csharp
namespace DeckSystem
{
    public class DeckSystemTest : MonoBehaviour
    {
        // デッキシステムテスト用
        // UI操作とイベントログ機能
    }
}
```

## 移行手順

### Phase 1: テスト環境の準備

1. **テストシーンの作成**
   ```
   Assets/Scenes/DeckSystemTestScene.unity
   ```

2. **ScriptableObjectの作成**
   ```
   Assets/SO/DeckSystem/
   ├── DefaultDeckData.asset
   ├── DefaultDeckEventChannel.asset
   └── TestCards/
       ├── AttackCard.asset
       ├── DefenseCard.asset
       └── HealCard.asset
   ```

3. **テストコンポーネントの配置**
   - DeckManager
   - DeckSystemTest
   - UI要素（ボタン、テキスト）

### Phase 2: GameManager統合

1. **GameManager.csにDeck System統合を追加**
   ```csharp
   [Header("Deck System Integration")]
   public DeckSystem.DeckManager deckManager;
   public DeckSystem.DeckDataSO deckData;
   public DeckSystem.DeckEventChannel deckEventChannel;
   ```

2. **イベント購読の追加**
   ```csharp
   private void SubscribeToDeckEvents()
   {
       if (deckEventChannel != null)
       {
           deckEventChannel.OnDeckChanged.AddListener(OnDeckSystemChanged);
           deckEventChannel.OnCardDrawn.AddListener(OnDeckCardDrawn);
       }
   }
   ```

3. **既存メソッドの条件分岐追加**
   ```csharp
   public void SetPlayerDeck(PlayerDeck deck)
   {
       // デッキシステムが利用可能な場合はそちらを使用
       if (deckManager != null)
       {
           // 新しいデッキシステムを使用
       }
       else
       {
           // 従来の処理（後方互換性のため）
       }
   }
   ```

### Phase 3: 機能移行

1. **カード管理機能の移行**
   - `SetPlayerDeck()` → `DeckManager.AddCardToDeck()`
   - `GetPlayerDeck()` → `DeckManager.GetDeckData()`

2. **ドロー機能の移行**
   - 既存のドロー機能 → `DeckManager.DrawCard()`

3. **シャッフル機能の移行**
   - 既存のシャッフル機能 → `DeckManager.ShuffleDeck()`

### Phase 4: テストと検証

1. **単体テスト**
   - デッキシステムテンプレートの動作確認
   - イベントチャンネルの動作確認
   - コンポーネント間の連携確認

2. **統合テスト**
   - GameManagerとDeck Systemの連携確認
   - 既存機能の動作確認
   - パフォーマンステスト

3. **後方互換性テスト**
   - 既存のデッキ機能が正常に動作することを確認
   - 段階的移行の安全性確認

## 移行時の注意点

### 1. 後方互換性の維持
- 既存の`PlayerDeck`クラスとの互換性を維持
- 段階的移行のための条件分岐を実装

### 2. イベントシステムの活用
- 疎結合のためのイベントチャンネルを使用
- 既存システムとの連携をイベントで実現

### 3. データの永続化
- ScriptableObjectによるデータの永続化
- セーブ/ロード機能との連携

### 4. パフォーマンスの考慮
- デッキサイズの制限
- メモリ使用量の最適化

## 期待される効果

### 1. 保守性の向上
- デッキシステムが独立したコンポーネントに
- 単一責任原則の実現
- コードの可読性向上

### 2. 拡張性の向上
- 新しいカードタイプの追加が容易
- デッキシステムの機能拡張が簡単
- 他のシステムとの連携が柔軟

### 3. テスト容易性の向上
- デッキシステムの単体テストが可能
- 独立したテスト環境の構築
- デバッグの効率化

### 4. 再利用性の向上
- 他のプロジェクトでの再利用が可能
- モジュール化されたデッキシステム
- 標準化されたインターフェース

## 次のステップ

1. **UI System テンプレートの作成**
   - UIManager.cs
   - UIDataSO.cs
   - UIEventChannel.cs
   - UISystemTest.cs

2. **最終的な統合テスト**
   - 全システムテンプレートの連携確認
   - パフォーマンステスト
   - 安定性テスト

3. **段階的な旧コード削除**
   - 新しいシステムの安定化後
   - 既存機能の完全移行後
   - テスト完了後の安全な削除

---

**注意**: この移行は段階的に行い、各段階で十分なテストを行ってください。既存の機能を損なわないよう、慎重に進めてください。 