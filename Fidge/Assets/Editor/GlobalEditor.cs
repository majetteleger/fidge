using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class GlobalEditor : MonoBehaviour
{
    [MenuItem("Tools/Reset everything")]
    public static void ResetProgress()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    public static T[] GetAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);  //FindAssets uses tags check documentation for more info
        T[] a = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++)         //probably could get optimized 
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            a[i] = AssetDatabase.LoadAssetAtPath<T>(path);
        }

        return a;

    }

    [MenuItem("Tools/Save All Levels")]
    public static void SaveAllLevels()
    {
        var editableLevels = GetAllInstances<EditableLevel>();

        foreach (var editableLevel in editableLevels)
        {
            LevelEditor.ComputeSolution(editableLevel);
        }
    }
    
    /*[MenuItem("Tools/Rewrite")]
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
    }*/
}
