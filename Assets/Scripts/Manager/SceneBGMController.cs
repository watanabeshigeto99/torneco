using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneBGMController : MonoBehaviour
{
    private void Awake()
    {
        // シーン遷移イベントを登録
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDestroy()
    {
        // イベントを解除
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// シーンが読み込まれた時の処理
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"SceneBGMController: シーン '{scene.name}' が読み込まれました");
        
        // 少し待ってからBGMを切り替え（シーンの初期化を待つ）
        Invoke(nameof(PlayBGMForCurrentScene), 0.2f);
    }
    
    /// <summary>
    /// 現在のシーンに応じたBGMを再生
    /// </summary>
    private void PlayBGMForCurrentScene()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayBGMForCurrentScene();
        }
        else
        {
            Debug.LogWarning("SceneBGMController: SoundManagerが見つかりません");
        }
    }
} 