# Floor System 移行ガイド

## 概要

このガイドは、`GameManager.cs`から階層システム機能を分離し、新しいFloor Systemテンプレートに移行するための手順を説明します。

## 移行対象

### GameManager.cs から移行する機能

```csharp
// 階層システム関連フィールド
public int currentFloor = 1;
public int maxFloor = 10;

// 階層システムイベント
public static event System.Action<int> OnFloorChanged;
public static event System.Action OnGameClear;

// 階層システムメソッド
public void GoToNextFloor()
private System.Collections.IEnumerator GoToDeckBuilderCoroutine()
private System.Collections.IEnumerator GenerateFloorCoroutine(int floorNumber)
```

## 新しいFloor Systemテンプレート

### 1. FloorManager.cs
- **責務**: 階層システムの管理のみ
- **機能**: 階層の進行、完了、生成、シーン遷移
- **特徴**: 単一責任原則、イベント駆動

### 2. FloorDataSO.cs
- **責務**: 階層データの状態管理のみ
- **機能**: 階層情報、進捗、統計、難易度
- **特徴**: ScriptableObject、データ駆動

### 3. FloorEventChannel.cs
- **責務**: 階層関連イベントの管理のみ
- **機能**: UnityEvent、疎結合
- **特徴**: イベント駆動、拡張性

### 4. FloorSystemTest.cs
- **責務**: 階層システムのテストのみ
- **機能**: UI操作、イベント監視、状態表示
- **特徴**: 独立テスト、デバッグ支援

## 段階的移行手順

### Phase 1: テスト環境の準備

#### 1.1 テストシーンの作成
```csharp
// 新しいテストシーンを作成
// Assets/Scenes/FloorSystemTestScene.unity
```

#### 1.2 コンポーネントの配置
```csharp
// FloorSystemTestコンポーネントをGameObjectに追加
// 必要なUI要素を配置
// ScriptableObjectアセットを作成
```

#### 1.3 動作確認
- [ ] FloorManagerの初期化
- [ ] FloorDataSOのデータ管理
- [ ] FloorEventChannelのイベント発火
- [ ] UI操作の応答

### Phase 2: GameManager統合準備

#### 2.1 参照の追加
```csharp
// GameManager.cs に追加
[Header("Floor System Integration")]
public FloorSystem.FloorManager floorManager;
public FloorSystem.FloorDataSO floorData;
public FloorSystem.FloorEventChannel floorEventChannel;
```

#### 2.2 イベント購読の追加
```csharp
// GameManager.cs のAwake()に追加
SubscribeToFloorEvents();
```

#### 2.3 統合用メソッドの追加
```csharp
private void SubscribeToFloorEvents()
{
    if (floorEventChannel != null)
    {
        floorEventChannel.OnFloorChanged.AddListener(OnFloorChanged);
        floorEventChannel.OnGameClear.AddListener(OnGameClear);
    }
}
```

### Phase 3: 機能移行

#### 3.1 GoToNextFloor() の移行
```csharp
// 既存のメソッドを修正
public void GoToNextFloor()
{
    // 階層システムが利用可能な場合はそちらを使用
    if (floorManager != null)
    {
        floorManager.GoToNextFloor();
    }
    else
    {
        // 従来の処理（後方互換性のため）
        if (gameOver || gameClear) return;
        currentFloor++;
        OnFloorChanged?.Invoke(currentFloor);
        if (currentFloor > maxFloor)
        {
            GameClear();
            return;
        }
        StartCoroutine(GoToDeckBuilderCoroutine());
    }
}
```

#### 3.2 階層データの同期
```csharp
// GameManager.cs に追加
private void SyncFloorDataWithFloorSystem()
{
    if (floorData != null)
    {
        currentFloor = floorData.currentFloor;
        maxFloor = floorData.maxFloor;
    }
}

private void ApplyFloorDataToFloorSystem()
{
    if (floorData != null)
    {
        floorData.SetFloor(currentFloor);
    }
}
```

#### 3.3 イベントハンドラーの追加
```csharp
private void OnFloorChanged(int newFloor)
{
    // 既存の階層変更処理を呼び出し
    if (UIManager.Instance != null)
    {
        UIManager.Instance.AddLog($"階層 {newFloor} に進みました");
    }
}

private void OnGameClear()
{
    // 既存のゲームクリア処理を呼び出し
    GameClear();
}
```

### Phase 4: テストと検証

#### 4.1 単体テスト
- [ ] FloorManagerの各メソッド
- [ ] FloorDataSOのデータ操作
- [ ] FloorEventChannelのイベント発火

#### 4.2 統合テスト
- [ ] GameManagerとFloor Systemの連携
- [ ] 既存機能の動作確認
- [ ] パフォーマンステスト

#### 4.3 回帰テスト
- [ ] 階層進行の動作
- [ ] ゲームクリアの動作
- [ ] シーン遷移の動作

## 重要な考慮事項

### 1. 段階的移行
- 既存のコードは残し、新しいシステムと並行運用
- 後方互換性を維持
- 段階的に機能を移行

### 2. データ整合性
- GameManagerとFloorDataSOの同期
- セーブデータとの互換性
- イベントの順序制御

### 3. パフォーマンス
- イベントの適切な購読解除
- メモリリークの防止
- 初期化順序の制御

## 移行完了後の利点

### 1. 保守性の向上
- 単一責任原則の適用
- コードの分離と整理
- テストの容易さ

### 2. 拡張性の向上
- 新しい階層機能の追加が容易
- イベント駆動による疎結合
- モジュール化された設計

### 3. 再利用性の向上
- 他のプロジェクトでの再利用
- テンプレートとしての活用
- 標準化されたインターフェース

## 次のステップ

1. **Deck System テンプレートの作成**
2. **UI System テンプレートの作成**
3. **最終的な統合と最適化**

---

この移行により、プロジェクトの安定性と保守性が大幅に向上します。 