using UnityEngine;

public class DamageVFXTest : MonoBehaviour
{
    [Header("Test Settings")]
    public KeyCode testKey = KeyCode.T;
    public int testDamage = 5;
    public Vector2 testHitDirection = Vector2.right;

    private DamageVFX damageVFX;

    void Start()
    {
        damageVFX = GetComponent<DamageVFX>();
        if (damageVFX == null)
        {
            Debug.LogWarning("DamageVFXTest: DamageVFX component not found on this GameObject");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(testKey))
        {
            TestDamageVFX();
        }
    }

    void TestDamageVFX()
    {
        if (damageVFX != null)
        {
            Debug.Log($"DamageVFXTest: Testing damage VFX with direction {testHitDirection}");
            damageVFX.Play(testHitDirection);
        }
        else
        {
            Debug.LogError("DamageVFXTest: DamageVFX component not found");
        }
    }

    void OnGUI()
    {
        if (damageVFX != null)
        {
            GUI.Label(new Rect(10, 10, 300, 20), $"Press {testKey} to test DamageVFX");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 300, 20), "DamageVFX component not found!");
        }
    }
}
