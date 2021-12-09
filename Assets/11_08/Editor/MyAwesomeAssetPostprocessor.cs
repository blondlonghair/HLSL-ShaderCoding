using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _11_08.Editor
{
    public class MyAwesomeAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var importedAsset in importedAssets)
            {
                Debug.Log($"Added : {importedAsset}");

                if (importedAsset.Contains(" "))
                {
                    Debug.LogError("your file name is wrong please fix it");
                }

                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = "cmd";
                process.StartInfo.Arguments = $@"/c C:\Users\Owner\source\repos\FixMyGame\FixMyGame\bin\Debug\net5.0\FixMyGame.exe ""{importedAsset}""";
                process.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                process.Start();
            }

            foreach (var deletedAsset in deletedAssets)
            {
                Debug.Log($"Deleted : {deletedAsset}");
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                Debug.Log($"Moved {movedAssets[i]} from path {movedFromAssetPaths[i]}");
            }
        }
    }
}