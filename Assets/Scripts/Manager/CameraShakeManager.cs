using UnityEngine;

public class CameraShakeManager : MonoBehaviour
{
    static CameraShakeManager _inst;
    Transform _cam;
    Vector3 _baseLocalPos;
    float _remain;
    float _strength;  // 要求の最大値を優先

    void Awake()
    {
        if (_inst && _inst != this) { Destroy(gameObject); return; }
        _inst = this;
        _cam = Camera.main ? Camera.main.transform : null;
        if (_cam) _baseLocalPos = _cam.localPosition;
    }

    void LateUpdate()
    {
        if (_cam == null) return;

        if (_remain > 0f)
        {
            _remain -= Time.unscaledDeltaTime;
            float sx = (Random.value - 0.5f) * _strength;
            float sy = (Random.value - 0.5f) * _strength;
            _cam.localPosition = _baseLocalPos + new Vector3(sx, sy, 0f);

            if (_remain <= 0f)
            {
                _cam.localPosition = _baseLocalPos;
                _strength = 0f;
            }
        }
    }

    public static void Request(float time, float strength)
    {
        if (!_inst)
        {
            var go = new GameObject("CameraShakeManager");
            _inst = go.AddComponent<CameraShakeManager>();
            DontDestroyOnLoad(go);
        }
        // 同時要求は「長い方に延長」「強い方を採用」
        _inst._remain = Mathf.Max(_inst._remain, time);
        _inst._strength = Mathf.Max(_inst._strength, strength);
    }
}
