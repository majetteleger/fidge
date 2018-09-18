using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsPanel : Panel
{
    public static OptionsPanel instance = null;

    public GameObject ResetConfirmationPopup;
    public Button PayButton;
    public Image MusicImage;
    public Image SoundImage;
    public Slider MusicSlider;
    public Slider SoundSlider;
    public Sprite MusicOnSprite;
    public Sprite MusicOffSprite;
    public Sprite SoundOnSprite;
    public Sprite SoundOffSprite;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        SetupSounds();
    }
    
    void Update()
    {
        if(!IsActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenuPanel.instance.Show();
        }
    }

    public override void Show(Panel originPanel = null)
    {
        UpdateButtonSprite(MusicImage, MusicOnSprite, MusicOffSprite, AudioManager.Instance.MusicOn);
        UpdateButtonSprite(SoundImage, SoundOnSprite, SoundOffSprite, AudioManager.Instance.SoundOn);

        MusicSlider.value = PlayerPrefs.GetFloat("MusicVolume");
        SoundSlider.value = PlayerPrefs.GetFloat("SoundVolume");

        UpdatePayButton();

        base.Show(originPanel);
    }

    public void UpdatePayButton()
    {
        PayButton.interactable = !MainManager.Instance.Paid;
    }

    private void ResetProgress()
    {
        for (var i = 0; i < MainManager.Instance.Levels.Length; i++)
        {
            PlayerPrefs.SetString("Level" + i, "000");
        }

        PlayerPrefs.Save();

        MainManager.Instance.DirtyMedals = true;
    }

    public override void Hide()
    {
        ResetConfirmationPopup.SetActive(false);

        base.Hide();
    }

    public void UI_OpenResetConfirmation()
    {
        ResetConfirmationPopup.SetActive(true);
        ForceLayoutRebuilding(ResetConfirmationPopup.GetComponent<RectTransform>());
    }

    public void UI_CancelReset()
    {
        ResetConfirmationPopup.SetActive(false);
    }

    public void UI_ConfirmReset()
    {
        ResetProgress();
        ResetConfirmationPopup.SetActive(false);
    }
    
    public void UI_ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
        MusicImage.sprite = AudioManager.Instance.MusicOn ? MusicOnSprite : MusicOffSprite;
    }

    public void UI_ToggleSound()
    {
        AudioManager.Instance.ToggleSound();
        SoundImage.sprite = AudioManager.Instance.SoundOn ? SoundOnSprite : SoundOffSprite;
    }

    public void UI_AdjustMusic()
    {
        AudioManager.Instance.AdjustMusic(MusicSlider.value);
    }

    public void UI_AdjustSound()
    {
        AudioManager.Instance.AdjustSound(SoundSlider.value);
    }
    
    public void UI_SaveMusicValue()
    {
        AudioManager.Instance.SaveVolumePref("MusicVolume", MusicSlider.value);
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.MenuClick);
    }

    public void UI_SaveSoundValue()
    {
        AudioManager.Instance.SaveVolumePref("SoundVolume", SoundSlider.value);
        AudioManager.Instance.PlaySoundEffect(AudioManager.Instance.MenuClick);
    }
    
    public void UI_UnlockAllLevels()
    {
        for (var i = 0; i < MainManager.Instance.Levels.Length; i++)
        {
            PlayerPrefs.SetString("Level" + i, "111");
        }

        PlayerPrefs.Save();

        MainManager.Instance.DirtyMedals = true;
    }

    public void UI_Pay()
    {
        PurchasePanel.instance.Show();
    }

    public void UI_ResetPay()
    {
        MainManager.Instance.Paid = false;
        LevelSelectionPanel.Instance.UpdateSectionBlockers();
        UpdatePayButton();
    }

    public void UI_Back()
    {
        MainMenuPanel.instance.Show();
    }
}
