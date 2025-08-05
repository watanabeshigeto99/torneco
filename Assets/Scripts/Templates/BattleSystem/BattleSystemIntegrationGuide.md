# 戦闘システム統合ガイド

## 📋 概要

既存の`GameManager.cs`から戦闘関連機能を新しい戦闘システムテンプレートに段階的に移行するためのガイドです。

## 🎯 移行対象機能

### 1. 戦闘結果処理
- `EnemyDefeated()` - 敵撃破時の処理
- `GameOver()` - ゲームオーバー処理  
- `GameClear()` - ゲームクリア処理

### 2. プレイヤーデータ管理
- プレイヤーHP管理
- レベル・経験値管理
- スコア管理

## 🔄 段階的移行手順

### Phase 1: 戦闘システムテンプレートの動作確認

#### 1.1 テストシーンの作成
```csharp
// 新しいテストシーンを作成
// Assets/Scenes/BattleSystemTestScene.unity
```

#### 1.2 テスト用GameObjectの設定
```
BattleSystemTestScene
├── BattleStarter (BattleStarter.cs)
├── BattleState (BattleStateSO)
├── BattleConfig (BattleConfigSO)
├── BattleEventChannel (BattleEventChannel.cs)
├── TestPlayer (BattleUnit.cs)
├── TestEnemy (BattleUnit.cs)
└── BattleSystemTest (BattleSystemTest.cs)
```

#### 1.3 動作確認項目
- [ ] バトル開始・終了
- [ ] ターン管理
- [ ] ユニットのダメージ・回復
- [ ] イベントチャンネルの動作
- [ ] バトル状態の管理

### Phase 2: 既存コードとの統合準備

#### 2.1 GameManagerの戦闘機能を無効化
```csharp
// GameManager.cs の該当メソッドを一時的にコメントアウト
/*
public void EnemyDefeated()
{
    // 一時的に無効化
}

public void GameOver()
{
    // 一時的に無効化
}

public void GameClear()
{
    // 一時的に無効化
}
*/
```

#### 2.2 新しい戦闘システムの統合
```csharp
// GameManager.cs に新しい戦闘システムへの参照を追加
[Header("Battle System Integration")]
public BattleStarter battleStarter;
public BattleStateSO battleState;
public BattleEventChannel battleEventChannel;
```

### Phase 3: 機能移行

#### 3.1 EnemyDefeated() の移行
```csharp
// 既存のGameManager.cs
public void EnemyDefeated()
{
    // この機能をBattleStarterに移行
    if (battleStarter != null)
    {
        battleStarter.HandleEnemyDefeated();
    }
}
```

#### 3.2 GameOver() の移行
```csharp
// 既存のGameManager.cs
public void GameOver()
{
    // この機能をBattleStarterに移行
    if (battleStarter != null)
    {
        battleStarter.EndBattle(BattleResult.EnemyVictory);
    }
}
```

#### 3.3 GameClear() の移行
```csharp
// 既存のGameManager.cs
public void GameClear()
{
    // この機能をBattleStarterに移行
    if (battleStarter != null)
    {
        battleStarter.EndBattle(BattleResult.PlayerVictory);
    }
}
```

### Phase 4: イベント連携

#### 4.1 戦闘システムイベントの購読
```csharp
// GameManager.cs に追加
private void SubscribeToBattleEvents()
{
    if (battleEventChannel != null)
    {
        battleEventChannel.OnBattleEnded.AddListener(OnBattleEnded);
        battleEventChannel.OnUnitDefeated.AddListener(OnUnitDefeated);
    }
}

private void OnBattleEnded(BattleStateSO battleState)
{
    switch (battleState.battleResult)
    {
        case BattleResult.PlayerVictory:
            HandlePlayerVictory();
            break;
        case BattleResult.EnemyVictory:
            HandlePlayerDefeat();
            break;
    }
}
```

#### 4.2 プレイヤーデータの同期
```csharp
// GameManager.cs に追加
private void SyncPlayerDataWithBattleSystem()
{
    if (battleState != null && battleState.playerUnit != null)
    {
        // プレイヤーデータを戦闘システムと同期
        playerCurrentHP = battleState.playerUnit.currentHP;
        playerMaxHP = battleState.playerUnit.maxHP;
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
        }
    }
}
```

## 🧪 テスト戦略

### 1. 単体テスト
- 各戦闘システムコンポーネントの独立テスト
- イベントチャンネルの動作確認
- データ構造の整合性確認

### 2. 統合テスト
- GameManagerと戦闘システムの連携テスト
- 既存機能の動作確認
- パフォーマンステスト

### 3. 回帰テスト
- 既存のゲームプレイ機能の動作確認
- UI表示の正確性確認
- セーブ・ロード機能の確認

## ⚠️ 注意事項

### 1. 段階的移行
- 一度に全ての機能を移行せず、段階的に行う
- 各段階でテストを実施
- 問題が発生した場合は前の段階に戻れるようにする

### 2. データ整合性
- プレイヤーデータの二重管理を避ける
- セーブデータの互換性を保つ
- 既存のセーブファイルとの互換性を確認

### 3. パフォーマンス
- 新しいシステムのパフォーマンス影響を監視
- メモリ使用量の確認
- フレームレートへの影響を測定

## 📊 進捗管理

### 完了チェックリスト
- [ ] Phase 1: 戦闘システムテンプレートの動作確認
- [ ] Phase 2: 既存コードとの統合準備
- [ ] Phase 3: 機能移行
- [ ] Phase 4: イベント連携
- [ ] 単体テスト完了
- [ ] 統合テスト完了
- [ ] 回帰テスト完了

### 次のステップ
1. 戦闘システムテンプレートの動作確認
2. テストシーンの作成
3. 既存GameManagerとの統合開始
4. 段階的な機能移行
5. セーブシステムとの連携確認

## 🔗 関連ファイル

- `Scripts/Templates/BattleSystem/BattleStarter.cs`
- `Scripts/Templates/BattleSystem/BattleStateSO.cs`
- `Scripts/Templates/BattleSystem/BattleEventChannel.cs`
- `Scripts/Templates/BattleSystem/BattleSystemTest.cs`
- `Scripts/Manager/GameManager.cs`
- `Scripts/Templates/SaveSystem/SaveLoader.cs` 