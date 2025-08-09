# カード強化システム エディタ設定手順

## 前提条件

- Unity 2022.3以降
- 既存のカードゲームプロジェクト
- カード強化システムのスクリプトが実装済み

## 1. 強化曲線データの作成

### 1-1. 基本強化曲線の作成

1. **Project ウィンドウ**で `Assets/SO/` フォルダを右クリック
2. **Create > Card Battle > Upgrade Curve Data** を選択
3. ファイル名を `DefaultUpgradeCurve` として作成

### 1-2. 強化曲線の設定

作成した `DefaultUpgradeCurve` を選択し、Inspectorで以下の設定を行います：

```
Rarity Settings:
- Rarity: Normal
- Max Level: 8

Value Growth:
- Value Per Level: [1.0, 1.2, 1.4, 1.6, 1.8, 2.0, 2.2, 2.4, 2.6, 2.8, 3.0, 3.2, 3.4, 3.6, 3.8]

Upgrade Costs:
- Cost Gold: [100, 200, 300, 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1300, 1400, 1500]
- Cost Shards: [10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150]

Success Rates:
- Success Rate: [1.0, 0.95, 0.90, 0.85, 0.80, 0.75, 0.70, 0.65, 0.60, 0.55, 0.50, 0.45, 0.40, 0.35, 0.30]
```

### 1-3. レア度別強化曲線の作成

各レア度用の強化曲線を作成します：

1. **Rare用強化曲線**
   - ファイル名: `RareUpgradeCurve`
   - Rarity: Rare
   - Max Level: 10
   - より高い倍率とコストを設定

2. **SuperRare用強化曲線**
   - ファイル名: `SuperRareUpgradeCurve`
   - Rarity: SuperRare
   - Max Level: 12
   - さらに高い倍率とコストを設定

3. **UltraRare用強化曲線**
   - ファイル名: `UltraRareUpgradeCurve`
   - Rarity: UltraRare
   - Max Level: 15
   - 最高倍率とコストを設定

## 2. カードデータの設定

### 2-1. 既存カードデータの更新

1. **Project ウィンドウ**で `Assets/SO/Card/` フォルダを開く
2. 各カードデータ（例：`Attack.asset`）を選択
3. Inspectorで以下の設定を追加：

```
Rarity:
- Rarity: Normal (または適切なレア度)

Level System:
- Level: 1
- Max Level: 5 (または適切な最大レベル)

Upgrade Data:
- Upgrade Data: 対応する強化曲線データをアサイン
```

### 2-2. カードデータの一括設定

複数のカードデータを同時に設定する場合：

1. **Project ウィンドウ**で `Assets/SO/Card/` フォルダを選択
2. **Ctrl+A** で全選択
3. Inspectorで共通設定を適用
4. 個別にレア度や強化曲線を調整

## 3. UpgradeServiceの設定

### 3-1. UpgradeServiceコンポーネントの追加

1. **Hierarchy ウィンドウ**で空のGameObjectを作成
2. 名前を `UpgradeService` に変更
3. **Add Component** で `UpgradeService` スクリプトを追加

### 3-2. UpgradeServiceの設定

Inspectorで以下の設定を行います：

```
Upgrade Curves:
- Size: 4 (レア度数)
- Element 0: DefaultUpgradeCurve (Normal)
- Element 1: RareUpgradeCurve (Rare)
- Element 2: SuperRareUpgradeCurve (SuperRare)
- Element 3: UltraRareUpgradeCurve (UltraRare)

Default Settings:
- Default Upgrade Curve: DefaultUpgradeCurve
```

## 4. UIの設定

### 4-1. CardUpgradeModalプレハブの作成

1. **Hierarchy ウィンドウ**で空のGameObjectを作成
2. 名前を `CardUpgradeModal` に変更
3. **Add Component** で `CardUpgradeModal` スクリプトを追加

### 4-2. モーダルUIの構築

以下の階層構造でUIを作成：

```
CardUpgradeModal
├── ModalPanel (GameObject)
│   ├── Background (Image)
│   ├── CardIcon (Image)
│   ├── CardNameText (TextMeshProUGUI)
│   ├── CurrentLevelText (TextMeshProUGUI)
│   ├── NextLevelText (TextMeshProUGUI)
│   ├── CurrentValueText (TextMeshProUGUI)
│   ├── NextValueText (TextMeshProUGUI)
│   ├── SuccessRateText (TextMeshProUGUI)
│   ├── RequiredGoldText (TextMeshProUGUI)
│   ├── RequiredShardsText (TextMeshProUGUI)
│   ├── UpgradeButton (Button)
│   │   └── UpgradeButtonText (TextMeshProUGUI)
│   ├── CloseButton (Button)
│   └── LoadingIndicator (GameObject)
└── ResultPanel (GameObject)
    ├── ResultText (TextMeshProUGUI)
    └── ResultCloseButton (Button)
```

### 4-3. CardUpgradeModalの設定

Inspectorで以下の参照を設定：

