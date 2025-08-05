# プレイヤーデータシステム移行ガイド

## 📋 概要

既存の`GameManager.cs`からプレイヤーデータ管理機能を新しいプレイヤーデータシステムテンプレートに段階的に移行するためのガイドです。

## 🎯 移行対象機能

### 1. プレイヤーデータ管理
- `playerLevel` - プレイヤーレベル
- `playerExp` - プレイヤー経験値
- `playerExpToNext` - 次のレベルアップに必要な経験値
- `playerMaxHP` - プレイヤー最大HP
- `playerCurrentHP` - プレイヤー現在HP
- `playerMaxLevel` - プレイヤー最大レベル

### 2. プレイヤー関連メソッド
- `AddPlayerExp()` - 経験値追加
- `PlayerLevelUp()` - レベルアップ処理
- `SetPlayerHP()` - HP設定
- `SyncPlayerDataFromPlayer()` - プレイヤーデータ同期
- `ApplyPlayerDataToPlayer()` - プレイヤーデータ適用

## 🔄 段階的移行手順

### Phase 1: プレイヤーデータシステムテンプレートの動作確認

#### 1.1 テストシーンの作成
```csharp
// 新しいテストシーンを作成
// Assets/Scenes/PlayerDataSystemTestScene.unity
```

#### 1.2 テスト用GameObjectの設定
```
PlayerDataSystemTestScene
├── PlayerDataManager (PlayerDataManager.cs)
├── PlayerData (PlayerDataSO)
├── PlayerEventChannel (PlayerEventChannel.cs)
└── PlayerDataTest (PlayerDataTest.cs)
```

#### 1.3 動作確認項目
- [ ] プレイヤーデータの初期化
- [ ] 経験値獲得・レベルアップ
- [ ] HP変更（ダメージ・回復）
- [ ] イベントチャンネルの動作
- [ ] プレイヤーデータの状態管理

### Phase 2: 既存コードとの統合準備

#### 2.1 GameManagerのプレイヤーデータ機能を無効化
```csharp
// GameManager.cs の該当メソッドを一時的にコメントアウト
/*
public void AddPlayerExp(int amount)
{
    // 一時的に無効化
}

private void PlayerLevelUp()
{
    // 一時的に無効化
}

public void SetPlayerHP(int currentHP, int maxHP = -1)
{
    // 一時的に無効化
}
*/
```

#### 2.2 新しいプレイヤーデータシステムの統合
```csharp
// GameManager.cs に新しいプレイヤーデータシステムへの参照を追加
[Header("Player Data System Integration")]
public PlayerDataSystem.PlayerDataManager playerDataManager;
public PlayerDataSystem.PlayerDataSO playerData;
public PlayerDataSystem.PlayerEventChannel playerEventChannel;
```

### Phase 3: 機能移行

#### 3.1 AddPlayerExp() の移行
```csharp
// 既存のGameManager.cs
public void AddPlayerExp(int amount)
{
    // プレイヤーデータシステムが利用可能な場合はそちらを使用
    if (playerDataManager != null)
    {
        playerDataManager.AddPlayerExp(amount);
    }
    else
    {
        // 従来の処理（後方互換性のため）
        // 既存のコード
    }
}
```

#### 3.2 SetPlayerHP() の移行
```csharp
// 既存のGameManager.cs
public void SetPlayerHP(int currentHP, int maxHP = -1)
{
    // プレイヤーデータシステムが利用可能な場合はそちらを使用
    if (playerDataManager != null)
    {
        playerDataManager.SetPlayerHP(currentHP, maxHP);
    }
    else
    {
        // 従来の処理（後方互換性のため）
        // 既存のコード
    }
}
```

