﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace Hero.SpellEditor
{
    public class SpellEditorUtilities
    {
        public static string currentPath = "";


        public static bool CheckIfAssetExists(string assetName, string directory)
        {   
            #if UNITY_EDITOR

            var assets = AssetDatabase.FindAssets(assetName);

            if (assets.Length > 0)
            {
                foreach (var asset in assets)
                {
                    string targetPath = AssetDatabase.GUIDToAssetPath(asset);

                    if (targetPath.Replace(assetName + ".asset", "").Equals(directory))
                    {
                        return true;
                    }
                }
            }

            #endif

            return false;
        }


        public static void CreateAsset(Object asset, string path, string fileName)
        {
            #if UNITY_EDITOR

            Debug.Log(path);
            if (!AssetDatabase.IsValidFolder(path))
            {
                Debug.Log("Making Path");

                string folderName = path.Substring(path.LastIndexOf("/")+1);
                string newPath = path.Substring(0, path.LastIndexOf("/"));
                AssetDatabase.CreateFolder(newPath, folderName);
            }
            
            AssetDatabase.CreateAsset(asset, path + "/" + fileName + ".asset");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            #endif
        }


        public static void CreateTextFile(string path)
        {
            #if UNITY_EDITOR

            if (!File.Exists(path))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(path + ".txt"))
                {
                    sw.WriteLine("1|TerminateAnimation");
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            #endif
        }


        public static string GetAssetPath(Object asset)
        {
            string result = "";

            #if UNITY_EDITOR
            
            result = AssetDatabase.GetAssetPath(asset);

            #endif

            return result;
        }


        public static bool DoesAssetExist(Object asset)
        {
            bool result = false;

            #if UNITY_EDITOR

            result = AssetDatabase.Contains(asset);

            #endif

            return result;
        }

        
        public static void CleanUp()
        {
            #if UNITY_EDITOR

            var assets = AssetDatabase.FindAssets("t:Spell");

            foreach(var asset in assets)
            {
                Spell s = (Spell)AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(asset), typeof(Spell));

                if (s.spellFamily != null && !AssetDatabase.Contains(s.spellFamily))
                    s.spellFamily = null;

                if (s.spellAnimation != null && !AssetDatabase.Contains(s.spellAnimation))
                    s.spellAnimation = null;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            #endif
        }
    }
}
