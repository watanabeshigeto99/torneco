using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 追従対象（プレイヤー）
    public Vector3 offset = new Vector3(0, 0, -10); // カメラのオフセット
    public bool followX = true; // X軸追従
    public bool followY = true; // Y軸追従

    private void Start()
    {
        // プレイヤーを自動で見つける（遅延生成に対応）
        StartCoroutine(FindPlayerDelayed());
    }

    private System.Collections.IEnumerator FindPlayerDelayed()
    {
        // プレイヤーが生成されるまで待機
        while (target == null)
        {
            Player player = FindObjectOfType<Player>();
            if (player != null)
            {
                target = player.transform;
                Debug.Log("プレイヤーを発見！カメラ追従開始");
                
                // 初期位置に即座に移動
                OnPlayerMoved(player.transform.position);
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    // プレイヤーが移動した時に呼び出される
    public void OnPlayerMoved(Vector3 playerPosition)
    {
        Vector3 desiredPosition = playerPosition + offset;
        
        // 追従軸を制限
        if (!followX)
        {
            desiredPosition.x = transform.position.x;
        }
        if (!followY)
        {
            desiredPosition.y = transform.position.y;
        }

        // 即座に移動（ターン制ゲームなので）
        transform.position = desiredPosition;
        Debug.Log($"カメラ移動: {transform.position}");
    }
} 