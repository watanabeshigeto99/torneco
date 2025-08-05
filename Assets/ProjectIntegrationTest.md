# プロジェクト統合テストガイド

## 概要

このガイドは、作成されたすべてのシステムテンプレートが正常に連携することを確認するための統合テスト手順を説明します。

## テスト対象システム

### 1. Battle System
- **BattleManager.cs** - 戦闘システム管理
- **BattleDataSO.cs** - 戦闘データ管理
- **BattleEventChannel.cs** - 戦闘イベント管理
- **BattleSystemTest.cs** - 戦闘システムテスト

### 2. Save System
- **SaveManager.cs** - セーブシステム管理
- **SaveDataSO.cs** - セーブデータ管理
- **SaveEventChannel.cs** - セーブイベント管理
- **SaveSystemTest.cs** - セーブシステムテスト

### 3. Player Data System
- **PlayerDataManager.cs** - プレイヤーデータ管理
- **PlayerDataSO.cs** - プレイヤーデータ管理
- **PlayerEventChannel.cs** - プレイヤーイベント管理
- **PlayerDataSystemTest.cs** - プレイヤーデータシステムテスト

### 4. Floor System
- **FloorManager.cs** - 階層システム管理
- **FloorDataSO.cs** - 階層データ管理
- **FloorEventChannel.cs** - 階層イベント管理
- **FloorSystemTest.cs** - 階層システムテスト

### 5. Deck System
- **DeckManager.cs** - デッキシステム管理
- **DeckDataSO.cs** - デッキデータ管理
- **DeckEventChannel.cs** - デッキイベント管理
- **DeckSystemTest.cs** - デッキシステムテスト

### 6. UI System
- **UIManager.cs** - UIシステム管理
- **UIDataSO.cs** - UIデータ管理
- **UIEventChannel.cs** - UIイベント管理
- **UISystemTest.cs** - UIシステムテスト

## 統合テスト手順

### Phase 1: 個別システムテスト

#### 1.1 Battle System テスト
```
1. BattleSystemTestScene.unity を作成
2. BattleManager, BattleDataSO, BattleEventChannel を配置
3. BattleSystemTest コンポーネントを配置
4. 戦闘開始、終了、ダメージ処理をテスト
5. イベントチャンネルの動作確認
```

#### 1.2 Save System テスト
```
1. SaveSystemTestScene.unity を作成
2. SaveManager, SaveDataSO, SaveEventChannel を配置
3. SaveSystemTest コンポーネントを配置
4. セーブ、ロード機能をテスト
5. データ永続化の確認
```

#### 1.3 Player Data System テスト
```
1. PlayerDataSystemTestScene.unity を作成
2. PlayerDataManager, PlayerDataSO, PlayerEventChannel を配置
3. PlayerDataSystemTest コンポーネントを配置
4. レベルアップ、経験値獲得をテスト
5. プレイヤー状態変更の確認
```

#### 1.4 Floor System テスト
```
1. FloorSystemTestScene.unity を作成
2. FloorManager, FloorDataSO, FloorEventChannel を配置
3. FloorSystemTest コンポーネントを配置
4. 階層移動、クリア判定をテスト
5. 階層データの更新確認
```

#### 1.5 Deck System テスト
```
1. DeckSystemTestScene.unity を作成
2. DeckManager, DeckDataSO, DeckEventChannel を配置
3. DeckSystemTest コンポーネントを配置
4. カード追加、削除、ドローをテスト
5. デッキ状態の管理確認
```

#### 1.6 UI System テスト
```
1. UISystemTestScene.unity を作成
2. UIManager, UIDataSO, UIEventChannel を配置
3. UISystemTest コンポーネントを配置
4. HUD更新、ログ表示、画面遷移をテスト
5. UI要素の表示制御確認
```

### Phase 2: システム間連携テスト

#### 2.1 Battle System ↔ Player Data System
```
1. 戦闘でダメージを与える
2. プレイヤーのHPが減少することを確認
3. レベルアップ時の戦闘力上昇を確認
4. イベントチャンネルでの連携確認
```

#### 2.2 Player Data System ↔ Save System
```
1. プレイヤーデータを変更
2. セーブ機能でデータを保存
3. ロード機能でデータを復元
4. データの整合性確認
```

#### 2.3 Floor System ↔ UI System
```
1. 階層を変更
2. UIの階層表示が更新されることを確認
3. 階層クリア時の画面遷移確認
4. イベント連携の動作確認
```

#### 2.4 Deck System ↔ UI System
```
1. カードを追加/削除
2. UIのカード表示が更新されることを確認
3. カードクリック時のイベント確認
4. デッキ状態のUI反映確認
```

#### 2.5 Battle System ↔ UI System
```
1. 戦闘状態を変更
2. UIの戦闘表示が更新されることを確認
3. 戦闘ログの表示確認
4. 戦闘結果のUI反映確認
```

### Phase 3: GameManager統合テスト

#### 3.1 GameManager統合確認
```
1. GameManagerに全システムの参照を追加
2. 各システムのイベント購読を設定
3. システム間の連携動作確認
4. 後方互換性の確認
```

#### 3.2 統合シナリオテスト
```
シナリオ1: ゲーム開始 → 戦闘 → レベルアップ → セーブ
シナリオ2: ロード → デッキ変更 → 階層移動 → 戦闘
シナリオ3: カード獲得 → デッキ更新 → UI更新 → セーブ
```

### Phase 4: パフォーマンステスト

#### 4.1 メモリ使用量テスト
```
1. 長時間プレイ時のメモリ使用量監視
2. システム間のメモリリーク確認
3. イベント購読の適切な解除確認
4. ScriptableObjectの永続化確認
```

#### 4.2 実行速度テスト
```
1. イベント処理の速度測定
2. UI更新の応答性確認
3. データ保存/読み込みの速度確認
4. システム間通信の遅延測定
```

## テスト結果の評価基準

### 機能テスト
- ✅ 各システムが独立して動作する
- ✅ システム間の連携が正常に動作する
- ✅ イベントチャンネルが適切に機能する
- ✅ データの永続化が正常に動作する

### パフォーマンステスト
- ✅ メモリ使用量が適切な範囲内
- ✅ 実行速度が許容範囲内
- ✅ UI応答性が良好
- ✅ データ処理が効率的

### 安定性テスト
- ✅ 長時間プレイでクラッシュしない
- ✅ エラー処理が適切に機能する
- ✅ 後方互換性が維持される
- ✅ 段階的移行が安全に実行される

## 問題が発生した場合の対処法

### 1. コンパイルエラー
- 名前空間の確認
- 依存関係の確認
- メソッド名の確認

### 2. 実行時エラー
- イベント購読の確認
- ScriptableObjectの設定確認
- コンポーネントの配置確認

### 3. パフォーマンス問題
- イベント購読の最適化
- メモリリークの調査
- 不要な処理の削除

## 次のステップ

### 1. テスト完了後
- テスト結果のドキュメント化
- 発見された問題の修正
- パフォーマンスの最適化

### 2. 段階的移行の開始
- 既存コードの条件分岐追加
- 新しいシステムの段階的導入
- 後方互換性の維持

### 3. 最終的な安定化
- 旧コードの安全な削除
- 最終的なテスト実行
- ドキュメントの更新

---

**注意**: このテストは慎重に行い、各段階で十分な確認を行ってください。問題が発生した場合は、即座に修正を行い、再テストを実施してください。 