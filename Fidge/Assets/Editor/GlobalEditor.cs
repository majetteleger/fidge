using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GlobalEditor : MonoBehaviour
{
    /*[MenuItem("Tools/Reset progress")]
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }*/

    /*[MenuItem("Tools/Unlock all levels")]
    public static void UnlockAllLevels()
    {
        MainManager mainManager = (MainManager) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/GameManager.prefab", typeof(MainManager));

        for (var i = 0; i < mainManager.Levels.Length; i++)
        {
            PlayerPrefs.SetString("Level" + i, "111");
        }
    }*/

    [MenuItem("Tools/Rewrite")]
    public static void Rewrite()
    {
        const string rootfolder = @"C:\Users\M-Antoine\Documents\SharedProjects\Fidge\Fidge\Assets\Levels";

        var files = Directory.GetFiles(rootfolder, "*.asset*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            try
            {
                File.SetAttributes(file, FileAttributes.Normal);
                var lines = File.ReadAllLines(file);
                
                for (var i = 0; i < lines.Length; i++)
                {
                    if (i > 20)
                    {
                        lines[i] = lines[i].Replace(@"(CY)", @"(CO)");
                        lines[i] = lines[i].Replace(@"(CM)", @"(CP)");
                        lines[i] = lines[i].Replace(@"(CC)", @"(CO)");
                    }
                }
                
                File.WriteAllLines(file, lines);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }
}
