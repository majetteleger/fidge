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

    public static CreditsPanel instance = null;

    public GameObject SubtitlePrefab;
    public GameObject EntryPrefab;
    public Transform Content;
    public CreditsSection[] Sections;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        LoadEntries();
        LayoutRebuilder.ForceRebuildLayoutImmediate(Content.GetComponent<RectTransform>());

        Initialize();
    }

    void Update()
    {
        if (!IsActive)
        {
            if (!IsActive)
            {
                return;
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            MainMenuPanel.instance.Show();
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
        MainMenuPanel.instance.Show();
    }
}
