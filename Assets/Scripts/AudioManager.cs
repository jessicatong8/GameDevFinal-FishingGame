using UnityEngine;

public class FishingAudioManager : MonoBehaviour
{     
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private AudioSource castWhooshSound;  
    [SerializeField] private AudioSource waterPlopSound;
    [SerializeField] private AudioSource fishBiteSound;
    [SerializeField] private AudioSource hookedFishSound;  
    [SerializeField] private AudioSource reelingSound;
    [SerializeField] private AudioSource caughtFishSound;
    [SerializeField] private AudioSource escapedFishSound;
    //[SerializeField] private AudioSource failBiteSound;    
    //maybe add tense music 


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
        
        // maybe if we can figure out more accurate reeling sound
        //PlayerInput ReelStarted += StartReelingSound;
        //PlayerInput ReelStopped += StopReelingSound;
    }

    private void OnDisable()
    {
        FishingManager.OnCast -= PlayCastSequence;
        FishingManager.OnBite -= PlayBiteSound;
        FishingManager.OnHook -= PlayHookSound;
        FishingManager.OnCaught -= PlayCatchSound;
        FishingManager.OnEscaped -= PlayEscapeSound;

        //PlayerInput ReelStarted -= StartReelingSound;
        //PlayerInput ReelStopped -= StopReelingSound;
    }

    private void PlayCastSequence()
    {
        castWhooshSound.Play();
        // delay the plop slightly or trigger it via a separate timer/event if you want more realism
        Invoke(nameof(PlayPlopSound), 0.5f); 
        //calls method in specified amount of time
    }

    private void PlayPlopSound()
    {
        waterPlopSound.Play();
    }

    private void PlayBiteSound()
    {
        fishBiteSound.Play();
        //maybe play tense music??
    }

    private void PlayHookSound()
    {
        hookedFishSound.Play();
    }

    private void StartReelingSound()
    {
        if (FishingManager.Instance.CurrentFishingGameState == FishingManager.FishingGameState.Reeling)
        {
            if (!reelingSound.isPlaying)
            {
                reelingSound.Play();
            }
        }
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
}