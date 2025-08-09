# DamageVFX システム セットアップガイド

## 概要
このシステムは、プレイヤーと敵のダメージエフェクトを統一的に管理するためのコンポーネントです。

## 実装済み機能
1. **スプライトのフラッシュ（赤点滅）** - ダメージを受けた時にスプライトが赤く点滅
2. **ノックバック（数ピクセルだけ押し戻す）** - ダメージを受けた方向に押し戻される
3. **ヒットストップ（0.06秒だけ時間停止）** - ダメージを受けた瞬間に時間が停止
4. **カメラシェイク（弱めの揺れ）** - ダメージを受けた時にカメラが揺れる
5. **パーティクル（ヒット火花）** - ダメージを受けた位置に火花エフェクト

## セットアップ手順

### 1. DamageVFXコンポーネントの追加

#### Player プレハブへの追加
1. `Prefabs/Player.prefab` を開く
2. Player GameObject に `DamageVFX` コンポーネントを追加
3. 以下の設定を行う：
   - **Target Sprite**: Player の SpriteRenderer を設定
   - **Flash Color**: (1, 0.2, 0.2, 1) - 赤色
   - **Flash Duration**: 0.12
   - **Knockback Distance**: 0.12
   - **Knockback Time**: 0.06
   - **Hit Stop**: 0.06
   - **Shake Time**: 0.12
   - **Shake Strength**: 0.08

#### Enemy プレハブへの追加
1. `Prefabs/Enemy.prefab` を開く
2. Enemy GameObject に `DamageVFX` コンポーネントを追加
3. Player と同じ設定を行う

### 2. パーティクルシステムの作成

#### HitSpark パーティクルシステムの作成
1. プロジェクト内で `Create > Effects > Particle System` を選択
2. 作成されたGameObjectを `HitSpark` と名付ける
3. 以下の設定を行う：

**Main モジュール:**
- Duration: 0.3
- Start Lifetime: 0.15
- Start Speed: 1.2
- Start Size: 0.08
- Max Particles: 50

**Emission モジュール:**
- Bursts: 1回（Count: 12）

**Shape モジュール:**
- Shape Type: Circle
- Radius: 0.05

**Renderer モジュール:**
- Material: Default-Particle

4. `HitSparkParticle.cs` スクリプトを追加
5. プレハブとして保存（`Prefabs/HitSpark.prefab`）

### 3. DamageVFXへのパーティクルシステムの割り当て

1. Player と Enemy の DamageVFX コンポーネントの `Hit Spark` フィールドに、作成した HitSpark プレハブを割り当て

### 4. カメラの設定

1. メインカメラの Transform を DamageVFX コンポーネントの `Cam` フィールドに割り当て
2. または、自動で `Camera.main.transform` が設定される

## 使用方法

### 基本的な使用方法
DamageVFX は `Unit.TakeDamage()` メソッドに自動的にフックされます。

```csharp
// 自動的に DamageVFX が再生される
player.TakeDamage(5, enemy);
```

### 手動での再生
必要に応じて手動で DamageVFX を再生することも可能です：

```csharp
var damageVFX = GetComponent<DamageVFX>();
if (damageVFX != null)
{
    Vector2 hitDirection = (transform.position - attacker.transform.position).normalized;
    damageVFX.Play(hitDirection);
}
```

## カスタマイズ

### エフェクトの調整
各エフェクトのパラメータは DamageVFX コンポーネントの Inspector から調整可能です：

- **Flash**: フラッシュの色と時間
- **Knockback**: ノックバックの距離と時間
- **Hit Stop**: ヒットストップの時間
- **Camera Shake**: カメラシェイクの強度と時間
- **Particles**: パーティクルシステムの参照

### 新しいエフェクトの追加
新しいエフェクトを追加する場合は、`DamageVFX.cs` の `Co_Play` メソッドに新しいコルーチンを追加してください。

## トラブルシューティング

### よくある問題

1. **フラッシュが表示されない**
   - Target Sprite が正しく設定されているか確認
   - SpriteRenderer コンポーネントが存在するか確認

2. **ノックバックが動作しない**
   - Knockback Distance と Knockback Time が 0 より大きいか確認
   - hitDirection が正しく渡されているか確認

3. **カメラシェイクが動作しない**
   - Cam フィールドにカメラの Transform が設定されているか確認
   - Camera.main が存在するか確認

4. **パーティクルが表示されない**
   - Hit Spark フィールドにパーティクルシステムが割り当てられているか確認
   - パーティクルシステムの設定が正しいか確認

## 注意事項

- DamageVFX は `StopAllCoroutines()` を使用するため、他のコルーチンと競合する可能性があります
- ヒットストップは `Time.timeScale` を変更するため、他のシステムに影響を与える可能性があります
- カメラシェイクは `localPosition` を変更するため、カメラの親オブジェクトがある場合は注意が必要です
