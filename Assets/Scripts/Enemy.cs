using UnityEngine;
using System.Collections;

[DefaultExecutionOrder(-20)]
public class Enemy : Unit
{
    public Vector2Int gridPosition;
    public SpriteRenderer spriteRenderer;
    
    // SOデータ
    [Header("Enemy Data")]
    public EnemyDataSO enemyData;
    
    // 動的な状態
    private bool hasMovedThisTurn = false;
    private Vector2Int lastPosition;
    
    // 演出用の変数
    [Header("Effects")]
    public ParticleSystem moveEffect;
    public ParticleSystem attackEffect;
    public float moveAnimationDuration = 0.4f;
    public float attackAnimationDuration = 0.3f;
    public float jumpHeight = 0.3f;

    protected override void Awake()
    {
        base.Awake();
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            
        SetupForClicking();
    }
    
    // 初期化（SOデータを使用）
    public void Initialize(Vector2Int startPos, EnemyDataSO data = null)
    {
        gridPosition = startPos;
        lastPosition = startPos;
        Vector3 worldPos = GridManager.Instance.GetWorldPosition(gridPosition);
        transform.position = worldPos;
        
        // SOデータを設定
        if (data != null)
        {
            enemyData = data;
            ApplyEnemyData();
        }
        else if (enemyData != null)
        {
            ApplyEnemyData();
        }
    }
    