```
UI References:
- Modal Panel: ModalPanel
- Card Icon: CardIcon
- Card Name Text: CardNameText
- Current Level Text: CurrentLevelText
- Next Level Text: NextLevelText
- Current Value Text: CurrentValueText
- Next Value Text: NextValueText
- Success Rate Text: SuccessRateText
- Required Gold Text: RequiredGoldText
- Required Shards Text: RequiredShardsText
- Upgrade Button: UpgradeButton
- Close Button: CloseButton
- Upgrade Button Text: UpgradeButtonText

Loading:
- Loading Indicator: LoadingIndicator

Result:
- Result Panel: ResultPanel
- Result Text: ResultText
- Result Close Button: ResultCloseButton
```

### 4-4. CardUIの更新

1. 既存の `CardUI` プレハブを開く
2. 以下の要素を追加：

```
CardUI
├── UpgradeButtonContainer (GameObject)
│   └── UpgradeButton (Button)
│       └── UpgradeButtonText (TextMeshProUGUI)
```

3. **CardUI** スクリプトの参照を設定：

```
Upgrade Button:
- Upgrade Button: UpgradeButton
- Upgrade Button Container: UpgradeButtonContainer
```

## 5. PlayerDataManagerの設定

### 5-1. PlayerDataManagerの確認

1. **Hierarchy ウィンドウ**で `PlayerDataManager` を探す
2. 存在しない場合は、空のGameObjectを作成
3. **Add Component** で `PlayerDataManager` スクリプトを追加

### 5-2. PlayerDataSOの設定

1. **Project ウィンドウ**で `Assets/Resources/SO/PlayerDataSystem/` フォルダを開く
2. `PlayerDataSO.asset` を選択
3. Inspectorで以下の設定を確認：

```
Card Collection:
- Owned Cards: 空のリスト（初期状態）

Stats:
- Gold: 1000 (テスト用)
- Shards: 100 (テスト用)
```

## 6. テスト設定

### 6-1. テスト用エディターウィンドウの確認

1. **Unity メニュー**で **Tools > Card Upgrade System Test** を選択
2. テストウィンドウが開くことを確認

### 6-2. テストデータの準備

1. **Project ウィンドウ**で `Assets/SO/Card/` フォルダを開く
2. テスト用カードデータ（例：`Attack.asset`）が存在することを確認
3. カードデータにレア度と強化曲線が設定されていることを確認

## 7. シーン設定

### 7-1. メインシーンの設定

1. **File > Build Settings** を開く
2. メインシーンが **Scenes In Build** に含まれていることを確認
3. シーンを開いて、以下のコンポーネントが存在することを確認：
   - `UpgradeService`
   - `PlayerDataManager`
   - `CardUpgradeModal`（プレハブからインスタンス化）

### 7-2. プレハブの配置

1. **Project ウィンドウ**で `Assets/Prefabs/` フォルダを開く
2. `CardUpgradeModal` プレハブをシーンにドラッグ&ドロップ
3. プレハブが非アクティブ状態で配置されていることを確認

## 8. 動作確認

### 8-1. エディターテスト

1. **Tools > Card Upgrade System Test** を開く
2. **強化サービステスト** ボタンをクリック
3. コンソールにテスト結果が表示されることを確認

### 8-2. ランタイムテスト

1. **Play** ボタンを押してゲームを開始
2. カード詳細画面で強化ボタンが表示されることを確認
3. 強化ボタンをクリックしてモーダルが開くことを確認
4. プレビューと強化機能が正常に動作することを確認

## 9. トラブルシューティング

### 9-1. よくある問題

**問題**: UpgradeServiceが見つからない
- **解決**: シーンにUpgradeServiceコンポーネントが追加されているか確認

**問題**: PlayerDataManagerが見つからない
- **解決**: PlayerDataManagerの初期化を確認

**問題**: カードデータが見つからない
- **解決**: カードデータのパスと名前を確認

**問題**: 強化ボタンが表示されない
- **解決**: CardUpgradeFeature.IsEnabledがtrueになっているか確認

### 9-2. デバッグ方法

1. **Console ウィンドウ**でエラーメッセージを確認
2. **Tools > Card Upgrade System Test** でテスト実行
3. **Inspector** でコンポーネントの参照設定を確認

## 10. パフォーマンス最適化

### 10-1. メモリ使用量の最適化

1. 強化曲線データの配列サイズを適切に設定
2. 不要なカードインスタンスを定期的にクリーンアップ

### 10-2. 処理速度の最適化

1. カードデータのキャッシュ機能を実装
2. UI更新の頻度を適切に制御

## 11. セキュリティ考慮事項

### 11-1. データ整合性

1. カードインスタンスのレベルが最大レベルを超えないように制御
2. リソース消費時の整合性チェック

### 11-2. 不正防止

1. クライアントサイドでの検証
2. サーバーサイドでの検証（オンライン対応時）

## 12. 今後の拡張

### 12-1. 保護アイテム機能

1. `useProtector` パラメータの実装
2. 保護アイテムの消費ロジック

### 12-2. 強化失敗時のダウン/破損

1. 失敗時のレベルダウン機能
2. カード破損機能

### 12-3. 強化アニメーション

1. 強化成功時のエフェクト
2. レベルアップ演出

この設定手順に従って、カード強化システムを正常に動作させることができます。
