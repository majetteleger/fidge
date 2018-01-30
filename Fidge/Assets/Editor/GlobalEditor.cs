using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GlobalEditor : MonoBehaviour
{
    [MenuItem("Tools/Reset progress")]
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    [MenuItem("Tools/Unlock all levels")]
    public static void UnlockAllLevels()
    {
        MainManager mainManager =
            (MainManager) AssetDatabase.LoadAssetAtPath("Assets/Prefabs/GameManager.prefab", typeof(MainManager));

        for (var i = 0; i < mainManager.Levels.Length; i++)
        {
            PlayerPrefs.SetString("Level" + i, "111");
        }
    }

    [MenuItem("Tools/Rewrite")]
    public static void Rewrite()
    {
        const string rootfolder = @"C:\Users\ma_je\OneDrive\Documents\BitBucket\WorkshopShmup\Fidge\Fidge\Assets\Levels\10-Julian\0-Basic";

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
                        lines[i] = lines[i].Replace(@"-TSR", @"(TSR)");
                        lines[i] = lines[i].Replace(@"-TSC", @"(TSC)");
                        lines[i] = lines[i].Replace(@"-CY", @"(CY)");
                        lines[i] = lines[i].Replace(@"-CM", @"(CM)");
                        lines[i] = lines[i].Replace(@"-CC", @"(CC)");
                        lines[i] = lines[i].Replace(@"-CB", @"(CB)");
                        lines[i] = lines[i].Replace(@"-CG", @"(CG)");
                        lines[i] = lines[i].Replace(@"-CR", @"(CR)");
                        lines[i] = lines[i].Replace(@"-DL", @"(DL)");
                        lines[i] = lines[i].Replace(@"-DD", @"(DD)");
                        lines[i] = lines[i].Replace(@"-DR", @"(DR)");
                        lines[i] = lines[i].Replace(@"-DU", @"(DU)");
                        lines[i] = lines[i].Replace(@"-LK", @"(LK)");
                        lines[i] = lines[i].Replace(@"-S", @"(S)");
                        lines[i] = lines[i].Replace(@"-C", @"(C)");
                        lines[i] = lines[i].Replace(@"-W", @"(W)");
                        lines[i] = lines[i].Replace(@"-L", @"(L)");
                        lines[i] = lines[i].Replace(@"-K", @"(K)");
                        lines[i] = lines[i].Replace(@"-V", @"(V)");
                        lines[i] = lines[i].Replace(@"-H", @"(H)");
                        lines[i] = lines[i].Replace(@"-P", @"(P)");
                        lines[i] = lines[i].Replace(@"-N", @"(N)");
                        lines[i] = lines[i].Replace(@"-F", @"(F)");
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
