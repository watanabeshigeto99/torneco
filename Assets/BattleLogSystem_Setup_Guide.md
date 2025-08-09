# バトルログシステム セットアップガイド

## 概要

トルネコ風ローグライクカードバトルゲーム用の包括的なアクティブログシステムを実装しました。このシステムは、チェックリストのすべての要件を満たし、詳細な戦闘ログ、フィルタリング、検索、エクスポート機能を提供します。

## システム構成

### 1. コアコンポーネント

#### BattleLogManager.cs
- **責務**: バトルログの核心機能
- **機能**: 
  - 必須ログ（ターン見出し、行動選択、ダメージ結果等）
  - 詳細ログ（カード移動、ドロー結果、RNG情報等）
  - ステータスログ（状態異常、スタック変更等）
  - 進行ログ（階層遷移、イベント結果等）
  - 失敗ログ（行動不能、コスト不足等）
  - フィルタリング・検索機能
  - エクスポート・コピー機能

#### BattleLogUI.cs
- **責務**: ログ表示UI
- **機能**:
  - 折りたたみ可能なログパネル
  - タグ・重要度・発生源フィルター
  - 検索機能
  - エクスポート・コピーボタン
  - 設定パネル

#### BattleLogIntegrationManager.cs
- **責務**: 既存システムとの統合
- **機能**:
  - TurnManager、CardManager、Player、EnemyManagerとの連携
  - イベント購読・発火
  - ログ自動生成

## セットアップ手順

### ステップ1: バトルログマネージャーの追加

1. メインシーンにBattleLogManagerを追加
```csharp
// 自動生成される場合
GameObject battleLogManager = new GameObject("BattleLogManager");
battleLogManager.AddComponent<BattleLogManager>();
```

2. UI要素の設定
   - ScrollRect (logScrollRect)
   - TextMeshProUGUI (logText)
   - フィルタートグル配列
   - エクスポートボタン

### ステップ2: バトルログUIの追加

1. CanvasにBattleLogUIプレハブを追加
2. UI要素の設定:
   - メインパネル (logPanel)
   - フィルターパネル (filterPanel)
   - 各種トグルボタン
   - 検索フィールド
   - エクスポートボタン

### ステップ3: 統合マネージャーの追加

1. メインシーンにBattleLogIntegrationManagerを追加
2. 統合設定の確認:
   - enableTurnLogging
   - enableCardLogging
   - enablePlayerLogging
   - enableEnemyLogging
   - enableDamageLogging
   - enableStatusLogging

### ステップ4: 既存システムとの連携

#### TurnManagerとの連携
```csharp
// TurnManager.cs に追加
public static event Action<int> OnTurnStarted;
public static event Action<int> OnTurnEnded;

// ターン開始時
OnTurnStarted?.Invoke(currentTurn);

// ターン終了時
OnTurnEnded?.Invoke(currentTurn);
```

#### CardManagerとの連携
```csharp
// CardManager.cs に追加
public static event Action<string, string> OnCardUsed;
public static event Action<string, string, string> OnCardMoved;

// カード使用時
OnCardUsed?.Invoke(cardName, target);

// カード移動時
OnCardMoved?.Invoke(cardName, fromLocation, toLocation);
```

#### Playerとの連携
```csharp
// Player.cs に追加
public static event Action<string, int> OnPlayerMoved;
public static event Action<string, int, bool> OnPlayerAttack;
public static event Action<int, int, int> OnPlayerHPChanged;

// 移動時
OnPlayerMoved?.Invoke(direction, distance);

// 攻撃時
OnPlayerAttack?.Invoke(target, damage, isCritical);

// HP変更時
OnPlayerHPChanged?.Invoke(oldHP, newHP, maxHP);
```

#### EnemyManagerとの連携
```csharp
// EnemyManager.cs に追加
public static event Action<string, string, int, bool> OnEnemyAttack;
public static event Action<string, int, int, int> OnEnemyHPChanged;
public static event Action<string> OnEnemyDefeated;

// 敵攻撃時
OnEnemyAttack?.Invoke(enemyName, target, damage, isCritical);

// 敵HP変更時
OnEnemyHPChanged?.Invoke(enemyName, oldHP, newHP, maxHP);

// 敵撃破時
OnEnemyDefeated?.Invoke(enemyName);
```

## ログ機能詳細

### 必須ログ機能

