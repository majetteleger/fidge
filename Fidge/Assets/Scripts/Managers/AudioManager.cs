using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; }

    public AudioSource BackgroundMusic;
    public AudioSource SoundEffect;

    public AudioClip Music;
    public AudioClip Good;
    public AudioClip Bad;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        PlayBackgroundMusic(Music);
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        BackgroundMusic.clip = music;
        BackgroundMusic.Play();
    }

    public void PlaySoundEffect(AudioClip effect)
    {
        SoundEffect.clip = effect;
        SoundEffect.Play();
    }
}
