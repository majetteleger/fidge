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
    
    public bool MusicOn
    {
        get
        {
            return PlayerPrefs.GetInt("MusicOn") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("MusicOn", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    public bool SoundOn
    {
        get
        {
            return PlayerPrefs.GetInt("SoundOn") == 1;
        }
        set
        {
            PlayerPrefs.SetInt("SoundOn", value ? 1 : 0);
            PlayerPrefs.Save();
        }
    }

    private bool AudioFirstStart
    {
        get
        {
            return PlayerPrefs.GetInt("AudioFirstStart") == 0;
        }
        set
        {
            PlayerPrefs.SetInt("AudioFirstStart", value ? 0 : 1);
            PlayerPrefs.Save();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Start()
    {
        if (AudioFirstStart)
        {
            MusicOn = true;
            SoundOn = true;
            SaveVolumePref("MusicVolume", 1f);
            SaveVolumePref("SoundVolume", 1f);
        }

        BackgroundMusic.mute = !MusicOn;
        foreach (var soundEffectSource in SoundEffectSources)
        {
            soundEffectSource.mute = !SoundOn;
        }

        AdjustMusic(PlayerPrefs.GetFloat("MusicVolume"));
        AdjustSound(PlayerPrefs.GetFloat("SoundVolume"));

        PlayBackgroundMusic(MenuMusic);

        AudioFirstStart = false;
    }

    public void PlayBackgroundMusic(AudioClip music)
    {
        if (music == null || BackgroundMusic.clip == music)
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

    public void ToggleMusic()
    {
        var newValue = !MusicOn;

        BackgroundMusic.mute = !newValue;
        MusicOn = newValue;
    }

    public void ToggleSound()
    {
        var newValue = !SoundOn;

        foreach (var soundEffectSource in SoundEffectSources)
        {
            soundEffectSource.mute = !newValue;
        }
        
        SoundOn = newValue;
    }

    public void AdjustMusic(float volumeValue)
    {
        BackgroundMusic.volume = volumeValue;
    }

    public void AdjustSound(float volumeValue)
    {
        foreach (var soundEffectSource in SoundEffectSources)
        {
            soundEffectSource.volume = volumeValue;
        }
    }

    public void SaveVolumePref(string volumePref, float volumeValue)
    {
        PlayerPrefs.SetFloat(volumePref, volumeValue);
    }
}
