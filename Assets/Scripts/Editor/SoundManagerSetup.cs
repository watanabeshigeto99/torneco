using UnityEngine;
using UnityEditor;

public class SoundManagerSetup : EditorWindow
{
    [MenuItem("Tools/Setup SoundManager")]
    public static void SetupSoundManager()
    {
        // SoundManagerを探す
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogError("SoundManager not found in the scene!");
            return;
        }
        
        // 音声ファイルを自動設定
        AssignAudioClips(soundManager);
        
        Debug.Log("SoundManager setup completed successfully!");
    }
    
    private static void AssignAudioClips(SoundManager soundManager)
    {
        // BGMの設定
        if (soundManager.bgmMain == null)
        {
            soundManager.bgmMain = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/世界破滅.mp3");
            Debug.Log("BGM Main assigned");
        }
        
        if (soundManager.bgmDeckBuilder == null)
        {
            soundManager.bgmDeckBuilder = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/街の商人.mp3");
            Debug.Log("BGM DeckBuilder assigned");
        }
        
        if (soundManager.bgmBattle == null)
        {
            soundManager.bgmBattle = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/世界破滅.mp3");
            Debug.Log("BGM Battle assigned");
        }
        
        if (soundManager.bgmGameOver == null)
        {
            soundManager.bgmGameOver = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/破壊.mp3");
            Debug.Log("BGM GameOver assigned");
        }
        
        if (soundManager.bgmGameClear == null)
        {
            soundManager.bgmGameClear = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/BGM/世界破滅2.mp3");
            Debug.Log("BGM GameClear assigned");
        }
        
        // SEの設定
        if (soundManager.attackSE == null)
        {
            soundManager.attackSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/刀で斬る2.mp3");
            Debug.Log("Attack SE assigned");
        }
        
        if (soundManager.healSE == null)
        {
            soundManager.healSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/ステータス治療1.mp3");
            Debug.Log("Heal SE assigned");
        }
        
        if (soundManager.selectSE == null)
        {
            soundManager.selectSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/ButtonClick.mp3");
            Debug.Log("Select SE assigned");
        }
        
        if (soundManager.enemyDeathSE == null)
        {
            soundManager.enemyDeathSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/打撃3.mp3");
            Debug.Log("Enemy Death SE assigned");
        }
        
        if (soundManager.levelUpSE == null)
        {
            soundManager.levelUpSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/レベルアップ.mp3");
            Debug.Log("Level Up SE assigned");
        }
        
        if (soundManager.enhanceSE == null)
        {
            soundManager.enhanceSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/宝箱を開ける.mp3");
            Debug.Log("Enhance SE assigned");
        }
        
        // 追加SEの設定
        if (soundManager.buttonClickSE == null)
        {
            soundManager.buttonClickSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/ButtonClick.mp3");
            Debug.Log("Button Click SE assigned");
        }
        
        if (soundManager.cardHoverSE == null)
        {
            soundManager.cardHoverSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/card_hover.mp3");
            Debug.Log("Card Hover SE assigned");
        }
        
        if (soundManager.cardAddSE == null)
        {
            soundManager.cardAddSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/アイテムを入手2.mp3");
            Debug.Log("Card Add SE assigned");
        }
        
        if (soundManager.cardRemoveSE == null)
        {
            soundManager.cardRemoveSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/Error.mp3");
            Debug.Log("Card Remove SE assigned");
        }
        
        if (soundManager.errorSE == null)
        {
            soundManager.errorSE = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sounds/SE/Error.mp3");
            Debug.Log("Error SE assigned");
        }
    }
    
    [MenuItem("Tools/Verify SoundManager Setup")]
    public static void VerifySoundManagerSetup()
    {
        SoundManager soundManager = FindObjectOfType<SoundManager>();
        
        if (soundManager == null)
        {
            Debug.LogError("SoundManager not found!");
            return;
        }
        
        bool allAssigned = true;
        
        // BGMの確認
        if (soundManager.bgmMain == null)
        {
            Debug.LogError("BGM Main is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.bgmDeckBuilder == null)
        {
            Debug.LogError("BGM DeckBuilder is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.bgmBattle == null)
        {
            Debug.LogError("BGM Battle is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.bgmGameOver == null)
        {
            Debug.LogError("BGM GameOver is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.bgmGameClear == null)
        {
            Debug.LogError("BGM GameClear is not assigned!");
            allAssigned = false;
        }
        
        // 基本SEの確認
        if (soundManager.attackSE == null)
        {
            Debug.LogError("Attack SE is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.healSE == null)
        {
            Debug.LogError("Heal SE is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.selectSE == null)
        {
            Debug.LogError("Select SE is not assigned!");
            allAssigned = false;
        }
        
        // 追加SEの確認
        if (soundManager.buttonClickSE == null)
        {
            Debug.LogError("Button Click SE is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.cardHoverSE == null)
        {
            Debug.LogError("Card Hover SE is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.cardAddSE == null)
        {
            Debug.LogError("Card Add SE is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.cardRemoveSE == null)
        {
            Debug.LogError("Card Remove SE is not assigned!");
            allAssigned = false;
        }
        
        if (soundManager.errorSE == null)
        {
            Debug.LogError("Error SE is not assigned!");
            allAssigned = false;
        }
        
        if (allAssigned)
        {
            Debug.Log("All SoundManager audio clips are properly assigned!");
        }
    }
} 