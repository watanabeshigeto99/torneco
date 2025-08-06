# ミニマップ実装チェックリスト

## 実装済み項目 ✅

### 基盤準備
- [x] GridManagerにTile管理Dictを追加
- [x] Tile.cs：1マスに対応、座標と色表示
- [x] Player.cs：gridPositionを持つ
- [x] Enemy.cs：gridPositionを持つ
- [x] Unit.cs：HPなど共通情報を扱う

### ミニマップ機能
- [x] MiniMapTile.cs（ミニマップ用タイルコンポーネント）
- [x] MiniMapManager.cs（ミニマップ管理）
- [x] EnemyManagerに敵位置取得メソッド追加
- [x] TurnManagerにミニマップ更新処理追加

### 自動更新機能
- [x] ゲーム初期化完了時の更新
- [x] プレイヤー移動時の更新
- [x] ユニット移動時の更新

## セットアップが必要な項目 ⚠️

### Unity側の設定
- [ ] MiniMapTileプレハブの作成
- [ ] MiniMapPanelの作成
- [ ] MiniMapManagerの設定
- [ ] Canvasの設定確認

### 動作確認
- [ ] プレイヤー位置が緑色で表示される
- [ ] 敵位置が赤色で表示される
- [ ] 出口位置が黄色で表示される
- [ ] 毎ターン自動でミニマップが更新される

## 実装詳細

### GridManager.cs 変更点
```csharp
// ミニマップ用のTile管理Dictを追加
public Dictionary<Vector2Int, Tile> tileDict = new();

// GenerateGridメソッドでTile管理Dictに登録
tileDict[pos] = tileScript;
```

### MiniMapTile.cs 新規作成
```csharp
public class MiniMapTile : MonoBehaviour
{
    private Vector2Int gridPos;
    public Image image;
    
    public void Initialize(Vector2Int pos)
    public void SetColor(Color c)
}
```

### MiniMapManager.cs 新規作成
```csharp
public class MiniMapManager : MonoBehaviour
{
    public RectTransform miniMapPanel;
    public GameObject miniMapTilePrefab;
    
    public void GenerateMiniMap(int width, int height)
    public void UpdateMiniMap(Vector2Int playerPos, List<Vector2Int> enemyPositions, Vector2Int exitPos)
}
```

### EnemyManager.cs 追加
```csharp
// ミニマップ用：全ての敵の位置を取得
public List<Vector2Int> GetAllEnemyPositions()
```

### TurnManager.cs 追加
```csharp
// ミニマップ更新処理
private void UpdateMiniMap()

// 各イベントハンドラーでUpdateMiniMap()を呼び出し
```

## 次のステップ

1. **Unity側のセットアップ**:
   - MiniMapTileプレハブの作成
   - MiniMapPanelの作成
   - MiniMapManagerの設定

2. **動作確認**:
   - ミニマップの表示確認
   - 色分けの確認
   - 自動更新の確認

3. **拡張機能の検討**:
   - 視界範囲の表示
   - 地形情報の表示
   - ミニマップのインタラクション機能 