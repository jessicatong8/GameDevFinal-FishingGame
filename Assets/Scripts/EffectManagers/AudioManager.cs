using System;
using UnityEngine;

public class FishingAudioManager : MonoBehaviour
{     
    [Header("Audio")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource castWhooshSound;  
    [SerializeField] private AudioSource waterPlopSound;
    [SerializeField] private AudioSource fishBiteSound;
    [SerializeField] private AudioSource hookedFishSound;  
    [SerializeField] private AudioSource reelingSound;
    [SerializeField] private AudioSource caughtFishSound;
    [SerializeField] private AudioSource escapedFishSound;
    [SerializeField] private AudioSource levelUpSound;
    [SerializeField] private AudioSource winSound;

    [Header("Timing Specification")]
    [SerializeField] private float plopDelay = 0.8f;

    private void Start()
    {
        backgroundMusic.Play();
    }

    private void OnEnable()
    {
        FishingManager.OnCast += PlayCastSequence;
        FishingManager.OnBite += PlayBiteSound;
        FishingManager.OnHook += PlayHookSound;
        FishingManager.OnCaught += PlayCatchSound;
        FishingManager.OnEscaped += PlayEscapeSound;
        LevelManager.OnLevelUp += PlayLevelUpSound;
        LevelManager.OnGameWin += PlayWinSound; 
        PlayerInputState.MashPerformed += PlayReelingSound;
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= PlayCastSequence;
        FishingManager.OnBite -= PlayBiteSound;
        FishingManager.OnHook -= PlayHookSound;
        FishingManager.OnCaught -= PlayCatchSound;
        FishingManager.OnEscaped -= PlayEscapeSound;
        FishingManager.OnLevelUp -= PlayLevelUpSound; 
        FishingManager.OnGameWin -= PlayWinSound;
        PlayerInputState.MashPerformed -= PlayReelingSound;
    }

    private void PlayCastSequence()
    {
        castWhooshSound.Play();
        Invoke(nameof(PlayPlopSound), plopDelay); 
    }

    private void PlayPlopSound()
    {
        waterPlopSound.Play();
    }

    private void PlayBiteSound()
    {
        fishBiteSound.Play();
    }

    private void PlayHookSound()
    {
        hookedFishSound.Play();
    }

    private void PlayReelingSound()
    {
        reelingSound.Play();
    }

    private void StopReelingSound()
    {
        reelingSound.Stop();
    }

    private void PlayCatchSound()
    {
        StopReelingSound();
        caughtFishSound.Play();
    }

    private void PlayEscapeSound()
    {
        StopReelingSound();
        escapedFishSound.Play();
    }
    private void PlayLevelUpSound(int level)
    {
        levelUpSound.Play();
    }
    private void PlayWinSound()
    {
        winSound.Play();
    }
}