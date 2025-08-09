using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DamageVFX : MonoBehaviour
{
    [Header("Flash")]
    public SpriteRenderer targetSprite;             // 未設定なら自動取得
    public Color flashColor = new Color(1f, 0.2f, 0.2f);
    public float flashDuration = 0.12f;

    [Header("Knockback")]
    public float knockbackDistance = 0.12f;
    public float knockbackTime = 0.06f;

    [Header("Hit Stop (global)")]
    public float hitStop = 0.06f;

    [Header("Camera Shake (global)")]
    public float shakeTime = 0.12f;
    public float shakeStrength = 0.08f;

    [Header("Particles (optional)")]
    public ParticleSystem hitSpark;

    // --- cache ---
    Transform _tr;
    Unit _unit;
    Transform _cam;
    Vector3 _camStartLocal;
    bool _isPlaying;
    Coroutine _co;
    Color _baseColor = Color.white;
    static readonly int _ColorId = Shader.PropertyToID("_Color");
    MaterialPropertyBlock _mpb;             // 再利用してGC削減

    void Awake()
    {
        _tr = transform;
        if (!targetSprite) targetSprite = GetComponentInChildren<SpriteRenderer>();
        if (targetSprite) _baseColor = targetSprite.color;
        _unit = GetComponent<Unit>();

        var mainCam = Camera.main;
        if (mainCam) {
            _cam = mainCam.transform;
            _camStartLocal = _cam.localPosition;
        }

        _mpb = new MaterialPropertyBlock();
        if (targetSprite) targetSprite.GetPropertyBlock(_mpb);
    }

    public void Play(Vector2 hitDir)
    {
        if (_isPlaying) return; // 二重実行を抑制
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Co_Play(hitDir));
    }

    IEnumerator Co_Play(Vector2 hitDir)
    {
        _isPlaying = true;

        // 1) Global HitStop 要求（重ねがけはマネージャで解決）
        if (hitStop > 0f) HitStopManager.Request(hitStop);

        // 2) Flash（MPBで色補間）
        if (targetSprite && flashDuration > 0f)
            yield return Co_FlashMPB();

        // 3) Knockback（座標復帰まで一括）
        if (hitDir.sqrMagnitude > 0.0001f && knockbackTime > 0f && knockbackDistance != 0f)
            yield return Co_Knockback(hitDir.normalized);

        // 4) Camera Shake（グローバルに積み上げ）
        if (shakeTime > 0f && shakeStrength > 0f)
            CameraShakeManager.Request(shakeTime, shakeStrength);

        // 5) Particle（割り当て済みを再生、Instantiateなし）
        if (hitSpark)
        {
            hitSpark.transform.position = _tr.position;
            hitSpark.Play();
        }

        _isPlaying = false;
        _co = null;
    }

    IEnumerator Co_FlashMPB()
    {
        float t = 0f;
        // 三角波で行って戻る（unscaled）
        while (t < flashDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Abs(t / flashDuration * 2f - 1f);
            // _baseColor→flashColor の補間をMPB経由で
            Color c = Color.Lerp(_baseColor, flashColor, k);
            _mpb.SetColor(_ColorId, c);
            targetSprite.SetPropertyBlock(_mpb);
            yield return null;
        }
        // 戻す
        _mpb.SetColor(_ColorId, _baseColor);
        targetSprite.SetPropertyBlock(_mpb);
    }

    IEnumerator Co_Knockback(Vector2 dir)
    {
        Vector3 start = _tr.position;
        Vector3 end = start + (Vector3)(dir * knockbackDistance);

        float t = 0f;
        float dur = knockbackTime;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            _tr.position = Vector3.LerpUnclamped(start, end, p);
            yield return null;
        }
        // 復帰
        t = 0f;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            _tr.position = Vector3.LerpUnclamped(end, start, p);
            yield return null;
        }
        _tr.position = start;
    }
}
