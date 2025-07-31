using UnityEngine;
using System;
using System.Collections;

[DefaultExecutionOrder(-30)]
public class Player : Unit
{
    public static Player Instance { get; private set; }
    
    // イベント定義
    public static event Action<Vector2Int> OnPlayerMoved;
    public static event Action<int> OnPlayerAttacked;
    public static event Action<int> OnPlayerHealed;
    public static event Action OnPlayerDied;
    public static event Action<int> OnPlayerLevelUp; // レベルアップイベントを追加
    
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;

    private bool isAwaitingMoveInput = false;
    private int allowedMoveDistance = 0;
    
    // 攻撃方向選択用の変数
    private bool isAwaitingAttackInput = false;
    private int attackPower = 0;
    
    // レベルシステム関連の変数
    [Header("Level System")]
    public int level = 1;
    public int exp = 0;
    public int expToNext = 10;
    public int maxLevel = 10;
    
    // 演出用の変数
    [Header("Effects")]
    public ParticleSystem moveEffect;
    public ParticleSystem attackEffect;
    public float moveAnimationDuration = 0.3f;
    public float attackAnimationDuration = 0.2f;
    public float jumpHeight = 0.5f;

    protected override void Awake()
    {
        base.Awake();
        
        // Singletonパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // コンポーネント参照の取得
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        
        // プレイヤー固有のHP設定
        maxHP = 20;
        currentHP = maxHP;
        
        // 基本的な変数の初期化
        gridPosition = new Vector2Int(2, 2);
        isAwaitingMoveInput = false;
        allowedMoveDistance = 0;
        isAwaitingAttackInput = false;
        attackPower = 0;
        
        // レベルシステムの初期化
        level = 1;
        exp = 0;
        expToNext = 10;
    }

