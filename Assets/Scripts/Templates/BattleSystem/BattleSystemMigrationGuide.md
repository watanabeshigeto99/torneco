# 戦闘システム移行ガイド

## 🔄 移行の目的
既存の巨大なGameManagerから戦闘関連の機能を分離し、単一責務のコンポーネントに再構成します。

## 📋 移行前後の比較

### 移行前（問題のある構造）
```
GameManager.cs (363行)
├── ゲーム状態管理
├── プレイヤーデータ管理
├── 階層システム
├── デッキシステム
├── 戦闘管理 ← ここを分離
└── イベント管理
```

### 移行後（単一責務）
```
BattleStarter.cs (戦闘開始専用)
BattleStateSO.cs (戦闘状態管理専用)
BattleUnit.cs (ユニット状態管理専用)
BattleEventChannel.cs (戦闘イベント専用)
BattleConfigSO.cs (戦闘設定専用)
```

## 🚀 移行ステップ

### STEP 1: 既存コードの分析
```csharp
// GameManager.cs から戦闘関連の機能を特定
public void EnemyDefeated() // ← 戦闘関連
public void GameOver() // ← 戦闘関連
public void GameClear() // ← 戦闘関連
```

### STEP 2: 新システムの導入
1. **BattleStarter**をシーンに追加
2. **BattleStateSO**アセットを作成
3. **BattleConfigSO**アセットを作成
4. **BattleEventChannel**アセットを作成

### STEP 3: 機能の移行
```csharp
// 移行前（GameManager.cs）
public void EnemyDefeated()
{
    score += 100;
    AddPlayerExp(10);
    // ...
}

// 移行後（BattleStarter.cs）
public void EndBattle(BattleResult result)
{
    battleState.SetBattleResult(result);
    OnBattleEnded?.Invoke(battleState);
    // ...
}
```

### STEP 4: イベントの置き換え
```csharp
// 移行前（直接参照）
if (GameManager.Instance != null)
{
    GameManager.Instance.EnemyDefeated();
}

// 移行後（イベント経由）
if (battleEventChannel != null)
{
    battleEventChannel.RaiseUnitDied(enemyUnit);
}
```

## 🔧 具体的な移行手順

### 1. 新システムのセットアップ
```csharp
// 1. ScriptableObjectアセットを作成
// Assets/SO/BattleSystem/BattleState.asset
// Assets/SO/BattleSystem/BattleConfig.asset
// Assets/SO/BattleSystem/BattleEventChannel.asset

// 2. シーンにBattleStarterを追加
GameObject battleSystem = new GameObject("BattleSystem");
BattleStarter starter = battleSystem.AddComponent<BattleStarter>();
starter.battleState = battleStateAsset;
starter.battleConfig = battleConfigAsset;
starter.battleEventChannel = eventChannelAsset;
```

### 2. 既存コードの無効化
```csharp
// GameManager.cs の戦闘関連メソッドを一時的に無効化
/*
public void EnemyDefeated()
{
    // 一時的にコメントアウト
    // score += 100;
    // AddPlayerExp(10);
}
*/
```

### 3. 新システムとの連携
```csharp
// Player.cs で新システムを使用
private void OnEnemyDefeated()
{
    if (BattleStarter.Instance != null)
    {
        // 新システムで戦闘終了処理
        BattleStarter.Instance.EndBattle(BattleResult.PlayerVictory);
    }
}
```

### 4. UIの更新
```csharp
// UIManager.cs で新システムのイベントを購読
private void SubscribeToBattleEvents()
{
    if (battleEventChannel != null)
    {
        battleEventChannel.OnBattleStarted.AddListener(OnBattleStarted);
        battleEventChannel.OnBattleEnded.AddListener(OnBattleEnded);
        battleEventChannel.OnUnitDamaged.AddListener(OnUnitDamaged);
    }
}
```

## ✅ 移行完了チェックリスト

- [ ] BattleStarterが正常に動作する
- [ ] BattleStateSOで戦闘状態が管理される
- [ ] BattleEventChannelでイベントが正しく発火する
- [ ] 既存の戦闘機能が新システムで動作する
- [ ] UIが新システムのイベントに反応する
- [ ] テストで動作確認が完了する

## 🧪 テスト方法

### 単体テスト
```csharp
// BattleSystemTest.cs を使用
// 1. 戦闘開始ボタンをクリック
// 2. ダメージボタンでユニットをダメージ
// 3. 回復ボタンでユニットを回復
// 4. 戦闘終了ボタンで戦闘を終了
```

### 統合テスト
```csharp
// 既存のPlayer.cs と連携
// 1. プレイヤーが敵に攻撃
// 2. 敵が死亡
// 3. 新システムで戦闘終了
// 4. UIが正しく更新される
```

## ⚠️ 注意事項

1. **段階的移行**: 一度に全てを移行せず、機能ごとに段階的に移行
2. **バックアップ**: 移行前に既存コードのバックアップを作成
3. **テスト**: 各段階で十分なテストを実施
4. **ロールバック**: 問題が発生した場合のロールバック手順を準備

## 📈 移行後の効果

- **保守性向上**: 各コンポーネントが単一責務を持つ
- **再利用性**: 戦闘システムを他のプロジェクトで再利用可能
- **テスタビリティ**: 単体テストが容易になる
- **拡張性**: 新機能の追加が容易になる 