#### ターン見出し
```csharp
BattleLogManager.Instance.LogTurnHeader(turn, handSize, deckSize, discardSize);
```

#### 行動選択
```csharp
BattleLogManager.Instance.LogActionSelection(cardName, target, LogSource.PLAYER);
```

#### ダメージ結果
```csharp
BattleLogManager.Instance.LogDamageResult(damage, isCritical, reduction, LogSource.PLAYER);
```

#### 被弾結果
```csharp
BattleLogManager.Instance.LogDamageReceived(attacker, damage, reduction, LogSource.ENEMY);
```

#### 状態変化
```csharp
BattleLogManager.Instance.LogStatusChange(statusName, isApplied, duration, LogSource.SYSTEM);
```

#### 撃破/被撃破
```csharp
BattleLogManager.Instance.LogDeath(target, isPlayer, LogSource.SYSTEM);
```

#### HP・シールド変動
```csharp
BattleLogManager.Instance.LogHPChange(target, oldHP, newHP, maxHP, LogSource.PLAYER);
```

#### ターン終了サマリ
```csharp
BattleLogManager.Instance.LogTurnSummary(cardsDrawn, statusEffectsProcessed, energyReset);
```

### 詳細ログ機能

#### カード移動
```csharp
BattleLogManager.Instance.LogCardMovement(cardName, fromLocation, toLocation, LogSource.CARD);
```

#### ドロー/サーチ結果
```csharp
BattleLogManager.Instance.LogDrawResult(drawnCards, isSearch, LogSource.CARD);
```

#### デッキ圧縮/肥大
```csharp
BattleLogManager.Instance.LogDeckChange(oldSize, newSize, reason, LogSource.SYSTEM);
```

#### タイル効果
```csharp
BattleLogManager.Instance.LogTileEffect(tileType, effect, LogSource.SYSTEM);
```

#### 命中/回避/ブロック計算
```csharp
BattleLogManager.Instance.LogCombatCalculation(calculation, result, LogSource.SYSTEM);
```

#### 敵AIの意図表示
```csharp
BattleLogManager.Instance.LogEnemyIntent(enemyName, intent);
```

#### RNG情報
```csharp
BattleLogManager.Instance.LogRNGInfo(context, value, LogSource.SYSTEM);
```

#### リソース変動
```csharp
BattleLogManager.Instance.LogResourceChange(resourceType, oldValue, newValue, LogSource.SYSTEM);
```

#### 効果解決順
```csharp
BattleLogManager.Instance.LogEffectResolution(effectName, priority, LogSource.SYSTEM);
```

### ステータス・持続効果ログ

#### 状態付与
```csharp
BattleLogManager.Instance.LogStatusApplied(target, statusName, duration, effectValue, LogSource.STATUS);
```

#### スタック変更
```csharp
BattleLogManager.Instance.LogStackChange(statusName, oldStacks, newStacks, LogSource.STATUS);
```

#### 効果失効
```csharp
BattleLogManager.Instance.LogStatusExpired(target, statusName, LogSource.STATUS);
```

#### ターンまたぎ処理
```csharp
BattleLogManager.Instance.LogTurnTransition(phase, processedEffects, LogSource.STATUS);
```

### 進行・メタログ

#### フロア/部屋遷移
```csharp
BattleLogManager.Instance.LogFloorTransition(oldFloor, newFloor, roomType);
```

#### イベント結果
```csharp
BattleLogManager.Instance.LogEventResult(eventName, result);
```

#### 実績/クエスト達成
```csharp
BattleLogManager.Instance.LogAchievement(achievementName, description);
```

#### セーブ/ロード
```csharp
BattleLogManager.Instance.LogSaveLoad(operation, slot);
```

### 失敗・例外ログ

#### 行動不能
```csharp
BattleLogManager.Instance.LogActionBlocked(reason, LogSource.SYSTEM);
```

#### コスト不足
```csharp
BattleLogManager.Instance.LogInsufficientCost(costType, required, current, LogSource.SYSTEM);
```

#### 対象不適
```csharp
BattleLogManager.Instance.LogInvalidTarget(reason, LogSource.SYSTEM);
```

#### クールダウン中
```csharp
BattleLogManager.Instance.LogCooldownActive(abilityName, remainingTurns, LogSource.SYSTEM);
```

## UI設定