    // SOデータを適用
    private void ApplyEnemyData()
    {
        if (enemyData == null) return;
        
        // HP設定
        maxHP = enemyData.maxHP;
        currentHP = maxHP;
        
        // スプライト設定
        if (spriteRenderer != null)
        {
            if (enemyData.sprite != null)
            {
                spriteRenderer.sprite = enemyData.sprite;
            }
            spriteRenderer.color = enemyData.spriteColor;
            spriteRenderer.sortingOrder = enemyData.sortingOrder;
        }
        
        // Collider2D設定
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = enemyData.colliderSize;
        }
    }
    
    // クリック可能にするための設定
    private void SetupForClicking()
    {
        // Collider2Dの設定を確認・調整
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            if (enemyData != null)
            {
                collider.size = enemyData.colliderSize;
            }
            else
            {
                collider.size = new Vector2(1f, 1f);
            }
        }
        else
        {
            // Collider2Dがない場合は追加
            collider = gameObject.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = enemyData != null ? enemyData.colliderSize : new Vector2(1f, 1f);
        }
        
        // SpriteRendererのSortingOrderを設定
        if (spriteRenderer != null)
        {
            int order = enemyData != null ? enemyData.sortingOrder : 10;
            spriteRenderer.sortingOrder = order;
        }
    }
    
    // マウスクリックで攻撃対象として選択
    private void OnMouseDown()
    {
        if (Player.Instance != null)
        {
            Player.Instance.OnEnemyClicked(gridPosition);
        }
    }

    public void Act()
    {
        // 非アクティブの場合は処理をスキップ
        if (!gameObject.activeInHierarchy) return;
        
        hasMovedThisTurn = false;
        lastPosition = gridPosition;
        
        // 移動パターンに応じて行動
        if (enemyData != null)
        {
            switch (enemyData.movementPattern)
            {
                case MovementPattern.Random:
                    MoveRandomly();
                    break;
                case MovementPattern.Chase:
                    MoveTowardsPlayer();
                    break;
                case MovementPattern.Patrol:
                    MoveInPatrol();
                    break;
                case MovementPattern.Stationary:
                    break;
            }
        }
        else
        {
            MoveRandomly();
        }
        
        // プレイヤーとの距離をチェックして、遠い場合は追跡移動を追加
        if (Player.Instance != null)
        {
            Vector2Int playerPos = Player.Instance.gridPosition;
            int distanceToPlayer = GetGridDistance(playerPos, gridPosition);
            
            if (distanceToPlayer > 3)
            {
                MoveTowardsPlayer();
            }
        }
        
        // 攻撃パターンに応じて攻撃
        if (enemyData != null && enemyData.canAttack)
        {
            switch (enemyData.attackPattern)
            {
                case AttackPattern.Melee:
                    TryAttackPlayerMelee();
                    break;
                case AttackPattern.Ranged:
                    TryAttackPlayerRanged();
                    break;
                case AttackPattern.Area:
                    TryAttackPlayerArea();
                    break;
                case AttackPattern.Special:
                    TryAttackPlayerSpecial();
                    break;
            }
        }
        else
        {
            TryAttackPlayerMelee();
        }
    }
    
    // ランダム移動
    private void MoveRandomly()
    {
        if (enemyData != null && !enemyData.canMove) return;
        if (!gameObject.activeInHierarchy) return; // 非アクティブの場合は処理をスキップ
        
        Vector2Int[] directions = {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        
        // プレイヤーとの距離をチェック
        Vector2Int preferredDirection = Vector2Int.zero;
        if (Player.Instance != null)
        {
            Vector2Int playerPos = Player.Instance.gridPosition;
            int distanceToPlayer = GetGridDistance(playerPos, gridPosition);
            
            if (distanceToPlayer > 2)
            {
                preferredDirection = NormalizeVector2Int(playerPos - gridPosition);
            }
        }
        
        Vector2Int dir;
        if (preferredDirection != Vector2Int.zero && UnityEngine.Random.Range(0f, 1f) < 0.7f)
        {
            dir = preferredDirection;
        }
        else
        {
            dir = directions[UnityEngine.Random.Range(0, directions.Length)];
        }
        
        Vector2Int newPos = gridPosition + dir;

        if (GridManager.Instance.IsInsideGrid(newPos) && !GridManager.Instance.IsOccupied(newPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = newPos;
            
            // 移動演出を実行（アクティブな場合のみ）
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateMove(oldPos, newPos));
            }
            else
            {
                // 非アクティブの場合は即座に位置を更新
                transform.position = GridManager.Instance.GetWorldPosition(newPos);
            }
            
            hasMovedThisTurn = true;
            
            // 敵の移動後に視界範囲を更新
            if (Player.Instance != null && GridManager.Instance != null)
            {
                GridManager.Instance.UpdateTileVisibility(Player.Instance.gridPosition);
            }
        }
    }
    
    // プレイヤーを追跡
    private void MoveTowardsPlayer()
    {
        if (enemyData != null && !enemyData.canMove) return;
        if (Player.Instance == null) return;
        if (!gameObject.activeInHierarchy) return; // 非アクティブの場合は処理をスキップ
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        Vector2Int direction = NormalizeVector2Int(playerPos - gridPosition);
        Vector2Int newPos = gridPosition + direction;

        if (GridManager.Instance.IsInsideGrid(newPos) && !GridManager.Instance.IsOccupied(newPos))
        {
            Vector2Int oldPos = gridPosition;
            gridPosition = newPos;
            
            // 移動演出を実行（アクティブな場合のみ）
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateMove(oldPos, newPos));
            }
            else
            {
                // 非アクティブの場合は即座に位置を更新
                transform.position = GridManager.Instance.GetWorldPosition(newPos);
            }
            
            hasMovedThisTurn = true;
            
            if (Player.Instance != null && GridManager.Instance != null)
            {
                GridManager.Instance.UpdateTileVisibility(Player.Instance.gridPosition);
            }
        }
        else
        {
            // 追跡移動が失敗した場合、ランダム移動を試行
            MoveRandomly();
        }
    }
    
    // Vector2Intを正規化するヘルパーメソッド
    private Vector2Int NormalizeVector2Int(Vector2Int vector)
    {
        if (vector.x == 0 && vector.y == 0) return Vector2Int.zero;
        
        // 最も大きな成分を1に正規化
        int maxComponent = Mathf.Max(Mathf.Abs(vector.x), Mathf.Abs(vector.y));
        return new Vector2Int(
            Mathf.RoundToInt((float)vector.x / maxComponent),
            Mathf.RoundToInt((float)vector.y / maxComponent)
        );
    }
    
    // パトロール移動
    private void MoveInPatrol()
    {
        if (enemyData != null && !enemyData.canMove) return;
        MoveRandomly();
    }
    
    // グリッドベースの距離計算（チェビシェフ距離）
    private int GetGridDistance(Vector2Int pos1, Vector2Int pos2)
    {
        return Mathf.Max(Mathf.Abs(pos1.x - pos2.x), Mathf.Abs(pos1.y - pos2.y));
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
        
        // スライドしながら移動するアニメーション
        float elapsed = 0f;
        
        while (elapsed < moveAnimationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveAnimationDuration;
            float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad
            
            transform.position = Vector3.Lerp(startPos, endPos, easeT);
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.1f, easeT);
            
            yield return null;
        }
        
        // 最終位置を確実に設定
        transform.position = endPos;
        spriteRenderer.transform.localScale = Vector3.one;
    }
    
    // 近接攻撃
    private void TryAttackPlayerMelee()
    {
        if (Player.Instance == null) return;
        if (!gameObject.activeInHierarchy) return; // 非アクティブの場合は処理をスキップ
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = 1;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower : 1;
            string enemyName = enemyData != null ? enemyData.enemyName : "敵";
            
            // 攻撃演出を実行（アクティブな場合のみ）
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateAttack(playerPos, damage));
            }
            
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 遠距離攻撃
    private void TryAttackPlayerRanged()
    {
        if (Player.Instance == null) return;
        if (!gameObject.activeInHierarchy) return; // 非アクティブの場合は処理をスキップ
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = enemyData != null ? enemyData.rangedAttackRange : 2;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower : 1;
            string enemyName = enemyData != null ? enemyData.enemyName : "敵";
            
            // 攻撃演出を実行（アクティブな場合のみ）
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateRangedAttack(playerPos, damage));
            }
            
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 範囲攻撃
    private void TryAttackPlayerArea()
    {
        if (Player.Instance == null) return;
        if (!gameObject.activeInHierarchy) return; // 非アクティブの場合は処理をスキップ
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = 2;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower : 1;
            string enemyName = enemyData != null ? enemyData.enemyName : "敵";
            
            // 攻撃演出を実行（アクティブな場合のみ）
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateAreaAttack(playerPos, damage));
            }
            
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 特殊攻撃
    private void TryAttackPlayerSpecial()
    {
        if (Player.Instance == null) return;
        if (!gameObject.activeInHierarchy) return; // 非アクティブの場合は処理をスキップ
        
        Vector2Int playerPos = Player.Instance.gridPosition;
        int distance = GetGridDistance(playerPos, gridPosition);
        int attackRange = 1;
        
        if (distance <= attackRange)
        {
            int damage = enemyData != null ? enemyData.attackPower * 2 : 2;
            string enemyName = enemyData != null ? enemyData.enemyName : "敵";
            
            // 攻撃演出を実行（アクティブな場合のみ）
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(AnimateSpecialAttack(playerPos, damage));
            }
            
            Player.Instance.TakeDamage(damage);
        }
    }
    
    // 近接攻撃演出
    private IEnumerator AnimateAttack(Vector2Int targetPos, int damage)
    {
        Vector3 targetWorldPos = GridManager.Instance.GetWorldPosition(targetPos);
        Vector3 direction = (targetWorldPos - transform.position).normalized;
        
        // 攻撃方向に向かって少し移動するアニメーション
        Vector3 attackOffset = direction * 0.2f;
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
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.2f, easeT);
            
            yield return null;
        }
        
        // 攻撃エフェクトを再生
        if (attackEffect != null)
        {
            attackEffect.transform.position = originalPos + attackOffset;
            attackEffect.Play();
        }
        
        // 攻撃エフェクトを生成
        CreateAttackEffect(targetWorldPos, Color.red);
        
        // 後半：元の位置へ戻る
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t; // EaseInQuad
            
            transform.position = Vector3.Lerp(attackPos, originalPos, easeT);
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one * 1.2f, Vector3.one, easeT);
            
            yield return null;
        }
        
        // 最終位置を確実に設定
        transform.position = originalPos;
        spriteRenderer.transform.localScale = Vector3.one;
    }
    
    // 遠距離攻撃演出
    private IEnumerator AnimateRangedAttack(Vector2Int targetPos, int damage)
    {
        Vector3 targetWorldPos = GridManager.Instance.GetWorldPosition(targetPos);
        
        // 遠距離攻撃エフェクトを生成
        CreateRangedAttackEffect(transform.position, targetWorldPos);
        
        // 攻撃中はスプライトを少し大きくする
        float elapsed = 0f;
        float halfDuration = attackAnimationDuration / 2f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad
            
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.3f, easeT);
            
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t; // EaseInQuad
            
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one * 1.3f, Vector3.one, easeT);
            
            yield return null;
        }
        
        spriteRenderer.transform.localScale = Vector3.one;
    }
    
    // 範囲攻撃演出
    private IEnumerator AnimateAreaAttack(Vector2Int targetPos, int damage)
    {
        Vector3 targetWorldPos = GridManager.Instance.GetWorldPosition(targetPos);
        
        // 範囲攻撃エフェクトを生成
        CreateAreaAttackEffect(targetWorldPos);
        
        // 攻撃中はスプライトを少し大きくする
        float elapsed = 0f;
        float halfDuration = attackAnimationDuration / 2f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad
            
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.4f, easeT);
            
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t; // EaseInQuad
            
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one * 1.4f, Vector3.one, easeT);
            
            yield return null;
        }
        
        spriteRenderer.transform.localScale = Vector3.one;
    }
    
    // 特殊攻撃演出
    private IEnumerator AnimateSpecialAttack(Vector2Int targetPos, int damage)
    {
        Vector3 targetWorldPos = GridManager.Instance.GetWorldPosition(targetPos);
        
        // 特殊攻撃エフェクトを生成
        CreateSpecialAttackEffect(targetWorldPos);
        
        // 攻撃中はスプライトを大きくする
        float elapsed = 0f;
        float halfDuration = attackAnimationDuration / 2f;
        
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = 1f - (1f - t) * (1f - t); // EaseOutQuad
            
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 1.5f, easeT);
            spriteRenderer.color = Color.Lerp(Color.white, Color.yellow, easeT);
            
            yield return null;
        }
        
        elapsed = 0f;
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float easeT = t * t; // EaseInQuad
            
            spriteRenderer.transform.localScale = Vector3.Lerp(Vector3.one * 1.5f, Vector3.one, easeT);
            spriteRenderer.color = Color.Lerp(Color.yellow, Color.white, easeT);
            
            yield return null;
        }
        
        spriteRenderer.transform.localScale = Vector3.one;
        spriteRenderer.color = Color.white;
    }
    
    // 攻撃エフェクト生成
    private void CreateAttackEffect(Vector3 position, Color color)
    {
        GameObject effect = new GameObject("EnemyAttackEffect");
        effect.transform.position = position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 1.5f;
        main.startSize = 0.2f;
        main.maxParticles = 15;
        main.startColor = color;
        
        var emission = particles.emission;
        emission.rateOverTime = 25;
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;
        
        Destroy(effect, 0.5f);
    }
    
    // 遠距離攻撃エフェクト生成
    private void CreateRangedAttackEffect(Vector3 startPos, Vector3 endPos)
    {
        GameObject effect = new GameObject("RangedAttackEffect");
        effect.transform.position = startPos;
        
        // 弾丸エフェクト
        GameObject bullet = new GameObject("Bullet");
        bullet.transform.position = startPos;
        bullet.transform.SetParent(effect.transform);
        
        // 弾丸の移動
        StartCoroutine(MoveBullet(bullet.transform, startPos, endPos, 0.3f));
        
        // 弾丸のパーティクル
        ParticleSystem bulletParticles = bullet.AddComponent<ParticleSystem>();
        var main = bulletParticles.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 0f;
        main.startSize = 0.1f;
        main.maxParticles = 5;
        main.startColor = Color.blue;
        
        var emission = bulletParticles.emission;
        emission.rateOverTime = 10;
        
        Destroy(effect, 0.5f);
    }
    
    // 弾丸移動コルーチン
    private IEnumerator MoveBullet(Transform bullet, Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            bullet.position = Vector3.Lerp(startPos, endPos, t);
            
            yield return null;
        }
        
        bullet.position = endPos;
        CreateAttackEffect(endPos, Color.blue);
    }
    
    // 範囲攻撃エフェクト生成
    private void CreateAreaAttackEffect(Vector3 position)
    {
        GameObject effect = new GameObject("AreaAttackEffect");
        effect.transform.position = position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 1f;
        main.startSpeed = 3f;
        main.startSize = 0.3f;
        main.maxParticles = 30;
        main.startColor = new Color(1f, 0.5f, 0f, 1f); // Orange
        
        var emission = particles.emission;
        emission.rateOverTime = 40;
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 1f;
        
        // 範囲攻撃は円形に広がる
        var velocityOverLifetime = particles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.space = ParticleSystemSimulationSpace.World;
        velocityOverLifetime.radial = new ParticleSystem.MinMaxCurve(2f);
        
        Destroy(effect, 1f);
    }
    
    // 特殊攻撃エフェクト生成
    private void CreateSpecialAttackEffect(Vector3 position)
    {
        GameObject effect = new GameObject("SpecialAttackEffect");
        effect.transform.position = position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startLifetime = 0.8f;
        main.startSpeed = 2f;
        main.startSize = 0.4f;
        main.maxParticles = 25;
        main.startColor = new Color(0.5f, 0f, 0.5f, 1f); // Purple
        
        var emission = particles.emission;
        emission.rateOverTime = 30;
        
        var shape = particles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.6f;
        
        // 特殊攻撃は渦巻き状
        var rotationOverLifetime = particles.rotationOverLifetime;
        rotationOverLifetime.enabled = true;
        rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(360f);
        
        Destroy(effect, 0.8f);
    }

    public void TakeDamage(int damage)
    {
        int oldHP = currentHP;
        currentHP -= damage;
        currentHP = Mathf.Clamp(currentHP, 0, maxHP);
        int actualDamage = oldHP - currentHP;
        
        string enemyName = enemyData != null ? enemyData.enemyName : "敵";
        
        // ダメージ演出を実行（アクティブな場合のみ）
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(AnimateDamage());
        }
        
        // UI更新
        if (UIManager.Instance != null)
        {
            UIManager.Instance.AddLog($"{enemyName}が{actualDamage}ダメージを受けた！HP: {currentHP}/{maxHP}");
        }
        
        if (IsDead)
        {
            Die();
        }
    }
    
    // ダメージ演出
    private IEnumerator AnimateDamage()
    {
        // ダメージを受けた時は赤く光る
        float elapsed = 0f;
        float duration = 0.2f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color targetColor = Color.Lerp(Color.white, Color.red, Mathf.Sin(t * Mathf.PI * 2));
            spriteRenderer.color = targetColor;
            
            yield return null;
        }
        
        spriteRenderer.color = Color.white;
        
        // ダメージエフェクトを生成
        GameObject damageEffect = new GameObject("DamageEffect");
        damageEffect.transform.position = transform.position;
        
        ParticleSystem damageParticles = damageEffect.AddComponent<ParticleSystem>();
        var main = damageParticles.main;
        main.startLifetime = 0.3f;
        main.startSpeed = 2f;
        main.startSize = 0.1f;
        main.maxParticles = 10;
        main.startColor = Color.red;
        
        var emission = damageParticles.emission;
        emission.rateOverTime = 20;
        
        var shape = damageParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        Destroy(damageEffect, 0.3f);
    }

    protected override void Die()
    {
        string enemyName = enemyData != null ? enemyData.enemyName : "敵";
        
        // 死亡演出を実行（アクティブな場合のみ）
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(AnimateDeath());
        }
        
        // 効果音再生
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound("Death");
        }
        
        // スコア加算
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyDefeated();
        }
        
        Destroy(gameObject);
    }
    
    // 死亡演出
    private IEnumerator AnimateDeath()
    {
        // フェードアウト
        float elapsed = 0f;
        float duration = 0.8f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            Color color = spriteRenderer.color;
            color.a = 1f - t;
            spriteRenderer.color = color;
            
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.3f, t);
            
            yield return null;
        }
        
        // 死亡エフェクトを生成
        GameObject deathEffect = new GameObject("EnemyDeathEffect");
        deathEffect.transform.position = transform.position;
        
        ParticleSystem deathParticles = deathEffect.AddComponent<ParticleSystem>();
        var main = deathParticles.main;
        main.startLifetime = 1.5f;
        main.startSpeed = 1.5f;
        main.startSize = 0.2f;
        main.maxParticles = 30;
        main.startColor = Color.black;
        
        var emission = deathParticles.emission;
        emission.rateOverTime = 30;
        
        var shape = deathParticles.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.8f;
        
        Destroy(deathEffect, 1.5f);
    }
}