#### 3.3 プレイヤーデータ同期の移行
```csharp
// 既存のGameManager.cs
public void SyncPlayerDataFromPlayer()
{
    // プレイヤーデータシステムが利用可能な場合はそちらを使用
    if (playerDataManager != null)
    {
        playerDataManager.SyncWithGameManager();
    }
    else
    {
        // 従来の処理（後方互換性のため）
        // 既存のコード
    }
}

public void ApplyPlayerDataToPlayer()
{
    // プレイヤーデータシステムが利用可能な場合はそちらを使用
    if (playerDataManager != null)
    {
        playerDataManager.ApplyToGameManager();
    }
    else
    {
        // 従来の処理（後方互換性のため）
        // 既存のコード
    }
}
```

### Phase 4: イベント連携

#### 4.1 プレイヤーデータシステムイベントの購読
```csharp
// GameManager.cs に追加
private void SubscribeToPlayerDataEvents()
{
    if (playerEventChannel != null)
    {
        playerEventChannel.OnPlayerLevelUp.AddListener(OnPlayerLevelUp);
        playerEventChannel.OnPlayerExpGained.AddListener(OnPlayerExpGained);
        playerEventChannel.OnPlayerHPChanged.AddListener(OnPlayerHPChanged);
    }
}

private void OnPlayerLevelUp(int newLevel)
{
    // 既存のレベルアップ処理を呼び出し
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateLevelDisplay(newLevel, playerData?.experience ?? 0, playerData?.experienceToNext ?? 10);
        UIManager.Instance.AddLog($"レベルアップ！レベル {newLevel}");
    }
}

private void OnPlayerExpGained(int expAmount)
{
    // 既存の経験値獲得処理を呼び出し
    if (UIManager.Instance != null)
    {
        UIManager.Instance.AddLog($"経験値獲得！+{expAmount}");
    }
}

private void OnPlayerHPChanged(int currentHP, int maxHP)
{
    // 既存のHP変更処理を呼び出し
    if (UIManager.Instance != null)
    {
        UIManager.Instance.UpdateHP(currentHP, maxHP);
    }
}
```

#### 4.2 プレイヤーデータの同期
```csharp
// GameManager.cs に追加
private void SyncPlayerDataWithPlayerDataSystem()
{
    if (playerData != null)
    {
        // プレイヤーデータをプレイヤーデータシステムと同期
        playerLevel = playerData.level;
        playerExp = playerData.experience;
        playerExpToNext = playerData.experienceToNext;
        playerCurrentHP = playerData.currentHP;
        playerMaxHP = playerData.maxHP;
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(playerCurrentHP, playerMaxHP);
            UIManager.Instance.UpdateLevelDisplay(playerLevel, playerExp, playerExpToNext);
        }
    }
}
```

## 🧪 テスト戦略

### 1. 単体テスト
- 各プレイヤーデータシステムコンポーネントの独立テスト
- イベントチャンネルの動作確認
- データ構造の整合性確認

### 2. 統合テスト
- GameManagerとプレイヤーデータシステムの連携テスト
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
- [ ] Phase 1: プレイヤーデータシステムテンプレートの動作確認
- [ ] Phase 2: 既存コードとの統合準備
- [ ] Phase 3: 機能移行
- [ ] Phase 4: イベント連携
- [ ] 単体テスト完了
- [ ] 統合テスト完了
- [ ] 回帰テスト完了

### 次のステップ
1. プレイヤーデータシステムテンプレートの動作確認
2. テストシーンの作成
3. 既存GameManagerとの統合開始
4. 段階的な機能移行
5. セーブシステムとの連携確認

## 🔗 関連ファイル

- `Scripts/Templates/PlayerDataSystem/PlayerDataManager.cs`
- `Scripts/Templates/PlayerDataSystem/PlayerDataSO.cs`
- `Scripts/Templates/PlayerDataSystem/PlayerEventChannel.cs`
- `Scripts/Templates/PlayerDataSystem/PlayerDataTest.cs`
- `Scripts/Manager/GameManager.cs`

このガイドに従って段階的に移行することで、プレイヤーデータ管理の安定性と保守性を大幅に向上させることができます。 