### ログ表示設定
- **maxLogLines**: 表示する最大行数（デフォルト: 100）
- **enableDetailedLogs**: 詳細ログの有効/無効
- **enableStatusLogs**: ステータスログの有効/無効
- **enableMetaLogs**: メタログの有効/無効
- **enableFailureLogs**: 失敗ログの有効/無効

### 視覚設定
- **playerColor**: プレイヤー関連ログの色
- **enemyColor**: 敵関連ログの色
- **systemColor**: システム関連ログの色
- **criticalColor**: 重要ログの色
- **importantColor**: 重要ログの色
- **infoColor**: 情報ログの色

### フィルター設定
- **タグフィルター**: ATK, DEF, BUFF, DEBUFF, DRAW, MOVE, AI, RNG, LOOT, SYSTEM, TURN, DAMAGE, HEAL, STATUS, DEATH, FLOOR, EVENT, SAVE, ERROR
- **重要度フィルター**: INFO, IMPORTANT, CRITICAL
- **発生源フィルター**: PLAYER, ENEMY, SYSTEM, CARD, STATUS, FLOOR, EVENT

## 使用方法

### 基本的なログ追加
```csharp
// カスタムログエントリを追加
BattleLogManager.Instance.AddLogEntry(
    turn,
    "カスタムメッセージ",
    BattleLogManager.LogSeverity.INFO,
    BattleLogManager.LogTags.SYSTEM,
    BattleLogManager.LogSource.SYSTEM
);
```

### 統合マネージャーを使用
```csharp
// プレイヤー移動ログ
BattleLogIntegrationManager.Instance.LogPlayerMoved("北", 2);

// プレイヤー攻撃ログ
BattleLogIntegrationManager.Instance.LogPlayerAttack("敵A", 15, false);

// 敵攻撃ログ
BattleLogIntegrationManager.Instance.LogEnemyAttack("敵A", "プレイヤー", 8, false);
```

### フィルター操作
```csharp
// フィルター設定を更新
BattleLogManager.Instance.UpdateFilters(
    BattleLogManager.LogTags.ATK | BattleLogManager.LogTags.DAMAGE,
    BattleLogManager.LogSeverity.IMPORTANT,
    new BattleLogManager.LogSource[] { BattleLogManager.LogSource.PLAYER }
);
```

### ログエクスポート
```csharp
// ログをファイルにエクスポート
// BattleLogManager内で自動的に実行される

// ログをクリップボードにコピー
// BattleLogManager内で自動的に実行される
```

## トラブルシューティング

### よくある問題

1. **ログが表示されない**
   - BattleLogManagerが正しく初期化されているか確認
   - UI要素が正しく設定されているか確認
   - フィルター設定を確認

2. **イベントが発火しない**
   - 統合マネージャーが正しく設定されているか確認
   - イベント購読が正しく行われているか確認

3. **パフォーマンスの問題**
   - maxLogLinesを適切に設定
   - 不要なログを無効化
   - フィルターを活用

### デバッグ情報

```csharp
// 統合状態を確認
Debug.Log(BattleLogIntegrationManager.Instance.GetIntegrationStatus());

// ログエントリ数を確認
Debug.Log($"ログエントリ数: {BattleLogManager.Instance.GetLogEntries().Count}");

// フィルタされたエントリ数を確認
Debug.Log($"フィルタされたエントリ数: {BattleLogManager.Instance.GetFilteredLogEntries().Count}");
```

## 拡張方法

### 新しいログタイプの追加

1. LogTags enumに新しいタグを追加
2. 対応するログメソッドをBattleLogManagerに追加
3. UIにフィルタートグルを追加
4. 統合マネージャーにメソッドを追加

### 新しい発生源の追加

1. LogSource enumに新しい発生源を追加
2. GetSourceDisplayNameメソッドに表示名を追加
3. GetSourceColorメソッドに色を追加
4. UIにフィルタートグルを追加

### カスタムフォーマットの追加

1. FormatLogEntryメソッドを拡張
2. 新しい色やスタイルを追加
3. カスタムタグの表示を追加

## まとめ

このバトルログシステムは、チェックリストのすべての要件を満たし、トルネコ風ローグライクカードバトルゲームに最適化された包括的なログ機能を提供します。適切にセットアップすることで、プレイヤーは戦闘の詳細を追跡し、デバッグや戦略分析に活用できます。
