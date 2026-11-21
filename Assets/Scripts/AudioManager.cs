using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource sfxSource;

    [Header("Applause")]
    public AudioClip[] applauseClips;

    [Header("Question")]
    public AudioClip questionMurmurClip;

    [Header("Countdown")]
    public AudioClip countdownClip;
    public AudioClip startClip;
    [Header("Question Voice")]
    public AudioClip babbleClip;
    public AudioSource babbleSource;

    

    public void PlayBabble()
    {
        
        if (babbleClip == null || babbleSource == null)
            return;

        babbleSource.pitch =
            Random.Range(1.2f, 1.8f);

        babbleSource.PlayOneShot(
            babbleClip,
            0.5f
        );
}



    public void PlayApplause()
    {
        if (applauseClips == null ||
            applauseClips.Length == 0)
            return;

        AudioClip clip =
            applauseClips[
                Random.Range(0, applauseClips.Length)
            ];

        sfxSource.PlayOneShot(clip);
    }


    public void PlayQuestionMurmur()
    {
        if (questionMurmurClip != null)
            sfxSource.PlayOneShot(questionMurmurClip);
    }


    public void PlayCountdown()
    {
        if (countdownClip != null)
            sfxSource.PlayOneShot(countdownClip);
    }


    public void PlayStart()
    {
        if (startClip != null)
            sfxSource.PlayOneShot(startClip);
    }
}