    // 初期化完了後に呼ばれるメソッド
    public void InitializePosition()
    {
        if (GridManager.Instance == null) return;
        
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
        }
    }
    


    public void StartMoveSelection(int moveDistance)
    {
        if (GridManager.Instance == null) return;
        
        isAwaitingMoveInput = true;
        allowedMoveDistance = moveDistance;
        GridManager.Instance.HighlightMovableTiles(gridPosition, allowedMoveDistance);
    }
    
    // 攻撃方向選択を開始
    public void StartAttackSelection(int power)
    {
        if (GridManager.Instance == null) return;
        
        isAwaitingAttackInput = true;
        attackPower = power;
        GridManager.Instance.HighlightAttackableTiles(gridPosition);
    }

    public void OnTileClicked(Vector2Int clickedPos)
    {
        if (GridManager.Instance == null) return;

        // 攻撃方向選択中の場合
        if (isAwaitingAttackInput)
        {
            HandleAttackDirectionSelection(clickedPos);
            return;
        }
        
        // 移動選択中の場合
        if (isAwaitingMoveInput)
        {
            HandleMoveSelection(clickedPos);
            return;
        }
    }
    
    // 攻撃方向選択の処理
    private void HandleAttackDirectionSelection(Vector2Int clickedPos)
    {
        // 攻撃可能範囲内かチェック
        Vector2Int[] attackRange = GetAttackRange();
        bool isValidAttackTarget = false;
        
        foreach (Vector2Int attackPos in attackRange)
        {
            if (attackPos == clickedPos)
            {
                isValidAttackTarget = true;
                break;
            }
        }
        
        if (isValidAttackTarget)
        {
            // 攻撃実行
            Vector2Int attackDirection = clickedPos - gridPosition;
            ExecuteAttack(attackDirection, attackPower);
            
            // 状態をリセット
            isAwaitingAttackInput = false;
            attackPower = 0;
            GridManager.Instance.ResetAllTileColors();
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
        }
        else
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲外です");
            }
        }
    }
    
    // 敵クリック時の処理
    public void OnEnemyClicked(Vector2Int enemyPos)
    {
        // 攻撃選択中でない場合は無視
        if (!isAwaitingAttackInput) return;
        
        // 攻撃可能範囲内かチェック
        Vector2Int[] attackRange = GetAttackRange();
        bool isValidAttackTarget = false;
        
        foreach (Vector2Int attackPos in attackRange)
        {
            if (attackPos == enemyPos)
            {
                isValidAttackTarget = true;
                break;
            }
        }
        
        if (isValidAttackTarget)
        {
            // 攻撃実行
            Vector2Int attackDirection = enemyPos - gridPosition;
            ExecuteAttack(attackDirection, attackPower);
            
            // 状態をリセット
            isAwaitingAttackInput = false;
            attackPower = 0;
            GridManager.Instance.ResetAllTileColors();
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
        }
        else
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲外の敵です");
            }
        }
    }
    
    // 移動選択の処理
    private void HandleMoveSelection(Vector2Int clickedPos)
    {
        int dist = Mathf.Abs(clickedPos.x - gridPosition.x) + Mathf.Abs(clickedPos.y - gridPosition.y);
        
        Debug.Log($"Player: 移動選択 - クリック位置: {clickedPos}, 距離: {dist}, 最大距離: {allowedMoveDistance}, 歩行可能: {GridManager.Instance.IsWalkable(clickedPos)}");
        
        if (dist <= allowedMoveDistance && GridManager.Instance.IsWalkable(clickedPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = clickedPos;
            
            // 移動演出を実行
            StartCoroutine(AnimateMove(oldPos, clickedPos));

            isAwaitingMoveInput = false;
            GridManager.Instance.ResetAllTileColors();

            // 移動イベントを発行
            OnPlayerMoved?.Invoke(gridPosition);

            // カメラ追従と視界範囲更新
            NotifyCameraFollow();
            
            // Exit判定（階層進行用）
            CheckExit(clickedPos);
            
            // ターン終了
            if (TurnManager.Instance != null)
            {
                TurnManager.Instance.OnPlayerCardUsed();
            }
        }
        else
        {
            Debug.Log($"Player: 移動できません - 距離: {dist}, 最大距離: {allowedMoveDistance}, 歩行可能: {GridManager.Instance.IsWalkable(clickedPos)}");
        }
    }

    // Exit判定（段階5実装）
    private void CheckExit(Vector2Int newPos)
    {
        if (GridManager.Instance != null && GridManager.Instance.exitPosition == newPos)
        {
            Debug.Log("Player: Exitに到達！次の階層に進みます");
            
            // 階段到達時の経験値獲得
            GainExp(5); // 階段到達で5経験値獲得
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("Exitに到達！次の階層に進みます");
            }
            
            // 次の階層に進む
            if (GameManager.Instance != null)
            {
                GameManager.Instance.GoToNextFloor();
            }
        }
    }
    
    // プレイヤーの位置をリセット（段階4実装）
    public void ResetPlayerPosition()
    {
        Debug.Log("Player: 位置をリセットします");
        
        // 中央位置にリセット
        gridPosition = new Vector2Int(2, 2);
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // HPを回復（階層進行の報酬）
        currentHP = maxHP;
        
        // 状態をリセット
        isAwaitingMoveInput = false;
        allowedMoveDistance = 0;
        isAwaitingAttackInput = false;
        attackPower = 0;
        
        // カメラ追従と視界範囲更新
        NotifyCameraFollow();
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.AddLog($"階層 {GameManager.Instance.currentFloor} に到達！HPが回復しました");
        }
        
        Debug.Log("Player: 位置リセット完了");
    }

    public void Move(Vector2Int delta)
    {
        Vector2Int newPos = gridPosition + delta;
        
        if (GridManager.Instance.IsWalkable(newPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = newPos;
            
            // 移動演出を実行
            StartCoroutine(AnimateMove(oldPos, newPos));
            
            // カメラ追従通知
            NotifyCameraFollow();
        }
    }

    public void MoveWithDistance(Vector2Int direction, int distance)
    {
        Vector2Int totalMove = direction * distance;
        Vector2Int newPos = gridPosition + totalMove;
        
        if (GridManager.Instance.IsWalkable(newPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = newPos;
            
            // 移動演出を実行
            StartCoroutine(AnimateMove(oldPos, newPos));
            
            // カメラ追従通知
            NotifyCameraFollow();
            
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"移動！方向: {direction}, 距離: {distance}");
            }
            
            // 効果音再生
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlaySound("Select");
            }
            

        }
        else
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog($"移動できません！位置: {newPos}");
            }
        }
    }

    public void SetPosition(Vector2Int newPos)
    {
        gridPosition = newPos;
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // カメラ追従通知
        NotifyCameraFollow();
    }

    public void ExecuteCardEffect(CardDataSO card)
    {
        if (card == null) return;

        switch (card.type)
        {
            case CardType.Attack:
                StartAttackSelection(card.GetEffectivePower());
                break;
            case CardType.Heal:
                Heal(card.GetEffectiveHealAmount());
                break;
            case CardType.Move:
                MoveWithDistance(card.moveDirection, card.GetEffectiveMoveDistance());
                break;
        }
    }
    
    // 攻撃実行（方向指定）
    public void ExecuteAttack(Vector2Int direction, int damage)
    {
        Vector2Int targetPos = gridPosition + direction;
        
        // 攻撃イベントを発行
        OnPlayerAttacked?.Invoke(damage);
        
        // 攻撃演出を実行
        StartCoroutine(AnimateAttack(direction, damage));
        
        // 目標位置にいる敵を探して攻撃
        bool hitEnemy = false;
        
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemies();
            
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null && enemy.gridPosition == targetPos)
                {
                    enemy.TakeDamage(damage);
                    hitEnemy = true;
                    
                    // UI更新
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.AddLog($"敵に{damage}ダメージを与えた！");
                    }
                    
                    // 攻撃エフェクト再生
                    PlayAttackEffect(targetPos);
                    break;
                }
            }
        }
        
        if (!hitEnemy)
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲に敵がいません");
            }
            
            // 空振りエフェクト再生
            PlayMissEffect(targetPos);
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Attack");
        }
    }
    
    // 移動演出
    private IEnumerator AnimateMove(Vector2Int fromPos, Vector2Int toPos)
    {
        Vector3 startPos = GridManager.Instance.GetWorldPosition(fromPos);
        Vector3 endPos = GridManager.Instance.GetWorldPosition(toPos);
        
        // 移動エフェクトを再生
        if (moveEffect != null)
        {
            moveEffect.transform.position = startPos;
            moveEffect.Play();
        }
        
        // ジャンプしながら移動するアニメーション
        Vector3 midPos = (startPos + endPos) / 2f + Vector3.up * jumpHeight;
        
        float elapsed = 0f;
        float halfDuration = moveAnimationDuration / 2f;
        
        // 前半：開始位置から中間位置へ
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad
            
            transform.position = Vector3.Lerp(startPos, midPos, easeT);
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, easeT);
            
            yield return null;
        }
        
        // 後半：中間位置から終了位置へ
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t; // EaseInQuad
            
            transform.position = Vector3.Lerp(midPos, endPos, easeT);
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, easeT);
            
            yield return null;
        }
        
        // 最終位置を確実に設定
        transform.position = endPos;
        spriteRenderer.transform.localScale = Vector3.one;
    }
    
    // 攻撃演出
    private IEnumerator AnimateAttack(Vector2Int direction, int damage)
    {
        // 攻撃方向に向かって少し移動するアニメーション
        Vector3 attackOffset = new Vector3(direction.x, direction.y, 0) * 0.3f;
        Vector3 originalPos = transform.position;
        Vector3 attackPos = originalPos + attackOffset;
        
        float elapsed = 0f;
        float halfDuration = attackAnimationDuration / 2f;
        
        // 前半：攻撃位置へ移動
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad
            
            transform.position = Vector3.Lerp(originalPos, attackPos, easeT);
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, easeT);
            
            yield return null;
        }
        
        // 攻撃エフェクトを再生
        if (attackEffect != null)
        {
            attackEffect.transform.position = originalPos + attackOffset;
            attackEffect.Play();
        }
        
        // 後半：元の位置へ戻る
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t; // EaseInQuad
            
            transform.position = Vector3.Lerp(attackPos, originalPos, easeT);
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, easeT);
            
            yield return null;
        }
        
        // 最終位置を確実に設定
        transform.position = originalPos;
        spriteRenderer.transform.localScale = Vector3.one;
    }
    
    // 攻撃エフェクト再生
    private void PlayAttackEffect(Vector2Int targetPos)
    {
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(targetPos);
        
        // ヒットエフェクトを生成
        GameObject hitEffect = new GameObject("HitEffect");
        hitEffect.transform.position = worldPos;
        
        // パーティクルシステムを追加
        ParticleSystem hitParticles = hitEffect.AddComponent<ParticleSystem>();
        var main = hitParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.2f;
        main.maxParticles = 10;
        
        var emission = hitParticles.emission;
        emission.rateOverTime = 20;
        
        var shape = hitParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // 0.5秒後に削除
        Destroy(hitEffect, 0.5f);
    }
    
    // 空振りエフェクト再生
    private void PlayMissEffect(Vector2Int targetPos)
    {
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(targetPos);
        
        // 空振りエフェクトを生成
        GameObject missEffect = new GameObject("MissEffect");
        missEffect.transform.position = worldPos;
        
        ParticleSystem missParticles = missEffect.AddComponent<ParticleSystem>();
        var main = missParticles.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.maxParticles = 5;
        main.startColor = Color.gray;
        
        var emission = missParticles.emission;
        emission.rateOverTime = 10;
        
        var shape = missParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        // 0.3秒後に削除
        Destroy(missEffect, 0.3f);
    }

    public void Attack(int damage)
    {
        // 攻撃イベントを発行
        OnPlayerAttacked?.Invoke(damage);
        
        // 攻撃範囲内の敵を探して攻撃
        bool hitEnemy = false;
        
        if (EnemyManager.Instance != null)
        {
            var enemies = EnemyManager.Instance.GetEnemies();
            Vector2Int[] attackRange = GetAttackRange();
            
            foreach (Enemy enemy in enemies)
            {
                if (enemy != null)
                {
                    // 攻撃範囲内かチェック
                    bool inAttackRange = false;
                    foreach (Vector2Int attackPos in attackRange)
                    {
                        if (attackPos == enemy.gridPosition)
                        {
                            inAttackRange = true;
                            break;
                        }
                    }
                    
                    if (inAttackRange)
                    {
                        enemy.TakeDamage(damage);
                        hitEnemy = true;
                        
                        // UI更新
                        if (UIManager.Instance != null)
                        {
                            UIManager.Instance.AddLog($"敵に{damage}ダメージを与えた！");
                        }
                        
                        // 1つの敵のみを攻撃（最初に見つかった敵）
                        break;
                    }
                }
            }
        }
        
        if (!hitEnemy)
        {
            // UI更新
            if (UIManager.Instance != null)
            {
                UIManager.Instance.AddLog("攻撃範囲に敵がいません");
            }
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Attack");
        }
    }

    public Vector2Int[] GetAttackRange()
    {
        // 攻撃範囲（隣接マス + 斜め）を返す
        return new Vector2Int[]
        {
            gridPosition + Vector2Int.up,
            gridPosition + Vector2Int.down,
            gridPosition + Vector2Int.left,
            gridPosition + Vector2Int.right,
            gridPosition + new Vector2Int(1, 1),   // 右上
            gridPosition + new Vector2Int(-1, 1),  // 左上
            gridPosition + new Vector2Int(1, -1),  // 右下
            gridPosition + new Vector2Int(-1, -1)  // 左下
        };
    }

    private void NotifyCameraFollow()
    {
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.OnPlayerMoved(transform.position);
        }
        
        // 視界範囲を更新
        if (GridManager.Instance != null)
        {
            GridManager.Instance.UpdateTileVisibility(gridPosition);
        }
    }

    public void Heal(int amount)
    {
        int oldHP = currentHP;
        currentHP += amount;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        int actualHeal = currentHP - oldHP;
        
        // 回復イベントを発行
        OnPlayerHealed?.Invoke(actualHeal);
        
        // 回復演出を実行
        StartCoroutine(AnimateHeal());
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.AddLog($"回復！回復量: {actualHeal}, HP: {currentHP}/{maxHP}");
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Heal");
        }
    }
    
    // 回復演出
    private IEnumerator AnimateHeal()
    {
        // 回復中はスプライトを緑色に光らせる
        float elapsed = 0f;
        float duration = 0.4f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color targetColor = Color.Lerp(Color.white, Color.green, Mathf.Sin(t * Mathf.PI * 2));
            spriteRenderer.color = targetColor;
            
            yield return null;
        }
        
        spriteRenderer.color = Color.white;
        
        // 回復エフェクトを生成
        GameObject healEffect = new GameObject("HealEffect");
        healEffect.transform.position = transform.position;
        
        ParticleSystem healParticles = healEffect.AddComponent<ParticleSystem>();
        var main = healParticles.main;
        main.startLifetime = 1f;
        main.startSpeed = 1f;
        main.startSize = 0.1f;
        main.maxParticles = 20;
        main.startColor = Color.green;
        
        var emission = healParticles.emission;
        emission.rateOverTime = 30;
        
        var shape = healParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        // 1秒後に削除
        Destroy(healEffect, 1f);
    }

    // 経験値獲得メソッド
    public void GainExp(int amount)
    {
        if (level >= maxLevel) return; // 最大レベルに達している場合は経験値を獲得しない
        
        exp += amount;
        Debug.Log($"Player: 経験値獲得！+{amount} (現在: {exp}/{expToNext})");
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"経験値獲得！+{amount}");
            UIManager.Instance.UpdateLevelDisplay(level, exp, expToNext);
        }
        
        // レベルアップチェック
        while (exp >= expToNext && level < maxLevel)
        {
            exp -= expToNext;
            LevelUp();
        }
    }
    
    // レベルアップメソッド
    private void LevelUp()
    {
        level++;
        expToNext = Mathf.RoundToInt(expToNext * 1.5f); // 次のレベルに必要な経験値を1.5倍に増加
        
        // レベルアップ時の能力向上
        int oldMaxHP = maxHP;
        maxHP += 5; // HPを5増加
        currentHP = maxHP; // レベルアップ時はHPを全回復
        
        Debug.Log($"Player: レベルアップ！レベル {level}、HP {oldMaxHP}→{maxHP}");
        
        // レベルアップイベントを発行
        OnPlayerLevelUp?.Invoke(level);
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdateHP(currentHP, maxHP);
            UIManager.Instance.UpdateLevelDisplay(level, exp, expToNext);
            UIManager.Instance.AddLog($"レベルアップ！レベル {level}、HP {oldMaxHP}→{maxHP}");
        }
        
        // レベルアップ演出を実行
        StartCoroutine(AnimateLevelUp());
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("LevelUp");
        }
    }
    
    // レベルアップ演出
    private IEnumerator AnimateLevelUp()
    {
        // レベルアップ中はスプライトを金色に光らせる
        float elapsed = 0f;
        float duration = 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color targetColor = Color.Lerp(Color.white, Color.yellow, Mathf.Sin(t * Mathf.PI * 4));
            spriteRenderer.color = targetColor;
            
            // スケールも少し大きくする
            float scale = 1f + Mathf.Sin(t * Mathf.PI * 4) * 0.2f;
            spriteRenderer.transform.localScale = Vector3.one * scale;
            
            yield return null;
        }
        
        spriteRenderer.color = Color.white;
        spriteRenderer.transform.localScale = Vector3.one;
        
        // レベルアップエフェクトを生成
        GameObject levelUpEffect = new GameObject("LevelUpEffect");
        levelUpEffect.transform.position = transform.position;
        
        ParticleSystem levelUpParticles = levelUpEffect.AddComponent<ParticleSystem>();
        var main = levelUpParticles.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 3f;
        main.startSize = 0.2f;
        main.maxParticles = 30;
        main.startColor = Color.yellow;
        
        var emission = levelUpParticles.emission;
        emission.rateOverTime = 40;
        
        var shape = levelUpParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        // 1.5秒後に削除
        Destroy(levelUpEffect, 1.5f);
    }

    protected override void Die()
    {
        // 死亡イベントを発行
        OnPlayerDied?.Invoke();
        
        // 死亡演出を実行
        StartCoroutine(AnimateDeath());
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
    
    // 死亡演出
    private IEnumerator AnimateDeath()
    {
        // フェードアウト
        float elapsed = 0f;
        float duration = 1f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color color = spriteRenderer.color;
            color.a = 1f - t;
            spriteRenderer.color = color;
            
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.5f, t);
            
            yield return null;
        }
        
        // 死亡エフェクトを生成
        GameObject deathEffect = new GameObject("DeathEffect");
        deathEffect.transform.position = transform.position;
        
        ParticleSystem deathParticles = deathEffect.AddComponent<ParticleSystem>();
        var main = deathParticles.main;
        main.startLifetime = 2f;
        main.startSpeed = 2f;
        main.startSize = 0.3f;
        main.maxParticles = 50;
        main.startColor = Color.red;
        
        var emission = deathParticles.emission;
        emission.rateOverTime = 50;
        
        var shape = deathParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        // 2秒後に削除
        Destroy(deathEffect, 2f);
        
        gameObject.SetActive(false);
    }
} 