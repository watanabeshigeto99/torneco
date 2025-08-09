using UnityEngine;

public class HitStopManager : MonoBehaviour
{
    // シーンに1つ置く。なければ自動生成
    static HitStopManager _inst;
    float _remain;                 // 残り停止時間（リアルタイム）
    float _originalScale = 1f;
    const float SlowScale = 0f;    // 完全停止（必要なら 0.05f などに）

    void Awake()
    {
        if (_inst && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        _originalScale = Time.timeScale;
    }

    void OnDestroy()
    {
        if (_inst == this) { _inst = null; Time.timeScale = _originalScale; }
    }

    void Update()
    {
        if (_remain > 0f)
        {
            _remain -= Time.unscaledDeltaTime;
            if (Time.timeScale != SlowScale) Time.timeScale = SlowScale;
            if (_remain <= 0f) Time.timeScale = _originalScale;
        }
    }

    public static void Request(float duration)
    {
        if (!_inst)
        {
            var go = new GameObject("HitStopManager");
            _inst = go.AddComponent<HitStopManager>();
            DontDestroyOnLoad(go);
        }
        // 残り時間に加算（複数ヒットも自然に延長）
        _inst._remain = Mathf.Max(_inst._remain, 0f) + duration;
    }
}
