# ミニマップ実装セットアップガイド

## 概要
トルネコ風ローグライクカードバトルゲームのミニマップ機能を実装しました。

## 実装済み機能
- ✅ GridManagerにTile管理Dictを追加
- ✅ MiniMapTile.cs（ミニマップ用タイルコンポーネント）
- ✅ MiniMapManager.cs（ミニマップ管理）
- ✅ EnemyManagerに敵位置取得メソッドを追加
- ✅ TurnManagerにミニマップ更新処理を追加

## セットアップ手順

### 1. MiniMapTileプレハブの作成

1. **UI → Image**をHierarchyに作成
2. **サイズ設定**: 10x10ピクセル
3. **MiniMapTile.cs**をアタッチ
4. **プレハブ化**: Prefabs/UI/MiniMapTile.prefabとして保存

### 2. MiniMapPanelの作成

1. **Canvas**をHierarchyに作成（既存のCanvasがある場合は使用）
2. **Canvas内にImage**を作成: `MiniMapPanel`
3. **Anchor設定**: Top-Right
4. **サイズ設定**: 150x150ピクセル
5. **位置調整**: 右上に配置

### 3. MiniMapManagerの設定

1. **MiniMapManager**をHierarchyに追加
2. **Inspector設定**:
   - `miniMapPanel`: MiniMapPanelのRectTransformを設定
   - `miniMapTilePrefab`: MiniMapTileプレハブを設定
   - `playerColor`: Color.green（プレイヤー色）
   - `enemyColor`: Color.red（敵色）
   - `defaultColor`: Color.gray（デフォルト色）
   - `exitColor`: Color.yellow（出口色）

### 4. 動作確認

1. **プレイヤー**: 緑色で表示
2. **敵**: 赤色で表示
3. **出口**: 黄色で表示
4. **その他**: グレーで表示

## 自動更新機能

以下のタイミングでミニマップが自動更新されます：
- ゲーム初期化完了時
- プレイヤー移動時
- ユニット移動時

## トラブルシューティング

### ミニマップが表示されない場合
1. MiniMapManagerの設定を確認
2. MiniMapTileプレハブにMiniMapTile.csがアタッチされているか確認
3. Canvasの設定を確認

### 色が正しく表示されない場合
1. MiniMapManagerの色設定を確認
2. MiniMapTileのImageコンポーネントを確認

### 更新されない場合
1. TurnManagerのイベント購読を確認
2. 各Managerのインスタンスが正しく生成されているか確認

## 拡張可能な機能

- 視界範囲の表示
- 地形情報の表示
- アイテム位置の表示
- ミニマップのズーム機能
- ミニマップのクリック機能

## 実装済みファイル一覧

- `Scripts/Manager/GridManager.cs` - Tile管理Dict追加
- `Scripts/UI/MiniMapTile.cs` - ミニマップ用タイル
- `Scripts/Manager/MiniMapManager.cs` - ミニマップ管理
- `Scripts/Manager/EnemyManager.cs` - 敵位置取得メソッド追加
- `Scripts/Manager/TurnManager.cs` - ミニマップ更新処理追加 