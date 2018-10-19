using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsPanel : Panel
{
    [Serializable]
    public class CreditsSection
    {
        public string Subtitle;
        public string[] Entries;
    }

    public GameObject SubtitlePrefab;
    public GameObject EntryPrefab;
    public Transform Content;
    public CreditsSection[] Sections;
    
    void Start()
    {
        LoadEntries();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());

        SetupSounds();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UIManager.Instance.MainMenuPanel.Show();
        }
    }
    
    public void LoadEntries()
    {
        for (var i = 0; i < Sections.Length; i++)
        {
            var newSubtitle = Instantiate(SubtitlePrefab, Content);
            newSubtitle.GetComponentInChildren<Text>().text = Sections[i].Subtitle;

            for (var j = 0; j < Sections[i].Entries.Length; j++)
            {
                var newEntry = Instantiate(EntryPrefab, Content);
                newEntry.GetComponentInChildren<Text>().text = Sections[i].Entries[j];
            }
        } 
    }

    public void UI_Back()
    {
        UIManager.Instance.MainMenuPanel.Show();
    }
}
