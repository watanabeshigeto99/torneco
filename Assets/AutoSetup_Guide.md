# 🔧 AutoSetupManager 使用ガイド

## 📋 概要

AutoSetupManagerは、SOアセットの設定を自動化するシステムです。Unity Editorで手動設定を行う必要がなくなり、プロジェクトの初期化が簡単になります。

## 🚀 使用方法

### **1. 自動設定（推奨）**

1. **シーンにAutoSetupManagerを追加**：
   - 空のGameObjectを作成
   - `AutoSetupManager`コンポーネントを追加
   - または、メニューから「Tools > Auto Setup > Setup All Assets」を実行

2. **設定の確認**：
   - Consoleで「AutoSetupManager: 自動設定が完了しました」を確認
   - または、メニューから「Tools > Auto Setup > Show Setup Status」で状況確認

### **2. 手動設定**

#### **エディタメニューから実行**：
- `Tools > Auto Setup > Setup All Assets` - 全アセットの設定
- `Tools > Auto Setup > Setup Event Channels` - イベントチャンネルのみ設定
- `Tools > Auto Setup > Setup Data Objects` - データオブジェクトのみ設定
- `Tools > Auto Setup > Show Setup Status` - 設定状況の表示

#### **コードから実行**：
```csharp
// AutoSetupManagerの取得
var autoSetup = FindObjectOfType<AutoSetupManager>();
if (autoSetup != null)
{
    // 全設定の実行
    autoSetup.ReRunAutoSetup();
    
    // 個別設定の実行
    autoSetup.SetupEventChannels();
    autoSetup.SetupDataObjects();
    
    // 設定状況の確認
    Debug.Log(autoSetup.GetSetupStatus());
}
```

## 📁 ファイル構造

```
Assets/
├── Resources/
│   └── SO/
│       ├── SaveSystem/
│       │   ├── SaveEventChannel.asset
│       │   └── SaveDataSO.asset
│       ├── DeckSystem/
│       │   ├── DeckEventChannel.asset
│       │   └── DeckDataSO.asset
│       ├── UISystem/
│       │   └── UIEventChannel.asset
│       └── PlayerDataSystem/
│           └── PlayerDataSO.asset
└── Scripts/
    └── Manager/
        └── AutoSetupManager.cs
```

## ⚙️ 設定項目

### **Event Channels（イベントチャンネル）**
- `SaveEventChannel` → `GameManager.saveEventChannel`、`SaveManager.saveEventChannel`
- `DeckEventChannel` → `GameManager.deckEventChannel`、`DeckManager.deckEventChannel`
- `UIEventChannel` → `GameManager.uiEventChannel`

### **Data Objects（データオブジェクト）**
- `SaveDataSO` → `SaveManager.saveData`
- `PlayerDataSO` → `PlayerDataManager.playerData`
- `DeckDataSO` → `DeckManager.deckData`

### **Managers（マネージャー）**
- 新しいシステムマネージャーの参照設定
- GameManagerとの統合設定

## 🔍 設定状況の確認

### **Console出力例**：
```
AutoSetupManager: 自動設定を開始します
AutoSetupManager: イベントチャンネルの設定を開始
AutoSetupManager: SaveEventChannelをResourcesから検索しました
AutoSetupManager: SaveEventChannelをGameManagerに設定しました
AutoSetupManager: SaveEventChannelをSaveManagerに設定しました
AutoSetupManager: イベントチャンネルの設定が完了しました
AutoSetupManager: データオブジェクトの設定を開始
AutoSetupManager: SaveDataSOをResourcesから検索しました
AutoSetupManager: SaveDataSOをSaveManagerに設定しました
AutoSetupManager: データオブジェクトの設定が完了しました
AutoSetupManager: マネージャーの設定を開始
AutoSetupManager: GameManagerの設定が完了しました
AutoSetupManager: マネージャーの設定が完了しました
AutoSetupManager: 自動設定が完了しました
```

### **設定状況の表示**：
```
Event Channels: ✅
Data Objects: ✅
Managers: ✅
SaveEventChannel: ✅
DeckEventChannel: ✅
UIEventChannel: ✅
SaveDataSO: ✅
PlayerDataSO: ✅
DeckDataSO: ✅
GameManager: ✅
SaveManager: ✅
PlayerDataManager: ✅
DeckManager: ✅
```

## 🛠️ トラブルシューティング

### **問題1: アセットが見つからない**
**症状**: `AutoSetupManager: SaveEventChannelが見つかりませんでした`
**解決策**: 
- Resources/SO/フォルダにアセットが配置されているか確認
- アセット名が正確か確認
- Unity Editorでアセットを再読み込み

### **問題2: 設定が反映されない**
**症状**: 設定後もnull参照エラーが発生
**解決策**:
- `ReRunAutoSetup()`を実行
- シーンを再読み込み
- 各マネージャーのAwake()で設定を確認

### **問題3: エディタメニューが表示されない**
**症状**: Tools > Auto Setupメニューが表示されない
**解決策**:
- スクリプトのコンパイルを待つ
- Unity Editorを再起動
- `AutoSetupManager.cs`のコンパイルエラーを確認

## 📝 カスタマイズ

### **新しいアセットの追加**：
1. Resources/SO/フォルダにアセットを配置
2. `AutoSetupManager.cs`の`SetupEventChannels()`または`SetupDataObjects()`に設定ロジックを追加
3. 対応するマネージャーにフィールドを追加

### **設定の無効化**：
```csharp
// AutoSetupManagerのInspectorで設定
enableAutoSetup = false;        // 自動設定を無効化
enableEventChannelSetup = false; // イベントチャンネル設定を無効化
enableDataObjectSetup = false;   // データオブジェクト設定を無効化
```

## ✅ 動作確認

1. **シーンの開始時**：
   - Consoleに「AutoSetupManager: 自動設定が完了しました」が表示される
   - 各マネージャーのInspectorでSOアセットが設定されている

2. **手動実行時**：
   - メニューから「Tools > Auto Setup > Setup All Assets」を実行
   - Consoleに設定状況が表示される

3. **設定状況の確認**：
   - メニューから「Tools > Auto Setup > Show Setup Status」を実行
   - 各項目に✅が表示される

## 🎯 メリット

- ✅ **手動設定不要**: Unity Editorでの手動設定が不要
- ✅ **自動検出**: SOアセットを自動的に検出・設定
- ✅ **エラー防止**: null参照エラーを防止
- ✅ **開発効率向上**: プロジェクト初期化が簡単
- ✅ **チーム開発対応**: 全開発者で同じ設定を共有
- ✅ **バージョン管理**: 設定変更をGitで追跡可能

---

**注意**: このシステムは開発時の利便性を向上させるためのものです。本番環境では、適切な設定管理を行ってください。 