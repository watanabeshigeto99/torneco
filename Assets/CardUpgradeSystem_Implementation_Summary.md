# カード強化システム 実装概要

## 実装完了項目

### 1. 安全ガード（必須）
- ✅ フィーチャーフラグ: `CardUpgradeFeature.IsEnabled`
- ✅ 既存システムとの互換性確保
- ✅ 段階的リリース対応

### 2. データ定義（最小）
- ✅ `CardInstance` クラス（所持データ）
- ✅ `UpgradeCurveSO` クラス（強化曲線データ）
- ✅ `UpgradePreview` クラス（強化プレビュー）
- ✅ `UpgradeResult` クラス（強化結果）

### 3. ビジネスロジック
- ✅ `UpgradeService` クラス（強化サービス）
  - `GetPreview()` メソッド
  - `TryUpgrade()` メソッド
  - リソース管理（ゴールド・シャード）
  - 成功率判定

### 4. UI（最小モーダル）
- ✅ `CardUpgradeModal` クラス（強化モーダル）
  - プレビュー表示
  - 強化実行
  - 結果表示
- ✅ `CardUI` クラスに強化ボタン追加

### 5. 既存システムへの組み込み
- ✅ `CardDataSO` にレア度フィールド追加
- ✅ `PlayerDataSO` にカードインスタンス管理機能追加
- ✅ 効果値計算でカードインスタンスレベルを考慮

## 主要ファイル

### 新規作成ファイル
1. `Scripts/Data/CardUpgradeSystem.cs` - 基本データ構造
2. `Scripts/Data/UpgradeCurveSO.cs` - 強化曲線データ
3. `Scripts/Services/UpgradeService.cs` - 強化サービス
4. `Scripts/UI/CardUpgradeModal.cs` - 強化UIモーダル
5. `Scripts/Editor/CardUpgradeSystemTest.cs` - テスト用エディタースクリプト

### 更新ファイル
1. `Scripts/Data/CardDataSO.cs` - レア度フィールド追加
2. `Scripts/Templates/PlayerDataSystem/PlayerDataSO.cs` - カードインスタンス管理機能追加
3. `Scripts/UI/CardUI.cs` - 強化ボタン追加

## 使用方法

### 1. フィーチャーフラグの制御
```csharp
// 強化機能の有効/無効を制御
public static class CardUpgradeFeature
{
    public static bool IsEnabled => UPGRADE_FEATURE;
    private const bool UPGRADE_FEATURE = true; // 開発中はtrue
}
```

### 2. 強化サービスの使用
```csharp
// プレビューを取得
var preview = UpgradeService.Instance.GetPreview("Attack");

// 強化を実行
var result = UpgradeService.Instance.TryUpgrade("Attack");
```

### 3. カードインスタンスの管理
```csharp
// カードインスタンスを取得
var cardInstance = playerData.GetCardInstance("Attack");

// カードインスタンスを追加
playerData.AddCardInstance(new CardInstance("Attack", 1));

// カードインスタンスを更新
playerData.UpdateCardInstance(cardInstance);
```

## 設定項目

### 1. 強化曲線データの作成
1. `Assets > Create > Card Battle > Upgrade Curve Data`
2. レア度ごとに設定
3. レベルごとの倍率、コスト、成功率を設定

### 2. カードデータの設定
1. 既存のカードデータにレア度フィールドを設定
2. 強化曲線データを関連付け

### 3. UIの設定
1. `CardUpgradeModal` プレハブを作成
2. `CardUI` に強化ボタンを追加

## テスト方法

### 1. エディターテスト
1. `Tools > Card Upgrade System Test` を開く
2. 各テストボタンを実行して動作確認

### 2. ランタイムテスト
1. ゲームを起動
2. カード詳細画面で強化ボタンをクリック
3. 強化モーダルでプレビューと強化をテスト

## 今後の拡張予定

### 1. 保護アイテム機能
- `useProtector` パラメータの実装
- 保護アイテムの消費ロジック

### 2. 強化失敗時のダウン/破損
- 失敗時のレベルダウン機能
- カード破損機能

### 3. 強化アニメーション
- 強化成功時のエフェクト
- レベルアップ演出

### 4. 強化履歴
- 強化履歴の保存
- 強化統計の表示

## 注意事項

1. **フィーチャーフラグ**: 本番リリース前は `UPGRADE_FEATURE = false` に設定
2. **データ互換性**: 既存セーブデータとの互換性を確保
3. **パフォーマンス**: 大量のカードインスタンス管理時の最適化
4. **バランス調整**: 強化コストと成功率の調整が必要

## トラブルシューティング

### よくある問題
1. **UpgradeServiceが見つからない**: シーンにUpgradeServiceコンポーネントを追加
2. **PlayerDataManagerが見つからない**: PlayerDataManagerの初期化を確認
3. **カードデータが見つからない**: カードデータのパスと名前を確認

### デバッグ方法
1. コンソールログを確認
2. エディターテストツールを使用
3. フィーチャーフラグの状態を確認
