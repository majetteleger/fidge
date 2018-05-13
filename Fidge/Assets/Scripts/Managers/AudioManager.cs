using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; }

    public AudioSource BackgroundMusic;
    public AudioSource[] SoundEffectSources;

    public AudioClip MenuMusic;
    public AudioClip LevelMusic;
    public AudioClip MenuClick;
    public AudioClip InputDirection;
    public AudioClip InputGo;
    public AudioClip PlayerMove;
    public AudioClip LevelFailed;
    public AudioClip LevelCleared;
    public AudioClip Slide;
    public AudioClip Crack;
    public AudioClip Wall;
    public AudioClip Lock;
    public AudioClip GetKey;
    public AudioClip GetStar;
    public AudioClip TeleportLink;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        PlayBackgroundMusic(MenuMusic);
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (music == null)
        {
            Debug.Log("Null music reference");
            return;
        }

        if (BackgroundMusic.clip == music)
        {
            return;
        }

        BackgroundMusic.clip = music;
        BackgroundMusic.Play();
    }

    public void PlaySoundEffect(AudioClip effect)
    {
        if (effect == null)
        {
            Debug.Log("Null sound effect reference");
            return;
        }

        foreach (var soundEffectSource in SoundEffectSources)
        {
            if (!soundEffectSource.isPlaying)
            {
                soundEffectSource.clip = effect;
                soundEffectSource.Play();

                break;
            }
        }
    }
}
